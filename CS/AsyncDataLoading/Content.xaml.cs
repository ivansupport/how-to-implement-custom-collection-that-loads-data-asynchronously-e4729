using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Editors;
using System.Collections;
namespace AsyncDataLoading {
    /// <summary>
    /// Interaction logic for Content.xaml
    /// </summary>
    public partial class Content : UserControl {
        public static readonly DependencyProperty DataSourceManagerProperty =
            DependencyProperty.Register("DataSourceManager", typeof(DataSourceManager), typeof(Content),
            new PropertyMetadata(null, (d, e) => ((Content)d).OnDataSourceManagerChanged((DataSourceManager)e.OldValue)));
        public static readonly DependencyProperty IsSettingsEnabledProperty =
            DependencyProperty.Register("IsSettingsEnabled", typeof(bool), typeof(Content), new PropertyMetadata(true));
        public DataSourceManager DataSourceManager {
            get { return (DataSourceManager)GetValue(DataSourceManagerProperty); }
            set { SetValue(DataSourceManagerProperty, value); }
        }
        public bool IsSettingsEnabled {
            get { return (bool)GetValue(IsSettingsEnabledProperty); }
            set { SetValue(IsSettingsEnabledProperty, value); }
        }

        public Content() {
            InitializeComponent();
        }
        void OnDataSourceManagerChanged(DataSourceManager oldManager) {
            if(DataSourceManager != null) {
                DataSourceManager.LoadingCompleted += OnLoadingCompleted;
                DataSourceManager.DataSourceUninitialized += OnDataSourceUninitialized;
            }
            if(oldManager != null) {
                oldManager.LoadingCompleted -= OnLoadingCompleted;
                oldManager.DataSourceUninitialized -= OnDataSourceUninitialized;
            }
            
        }
        void OnDataSourceUninitialized(DataSourceManagerBase sender, AsynchronousCollectionBase oldDataSource) {
            SetIsSettingsEnabled(false);
        }
        void OnLoadingCompleted(DataSourceManagerBase sender, AsynchronousCollectionBase dataSource) {
            SetIsSettingsEnabled(true);
        }
        void SetIsSettingsEnabled(bool value) {
            //IsSettingsEnabled = value;
        }
        void ThemeButtonClick(object sender, RoutedEventArgs e) {
            ApplicationThemeHelper.ApplicationThemeName = ((Button)sender).Content.ToString();
        }
        void ApplySettingsButtonClick(object sender, RoutedEventArgs e) {
            AsynchronousCollectionSettings sett;
            if(sett1Check.IsChecked.Value) sett = AsynchronousCollectionSettingsFactory.CreateOnDemandRequestDataModeSettings((int)requestDataRateEd.Value);
            else sett = AsynchronousCollectionSettingsFactory.CreateInBackgroundThreadRequestDataModeSettings((int)requestDataRateEd.Value);
            DataSourceManager.Uninitialize();
            DataSourceManager.Settings = sett;
            DataSourceManager.Initialize();
        }
    }
}
