using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Windows.Threading;
using DevExpress.Xpf.Core.Native;

namespace AsyncDataLoading {
    public abstract class AsynchronousCollectionBase : IList, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable, ILoaded {
        #region INotifyPropertyChanged
        event PropertyChangedEventHandler propertyChanged;
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }

        protected virtual void OnPropertyChanged(string propertyName) {
            if(propertyChanged != null) propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged;
        #region INotifyCollectionChanged
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void RaiseCollectionAdd(object item, int index) {
            if(CollectionChanged == null) return;
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
            CollectionChanged(this, args);
        }
        protected virtual void RaiseCollectionRemove(object item, int index) {
            if(CollectionChanged == null) return;
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
            CollectionChanged(this, args);
        }
        protected virtual void RaiseCollectionReset() {
            if(CollectionChanged == null) return;
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged(this, args);
        }
        #endregion
        #region IList
        object IList.this[int index] {
            get { return GetItem(index); }
            set { SetItem(index, value); }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumeratorCore();
        }
        int IList.IndexOf(object value) {
            return IndexOf_Core(value);
        }
        bool IList.Contains(object value) {
            return IndexOf_Core(value) > -1;
        }
        int IList.Add(object value) {
            Insert_Core(Count, value);
            return Count;
        }
        void IList.Insert(int index, object value) {
            Insert_Core(index, value);
        }
        void IList.Remove(object value) {
            int index = IndexOf_Core(value);
            RemoveAt_Core(index);
        }
        void IList.RemoveAt(int index) {
            RemoveAt_Core(index);
        }
        #endregion
        public int RequestDataRate {
            get { return requestDataRate; }
            internal set {
                if(value <= 0) throw new ArgumentException("Value must be more than zero");
                requestDataRate = value;
            }
        }
        [Bindable(BindableSupport.Yes, BindingDirection.OneWay)]
        public int LoadedCount {
            get { return loadedItemsCount; }
            protected set {
                loadedItemsCount = value;
                OnPropertyChanged("LoadedCount");
            }
        }
        [Bindable(BindableSupport.Yes, BindingDirection.OneWay)]
        public abstract int Count { get; }
        public bool IsLoadingCompleted { get { return Count == LoadedCount && Count > 0; } }
        public bool IsReadOnly { get { return IsReadOnlyCore; } }
        public bool IsFixedSize { get { return IsReadOnlyCore; } }
        public event EventHandler LoadedItemsCountUpdated;
        public event EventHandler LoadingCompleted;

        public AsynchronousCollectionBase(RequestCount requestCount, RequestData requestData)
            : this(requestCount, requestData, null) {
        }
        public AsynchronousCollectionBase(RequestCount requestCount, RequestData requestData, SubmitChanges submitChanges) {
                Storage = new DataStorage();
                SubmitChanges = submitChanges;
                RequestData = requestData;
                RequestCount = requestCount;
        }
        public virtual void Clear() {
            LoadedCount = 0;
            RaiseSubmitChanges(TypeSubmitChanges.Clear, null, null, -1);
        }
        public virtual void Dispose() {
            LoadedCount = 0;
            Storage.Dispose();
            Storage = null;
            RequestData = null;
            RequestCount = null;
        }
        public abstract void UpdateCount();
        public abstract void UpdateData(int startIndex, int count);
        public void UpdateData() {
            UpdateData(0, Count);
        }
        public void RaiseDataChanges(int startIndex, int count) {
            if(IsReadOnlyCore)
                throw new InvalidOperationException("This is read only collection.");
            for(int i = startIndex; i < startIndex + count; i++)
                RaiseSubmitChanges(TypeSubmitChanges.Update, null, Storage[i], i);
        }
        public void RaiseDataChanges(object record) {
            RaiseSubmitChanges(TypeSubmitChanges.Update, null, record, -1);
        }
        public bool IsItemLoaded(int index) {
            if(Storage == null) return false;
            return Storage.GetIsLoadedItem(index);
        }

        protected DataStorage Storage { get; private set; }
        protected RequestData RequestData { get; private set; }
        protected RequestCount RequestCount { get; private set; }
        protected SubmitChanges SubmitChanges { get; private set; }
        protected bool IsReadOnlyCore { get { return SubmitChanges == null; } }

        protected abstract object GetItem(int index);
        protected abstract void SetItem(int index, object value);
        protected virtual void RaiseOnLoadedItemsCountUpdated() {
            if(LoadedItemsCountUpdated != null) LoadedItemsCountUpdated(this, EventArgs.Empty);
        }
        protected virtual void RaiseOnLoadingCompleted() {
            if(LoadingCompleted != null) LoadingCompleted(this, EventArgs.Empty);
        }

        protected abstract IEnumerator GetEnumeratorCore();
        protected virtual int IndexOf_Core(object value) {
            if(value == null) return -1;
            for(int i = 0; i < Count; i++)
                if(Storage[i] == value) return i;
            return -1;
        }
        protected virtual void Insert_Core(int index, object value) {
            if(IsReadOnlyCore)
                throw new InvalidOperationException("This is read only collection. To enable editing you should create a collection with submitChanges event hadler.");
            RaiseSubmitChanges(TypeSubmitChanges.Add, null, value, index);
        }
        protected virtual void RemoveAt_Core(int index) {
            if(IsReadOnlyCore)
                throw new InvalidOperationException("This is read only collection. To enable editing you should create a collection with submitChanges event hadler.");
            RaiseSubmitChanges(TypeSubmitChanges.Remove, Storage[index], null, index);
        }

        protected virtual void RaiseGetData(int skipCount, int takeCount, bool isBackgroundRequest = false) {
            int startIndex = Storage.IndexOfFirstNotInitializedItemOnInterval(skipCount, takeCount);
            int endIndex = startIndex + takeCount;
            if(endIndex > Count) endIndex = Count;
            endIndex = Storage.IndexOfFirstInitializedItemOnInterval(startIndex, endIndex - startIndex);
            skipCount = startIndex;
            takeCount = endIndex - startIndex;
            if(takeCount == 0) return;
            Storage.InitializeItemsLoading(skipCount, takeCount);
            RequestDataArgs requestArgs = new RequestDataArgs(skipCount, takeCount, isBackgroundRequest);
            RequestDataResult requestResult = new RequestDataResult(requestArgs, RequestDataFeedback);
            RequestData(requestArgs, requestResult);
        }
        protected virtual void RaiseGetCount() {
            RequestCountArgs requestArgs = new RequestCountArgs();
            RequestCountResult requestResult = new RequestCountResult(requestArgs, RequestCountFeedback);
            RequestCount(requestArgs, requestResult);
        }
        protected virtual void RaiseSubmitChanges(TypeSubmitChanges type, object oldItem, object newItem, int index) {
            SubmitChangesArgs args = new SubmitChangesArgs(TypeSubmitChanges.Add, oldItem, newItem, index);
            SubmitChangesResult result = new SubmitChangesResult(args, SubmitChangesFeedback);
            SubmitChanges(args, result);
        }

        protected abstract void RequestCountFeedback(RequestCountResult requestResult);
        protected abstract void RequestDataFeedback(RequestDataResult requestResult);
        protected abstract void SubmitChangesFeedback(SubmitChangesResult resultFeedback);

        int requestDataRate = -1;
        int loadedItemsCount = 0;

        #region NotImplementedException
        void ICollection.CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }
        bool ICollection.IsSynchronized {
            get { throw new NotImplementedException(); }
        }
        object ICollection.SyncRoot {
            get { throw new NotImplementedException(); }
        }
        #endregion
        #region inner classes
        protected class DataStorageItem {
            public object Data { get; private set; }
            public bool IsLoaded { get; private set; }
            public bool IsLoading { get; private set; }
            public void StartLoading() {
                Data = null;
                IsLoaded = false;
                IsLoading = true;
            }
            public void EndLoading(object data) {
                if(!IsLoading || IsLoaded)
                    Debug.WriteLine("Cannot finish DataItem loading process before loading process initialization. Call the StartLoading method first.");
                Data = data;
                IsLoaded = true;
                IsLoading = false;
            }
            public void Clear() {
                Data = null;
                IsLoaded = false;
                IsLoading = false;
            }
            public DataStorageItem() {
                Data = null;
                IsLoaded = false;
                IsLoading = false;
            }
        }
        protected class DataStorage : IDisposable {
            public bool IsInitialized { get; private set; }
            public bool IsCompleted { get; private set; }
            public event EventHandler OnCompleted;
            public event EventHandler OnInitialized;

