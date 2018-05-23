using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;
using System.Collections;
using System.ServiceModel;
using System.Windows;
namespace AsyncDataLoading {
    public class DataSourceManager : DataSourceManagerBase {
        public static readonly DependencyProperty EntityTypeProperty =
            DependencyProperty.Register("EntityType", typeof(Type), typeof(DataSourceManager),
            new UIPropertyMetadata((d, e) => ((DataSourceManager)d).OnEntityTypeChanged()));
        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(int), typeof(DataSourceManager), new PropertyMetadata(0));
        public static readonly DependencyProperty LoadedCountProperty =
            DependencyProperty.Register("LoadedCount", typeof(int), typeof(DataSourceManager), new PropertyMetadata(0));
        public static readonly DependencyProperty AutoInitializeProperty =
            DependencyProperty.Register("AutoInitialize", typeof(bool), typeof(DataSourceManager),
            new PropertyMetadata(true));
        public event RequestCount RequestCount;
        public event RequestData RequestData;
        public event SubmitChanges SubmitChanges;
        public Type EntityType {
            get { return (Type)GetValue(EntityTypeProperty); }
            set { SetValue(EntityTypeProperty, value); }
        }
        public int Count {
            get { return (int)GetValue(CountProperty); }
            private set { SetValue(CountProperty, value); }
        }
        public int LoadedCount {
            get { return (int)GetValue(LoadedCountProperty); }
            private set { SetValue(LoadedCountProperty, value); }
        }
        public bool AutoInitialize {
            get { return (bool)GetValue(AutoInitializeProperty); }
            set { SetValue(AutoInitializeProperty, value); }
        }

        public DataSourceManager()
            : base() {
            DefaultStyleKey = typeof(DataSourceManager);
        }
        public override void Initialize() {
            if(IsDataSourceInitialized) return;
            InitializeCore();
        }
        public override void Uninitialize() {
            if(!IsDataSourceInitialized) return;
            if(DataSource != null)
                DataSource.Dispose();
            SetDataSource(null);
        }

        protected virtual void OnEntityTypeChanged() {
            if(IsDataSourceInitialized) throw new InvalidOperationException("Initialization has completed. Cannot set the EntityType property after initialization.");
            TryInitialize();
        }
        protected override void OnSettingsChanged() {
            base.OnSettingsChanged();
            TryInitialize();
        }
        protected override void SetDataSource(AsynchronousCollectionBase dataSource) {
            if(DataSource != null)
                ((INotifyPropertyChanged)DataSource).PropertyChanged -= OnDataSourcePropertyChanged;
            base.SetDataSource(dataSource);
            if(DataSource != null)
                ((INotifyPropertyChanged)DataSource).PropertyChanged += OnDataSourcePropertyChanged;
        }
        void OnDataSourcePropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == "Count" || e.PropertyName == "LoadedCount") {
                Count = ((AsynchronousCollectionBase)sender).Count;
                LoadedCount = ((AsynchronousCollectionBase)sender).LoadedCount;
            }
        }

        void TryInitialize() {
            if(IsDataSourceInitialized) return;
            if(AutoInitialize) InitializeCore();
        }
        void InitializeCore() {
            if(EntityType == null || Settings == null) return;
            if(RequestCount == null || RequestData == null) return;
            Type[] createCollectionMethodArgs = new Type[] { typeof(RequestCount), typeof(RequestData), typeof(SubmitChanges), typeof(AsynchronousCollectionSettings) };
            MethodInfo createCollectionMethod = typeof(AsynchronousCollectionFactory).GetMethod("CreateCollection", createCollectionMethodArgs);
            createCollectionMethod = createCollectionMethod.MakeGenericMethod(new Type[] { EntityType });
            object col;
            if(SubmitChanges != null)
                col = createCollectionMethod.Invoke(null, new object[] { new RequestCount(OnRequestCount), new RequestData(OnRequestData), new SubmitChanges(OnSubmitChanges), Settings });
            else
                col = createCollectionMethod.Invoke(null, new object[] { new RequestCount(OnRequestCount), new RequestData(OnRequestData), null, Settings });
            SetDataSource((AsynchronousCollectionBase)col);
        }

        void OnRequestCount(RequestCountArgs request, RequestCountResult feedback) {
            if(RequestCount != null) RequestCount(request, feedback);
        }
        void OnRequestData(RequestDataArgs request, RequestDataResult feedback) {
            if(RequestData != null) RequestData(request, feedback);
        }
        void OnSubmitChanges(SubmitChangesArgs args, SubmitChangesResult feedback) {
            SubmitChanges(args, feedback);
        }
    }
}