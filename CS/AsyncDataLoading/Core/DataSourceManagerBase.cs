using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
namespace AsyncDataLoading {
    public delegate void DataSourceInitializedEventHander(DataSourceManagerBase sender, AsynchronousCollectionBase newDataSource);
    public delegate void DataSourceUninitializedEventHander(DataSourceManagerBase sender, AsynchronousCollectionBase oldDataSource);
    public delegate void DataSourceLoadingCompleted(DataSourceManagerBase sender, AsynchronousCollectionBase dataSource);
    [Browsable(false)]
    public abstract class DataSourceManagerBase : Control, INotifyPropertyChanged {
        AsynchronousCollectionBase dataSource = null;

        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(AsynchronousCollectionSettings), typeof(DataSourceManagerBase),
            new PropertyMetadata((d, e) => ((DataSourceManagerBase)d).OnSettingsChanged()));
        public static readonly DependencyProperty IsDataSourceInitializedProperty =
            DependencyProperty.Register("IsDataSourceInitialized", typeof(bool), typeof(DataSourceManagerBase),
            new PropertyMetadata(false, (d, e) => ((DataSourceManagerBase)d).OnIsDataSourceInitializedChanged()));
        public AsynchronousCollectionSettings Settings {
            get { return (AsynchronousCollectionSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }
        public bool IsDataSourceInitialized {
            get { return (bool)GetValue(IsDataSourceInitializedProperty); }
            private set { SetValue(IsDataSourceInitializedProperty, value); }
        }
        public AsynchronousCollectionBase DataSource {
            get { return GetDataSource(); }
            private set {
                dataSource = value;
                OnPropertyChanged("DataSource");
            }
        }
        public event DataSourceInitializedEventHander DataSourceInitialized;
        public event DataSourceUninitializedEventHander DataSourceUninitialized;
        public event DataSourceLoadingCompleted LoadingCompleted;
        public bool IsLoadingCompleted { get { return DataSource != null ? DataSource.IsLoadingCompleted : false; } }

        public DataSourceManagerBase() {
            Settings = AsynchronousCollectionSettingsFactory.CreateOnDemandRequestDataModeSettings();
        }
        public abstract void Initialize();
        public abstract void Uninitialize();
        public void UpdateCount() {
            if(DataSource != null)
                DataSource.UpdateCount();
        }
        public void UpdateData() {
            if(DataSource != null)
                DataSource.UpdateData();
        }
        public void UpdateData(int skipCount, int takeCount) {
            if(DataSource != null)
                DataSource.UpdateData(skipCount, takeCount);
        }
        public void RaiseDataChanges(int skipCount, int takeCount) {
            if(DataSource != null)
                DataSource.RaiseDataChanges(skipCount, takeCount);
        }
        public void RaiseDataChanges(object record) {
            if(DataSource != null)
                DataSource.RaiseDataChanges(record);
        }

        protected virtual void OnSettingsChanged() {
            if(IsDataSourceInitialized) Uninitialize();
            Initialize();
        }
        protected virtual void OnIsDataSourceInitializedChanged() {
        }
        protected virtual void OnPropertyChanged(string propertyName) {
            if(propertyChanged != null) propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual AsynchronousCollectionBase GetDataSource() {
            return dataSource;
        }
        protected virtual void SetDataSource(AsynchronousCollectionBase dataSource) {
            AsynchronousCollectionBase oldDataSource = DataSource;
            AsynchronousCollectionBase newDataSource = dataSource;
            DataSource = dataSource;
            IsDataSourceInitialized = DataSource != null;
            if(oldDataSource != null) {
                oldDataSource.LoadingCompleted -= OnDataSourceOnLoadingCompleted;
            }
            if(newDataSource != null) {
                newDataSource.LoadingCompleted += OnDataSourceOnLoadingCompleted;
                if(newDataSource.IsLoadingCompleted) OnDataSourceOnLoadingCompleted(newDataSource, EventArgs.Empty);
            }
            if(IsDataSourceInitialized) {
                if(DataSourceInitialized != null) DataSourceInitialized(this, newDataSource);
            } else {
                if(DataSourceUninitialized != null) DataSourceUninitialized(this, oldDataSource);
            }
        }
        void OnDataSourceOnLoadingCompleted(object sender, EventArgs e) {
            if(LoadingCompleted != null) LoadingCompleted(this, (AsynchronousCollectionBase)sender);
        }

        event PropertyChangedEventHandler propertyChanged;
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }
    }
}