            public object this[int index] {
                get { return Items[index].Data; }
                private set { Items[index].EndLoading(value); }
            }
            public int Count { get { return count; } }

            public DataStorage() {
                IsInitialized = false;
            }
            public void Dispose() {
                Uninitialize();
                if(Items != null) Items.Clear();
            }
            public void Initialize(int count) {
                if(Items != null) {
                    if(count < Count) Items.RemoveRange(count, Count - count);
                    if(count > Count)
                        for(int i = Count; i < count; i++) Items.Add(new DataStorageItem());
                    if(Items.Count != count)
                        throw new InvalidOperationException("What a fuck!!");
                }
                this.count = count;
                if(Items == null) CreateItemsList();
                IsInitialized = true;
                if(OnInitialized != null) OnInitialized(this, EventArgs.Empty);
                CheckCompleted();
            }
            public void Uninitialize() {
                if(!IsInitialized) return;
                IsCompleted = false;
                IsInitialized = false;
            }
            public void LoadData(int startIndex, IEnumerator dataEnumerator) {
                dataEnumerator.Reset();
                int index = startIndex;
                while(dataEnumerator.MoveNext()) {
                    this[index] = dataEnumerator.Current;
                    index++;
                }
                CheckCompleted();
            }
            public void UnloadData(int startIndex, int count) {
                if(startIndex + count > Count) count = Count - startIndex;
                for(int i = startIndex; i < count; i++)
                    Items[i].Clear();
                IsCompleted = false;
            }
            public bool GetIsLoadedItem(int index) {
                return Items[index].IsLoaded;
            }
            public bool GetIsLoadingItem(int index) {
                return Items[index].IsLoading;
            }
            public void InitializeItemsLoading(int index, int count) {
                for(int i = index; i < index + count; i++)
                    InitializeItemLoading(i);
            }
            public void InitializeItemLoading(int index) {
                Items[index].StartLoading();
            }
            public int IndexOfFirstInitializedItemOnInterval(int index, int count) {
                int endIndex = index + count;
                if(endIndex > Count) endIndex = Count;
                for(int i = index; i < endIndex; i++)
                    if(Items[i].IsLoaded || Items[i].IsLoading) return i;
                return endIndex;
            }
            public int IndexOfFirstNotInitializedItemOnInterval(int index, int count) {
                int endIndex = index + count;
                if(endIndex > Count) endIndex = Count;
                for(int i = index; i < index + count; i++)
                    if(!Items[i].IsLoaded || !Items[i].IsLoading) return i;
                return Count;
            }
            public int IndexOfFirstNotLoadedItem() {
                for(int i = 0; i < Count; i++)
                    if(!Items[i].IsLoaded) return i;
                return Count;
            }
            public int GetLoadedItemsCount() {
                int res = 0;
                for(int i = 0; i < Count; i++)
                    if(GetIsLoadedItem(i)) res++;
                return res;
            }
            void CheckCompleted() {
                for(int i = 0; i < Count; i++)
                    if(!GetIsLoadedItem(i)) return;
                IsCompleted = true;
                if(OnCompleted != null) OnCompleted(this, EventArgs.Empty);
            }
            void CreateItemsList() {
                Items = new List<DataStorageItem>();
                for(int i = 0; i < count; i++)
                    Items.Add(new DataStorageItem());
            }

            List<DataStorageItem> Items;
            int count;
        }
        protected class EnumeratorBase : IEnumerator {
            public EnumeratorBase(AsynchronousCollectionBase owner) {
                this.owner = owner;
                this.index = -1;
                current = null;
            }
            public void Dispose() {
                current = null;
                owner = null;
            }
            object IEnumerator.Current {
                get { return current; }
            }
            public bool MoveNext() {
                index++;
                IList listOwner = ((IList)owner);
                if(index < owner.Count) {
                    current = listOwner[index];
                    return true;
                }
                current = null;
                return false;
            }
            public void Reset() {
                index = -1;
                current = null;
            }

            AsynchronousCollectionBase owner;
            int index;
            object current;
        }
        #endregion
    }
    
    /// <summary>
    /// Represents a collection of objects that can be asynchronously loaded on demand.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsynchronousCollection<T> : AsynchronousCollectionBase, IList<T> {
        public override int Count {
            get { return Storage != null ? Storage.Count : 0; }
        }
        public T this[int index] {
            get {
                IList me = (IList)this;
                if(me[index] == null) return default(T);
                return (T)me[index];
            }
            set {
                IList me = (IList)this;
                me[index] = value;
            }
        }

        public AsynchronousCollection(RequestCount requestCount, RequestData requestData)
            : this(requestCount, requestData, null, AsynchronousCollectionSettings.Default) {
        }
        public AsynchronousCollection(RequestCount requestCount, RequestData requestData, AsynchronousCollectionSettings settings)
            : this(requestCount, requestData, null, settings) {
        }
        public AsynchronousCollection(RequestCount requestCount, RequestData requestData, SubmitChanges submitChanges)
            : this(requestCount, requestData, submitChanges, AsynchronousCollectionSettings.Default) {
        }
        public AsynchronousCollection(RequestCount requestCount, RequestData requestData, SubmitChanges submitChanges, AsynchronousCollectionSettings settings) :
            base(requestCount, requestData, submitChanges) {
            settings.Apply<T>(this);
            Storage.OnInitialized += OnStorageInitialized;
            Storage.OnCompleted += OnStorageCompleted;
            RaiseGetCount();
        }
        public override void Dispose() {
            Storage.OnInitialized -= OnStorageInitialized;
            Storage.OnCompleted -= OnStorageCompleted;
            base.Dispose();
        }
        public override void UpdateCount() {
            RaiseGetCount();
            UpdateLoadedItemsCount();
        }
        public override void UpdateData(int startIndex, int count) {
            Storage.UnloadData(startIndex, count);
            UpdateLoadedItemsCount();
            for(int i = startIndex; i < startIndex + count; i++)
                RaiseCollectionRemove(this[i], i);
        }
        public IEnumerator<T> GetEnumerator() {
            return (IEnumerator<T>)GetEnumeratorCore();
        }
        public int IndexOf(T item) {
            IList me = this;
            return me.IndexOf(item);
        }
        public void Add(T item) {
            IList me = this;
            me.Add(item);
        }
        public bool Contains(T item) {
            IList me = this;
            return me.Contains(item);
        }
        public void Insert(int index, T item) {
            IList me = this;
            me.Insert(index, item);
        }
        public bool Remove(T item) {
            if(Contains(item)) {
                IList me = this;
                me.Remove(item);
                return true;
            }
            return false;
        }
        public void RemoveAt(int index) {
            IList me = this;
            me.RemoveAt(index);
        }
        
        protected override IEnumerator GetEnumeratorCore() {
            return new Enumerator(this);
        }
        protected override object GetItem(int index) {
            if(!Storage.GetIsLoadedItem(index)) {
                if(Storage.GetIsLoadingItem(index))
                    return default(T);
                int interval = index + RequestDataRate < Count ? RequestDataRate : Count - index;
                int endIndex = Storage.IndexOfFirstInitializedItemOnInterval(index, interval);
                RaiseGetData(index, endIndex - index);
                return default(T);
            }
            if(Storage[index] == null) return default(T);
            return (T)Storage[index];
        }
        protected override void SetItem(int index, object value) {
            if(IsReadOnlyCore)
                throw new InvalidOperationException("This is read only collection.");
            RaiseSubmitChanges(TypeSubmitChanges.Replace, Storage[index], value, index);
        }

        protected virtual void OnStorageInitialized(object sender, EventArgs e) {
            UpdateLoadedItemsCount();
            OnPropertyChanged("Count");
        }
        protected virtual void OnStorageCompleted(object sender, EventArgs e) { }

        protected virtual void Initialize(int count) {
            if(Storage == null) return;
            Storage.Uninitialize();
            Storage.Initialize(count);
            RaiseCollectionReset();
        }

        protected override void RequestCountFeedback(RequestCountResult requestResult) {
            Initialize(requestResult.Count);
        }
        protected override void RequestDataFeedback(RequestDataResult requestResult) {
            LoadData(((RequestDataArgs)requestResult.RequestArgs).SkipCount, requestResult.Data.GetEnumerator());
        }
        protected override void SubmitChangesFeedback(SubmitChangesResult resultFeedback) {
            if(resultFeedback.Cancel) return;
            SubmitChangesArgs args = (SubmitChangesArgs)resultFeedback.RequestArgs;
            UpdateCount();
            if(args.Type == TypeSubmitChanges.Replace || args.Type == TypeSubmitChanges.Update)
                UpdateData(args.Index, 1);
        }
        
        protected virtual void LoadData(int startIndex, IEnumerator dataEnumerator) {
            if(Storage == null) return;
            List<object> data = new List<object>();
            dataEnumerator.Reset();
            while(dataEnumerator.MoveNext()) data.Add(dataEnumerator.Current);
            Storage.LoadData(startIndex, dataEnumerator);
            RaiseCollectionAdd(data, startIndex);
            UpdateLoadedItemsCount();
        }
        protected virtual void UpdateLoadedItemsCount() {
            LoadedCount = Storage.GetLoadedItemsCount();
            RaiseOnLoadedItemsCountUpdated();
            if(Storage.IsCompleted) RaiseOnLoadingCompleted();
        }

        #region NotImplementedException
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }
        #endregion
        #region inner classes
        protected class Enumerator : EnumeratorBase, IEnumerator<T> {
            public Enumerator(AsynchronousCollectionBase owner) : base(owner) { }
            public T Current {
                get {
                    IEnumerator me = this;
                    if(me.Current == null) return default(T);
                    return (T)me.Current; 
                }
            }
        }
        #endregion
    }
    
    /// <summary>
    /// Represents a collection of objects that can be asynchronously loaded in background thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsynchronousCollection2<T> : AsynchronousCollection<T> {
        public int BackgroundRequestDataRate {
            get { return backgroundRequestDataRate; }
            set {
                if(value <= 0) throw new ArgumentException("Value must be more than zero");
                backgroundRequestDataRate = value;
            }
        }
        int backgroundRequestDataRate = -1;
        public TimeSpan BackgroundRequestDataInterval {
            get { return backgroundRequestDataInterval; }
            set { backgroundRequestDataInterval = value; }
        }
        TimeSpan backgroundRequestDataInterval = TimeSpan.Zero;

        protected int requestDataCounter = 0;
        protected DispatcherTimer RequestDataTimer { get; private set; }


        public AsynchronousCollection2(RequestCount requestCount, RequestData requestData)
            : this(requestCount, requestData, null, AsynchronousCollection2Settings.Default) {
        }
        public AsynchronousCollection2(RequestCount requestCount, RequestData requestData, AsynchronousCollection2Settings setting)
            : this(requestCount, requestData, null, setting) {
        }
        public AsynchronousCollection2(RequestCount requestCount, RequestData requestData, SubmitChanges submitChanges)
            : this(requestCount, requestData, submitChanges, AsynchronousCollection2Settings.Default) {
        }
        public AsynchronousCollection2(RequestCount requestCount, RequestData requestData, SubmitChanges submitChanges, AsynchronousCollection2Settings setting)
            : base(requestCount, requestData, submitChanges, setting) {
            RequestDataTimer = new DispatcherTimer();
        }

        public override void Dispose() {
            DisableRequestDataTimer();
            base.Dispose();
            RequestDataTimer = null;
        }
        public override void UpdateData(int startIndex, int count) {
            base.UpdateData(startIndex, count);
            if(!Storage.IsCompleted && !RequestDataTimer.IsEnabled)
                EnableRequestDataTimer();
        }
        protected virtual void CustomizeRequestDataTimer() {
            RequestDataTimer.Interval = BackgroundRequestDataInterval;
        }
        protected override void OnStorageInitialized(object sender, EventArgs e) {
            base.OnStorageInitialized(sender, e);
            EnableRequestDataTimer();
        }
        protected override void OnStorageCompleted(object sender, EventArgs e) {
            DisableRequestDataTimer();
        }
        protected virtual void EnableRequestDataTimer() {
            CustomizeRequestDataTimer();
            RequestDataTimer.Tick += OnRequestDataTimerTick;
            RequestDataTimer.Start();
        }
        protected virtual void DisableRequestDataTimer() {
            RequestDataTimer.Stop();
            RequestDataTimer.Tick -= OnRequestDataTimerTick;
        }
        protected virtual void OnRequestDataTimerTick(object sender, EventArgs e) {
            if(requestDataCounter > 0 || !Storage.IsInitialized) return;
            int skipCount = Storage.IndexOfFirstNotLoadedItem();
            int endIndex = Storage.IndexOfFirstInitializedItemOnInterval(skipCount, BackgroundRequestDataRate);
            RaiseGetData(skipCount, endIndex - skipCount, true);
        }
        protected override void RaiseGetData(int skipCount, int takeCount, bool isBackgroundRequest = false) {
            requestDataCounter++;
            base.RaiseGetData(skipCount, takeCount);
        }
        protected override void RequestDataFeedback(RequestDataResult requestResult) {
            base.RequestDataFeedback(requestResult);
            requestDataCounter--;
        }
    }
}