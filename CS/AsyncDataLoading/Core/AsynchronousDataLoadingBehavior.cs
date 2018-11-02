using System.Windows;
using System;
using System.Windows.Data;
using DevExpress.Xpf.Grid;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Utils;

namespace AsyncDataLoading {
    public class AsynchronousDataLoadingBehavior : Behavior<GridControl> {
        #region DP
        public static readonly DependencyProperty AutoDisableGridControlProperty =
            DependencyProperty.Register("AutoDisableGridControl", typeof(bool), typeof(AsynchronousDataLoadingBehavior), new PropertyMetadata(true));
        public static readonly DependencyProperty NeverEnableGridControlProperty =
            DependencyProperty.Register("NeverEnableGridControl", typeof(bool), typeof(AsynchronousDataLoadingBehavior), new PropertyMetadata(false));
        public static readonly DependencyProperty DataSourceManagerProperty =
            DependencyProperty.Register("DataSourceManager", typeof(DataSourceManager), typeof(AsynchronousDataLoadingBehavior),
            new PropertyMetadata(null, (d, e) => ((AsynchronousDataLoadingBehavior)d).OnDataSourceManagerChanged((DataSourceManager)e.OldValue)));
        public static readonly DependencyProperty AllowEditingProperty =
            DependencyProperty.Register("AllowEditing", typeof(bool), typeof(AsynchronousDataLoadingBehavior),
            new PropertyMetadata(true, (d, e) => ((AsynchronousDataLoadingBehavior)d).OnAllowEditingChanged()));
        public static readonly DependencyProperty AllowGroupingProperty =
            DependencyProperty.Register("AllowGrouping", typeof(bool), typeof(AsynchronousDataLoadingBehavior),
            new PropertyMetadata(true, (d, e) => ((AsynchronousDataLoadingBehavior)d).OnAllowGroupingChanged()));
        public static readonly DependencyProperty AllowSortingProperty =
            DependencyProperty.Register("AllowSorting", typeof(bool), typeof(AsynchronousDataLoadingBehavior),
            new PropertyMetadata(true, (d, e) => ((AsynchronousDataLoadingBehavior)d).OnAllowSortingChanged()));
        public static readonly DependencyProperty AllowColumnFilteringProperty =
            DependencyProperty.Register("AllowColumnFiltering", typeof(bool), typeof(AsynchronousDataLoadingBehavior),
            new PropertyMetadata(true, (d, e) => ((AsynchronousDataLoadingBehavior)d).OnAllowColumnFilteringChanged()));
        public static readonly DependencyProperty AllowFilterEditorProperty =
            DependencyProperty.Register("AllowFilterEditor", typeof(DefaultBoolean), typeof(AsynchronousDataLoadingBehavior),
            new PropertyMetadata(true, (d, e) => ((AsynchronousDataLoadingBehavior)d).OnAllowFilterEditorChanged()));
        public static readonly DependencyProperty ShowTotalSummaryProperty =
            DependencyProperty.Register("ShowTotalSummary", typeof(bool), typeof(AsynchronousDataLoadingBehavior),
            new PropertyMetadata(false, (d, e) => ((AsynchronousDataLoadingBehavior)d).OnShowTotalSummaryChanged()));
        public static readonly DependencyProperty ShowFilterPanelModeProperty =
            DependencyProperty.Register("ShowFilterPanelMode", typeof(ShowFilterPanelMode), typeof(AsynchronousDataLoadingBehavior),
            new PropertyMetadata(ShowFilterPanelMode.Default, (d, e) => ((AsynchronousDataLoadingBehavior)d).OnShowFilterPanelModeChanged()));

        public static readonly DependencyProperty GroupIndexProperty =
            DependencyProperty.RegisterAttached("GroupIndex", typeof(int), typeof(AsynchronousDataLoadingBehavior),
            new PropertyMetadata(-1, new PropertyChangedCallback(OnGroupIndexChanged)));
        public static readonly DependencyProperty SortIndexProperty =
            DependencyProperty.RegisterAttached("SortIndex", typeof(int), typeof(AsynchronousDataLoadingBehavior),
            new PropertyMetadata(-1, new PropertyChangedCallback(OnSortIndexChanged)));
        public static int GetGroupIndex(GridColumn obj) {
            return (int)obj.GetValue(GroupIndexProperty);
        }
        public static void SetGroupIndex(GridColumn obj, int value) {
            obj.SetValue(GroupIndexProperty, value);
        }
        public static int GetSortIndex(GridColumn obj) {
            return (int)obj.GetValue(SortIndexProperty);
        }
        public static void SetSortIndex(GridColumn obj, int value) {
            obj.SetValue(SortIndexProperty, value);
        }
        static void OnGroupIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            GridColumn column = (GridColumn)obj;
            if(column.View == null) return;
            GridControl grid = (GridControl)column.View.DataControl;
            AsynchronousDataLoadingBehavior bhvr = GetAsynchronousDataLoadingBehavior(grid);
            if(!bhvr.IsGridEnabled) return;
            column.GroupIndex = (int)e.NewValue;
        }
        static void OnSortIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            GridColumn column = (GridColumn)obj;
            if(column.View == null) return;
            GridControl grid = (GridControl)column.View.DataControl;
            AsynchronousDataLoadingBehavior bhvr = GetAsynchronousDataLoadingBehavior(grid);
            if(!bhvr.IsGridEnabled) return;
            column.SortIndex = (int)e.NewValue;
        }

        static readonly DependencyProperty AsynchronousDataLoadingBehaviorProperty =
            DependencyProperty.RegisterAttached("AsynchronousDataLoadingBehavior", typeof(AsynchronousDataLoadingBehavior), typeof(AsynchronousDataLoadingBehavior), new PropertyMetadata(null));
        static AsynchronousDataLoadingBehavior GetAsynchronousDataLoadingBehavior(GridControl obj) {
            return (AsynchronousDataLoadingBehavior)obj.GetValue(AsynchronousDataLoadingBehaviorProperty);
        }
        static void SetAsynchronousDataLoadingBehavior(GridControl obj, AsynchronousDataLoadingBehavior value) {
            obj.SetValue(AsynchronousDataLoadingBehaviorProperty, value);
        }
        #endregion
        public bool AutoDisableGridControl {
            get { return (bool)GetValue(AutoDisableGridControlProperty); }
            set { SetValue(AutoDisableGridControlProperty, value); }
        }
        public bool NeverEnableGridControl {
            get { return (bool)GetValue(NeverEnableGridControlProperty); }
            set { SetValue(NeverEnableGridControlProperty, value); }
        }
        public DataSourceManager DataSourceManager {
            get { return (DataSourceManager)GetValue(DataSourceManagerProperty); }
            set { SetValue(DataSourceManagerProperty, value); }
        }
        public bool AllowEditing {
            get { return (bool)GetValue(AllowEditingProperty); }
            set { SetValue(AllowEditingProperty, value); }
        }
        public bool AllowGrouping {
            get { return (bool)GetValue(AllowGroupingProperty); }
            set { SetValue(AllowGroupingProperty, value); }
        }
        public bool AllowSorting {
            get { return (bool)GetValue(AllowSortingProperty); }
            set { SetValue(AllowSortingProperty, value); }
        }
        public bool AllowColumnFiltering {
            get { return (bool)GetValue(AllowColumnFilteringProperty); }
            set { SetValue(AllowColumnFilteringProperty, value); }
        }
        public DefaultBoolean AllowFilterEditor {
            get { return (DefaultBoolean)GetValue(AllowFilterEditorProperty); }
            set { SetValue(AllowFilterEditorProperty, value); }
        }
        public bool ShowTotalSummary {
            get { return (bool)GetValue(ShowTotalSummaryProperty); }
            set { SetValue(ShowTotalSummaryProperty, value); }
        }
        public ShowFilterPanelMode ShowFilterPanelMode {
            get { return (ShowFilterPanelMode)GetValue(ShowFilterPanelModeProperty); }
            set { SetValue(ShowFilterPanelModeProperty, value); }
        }

        public DataViewBase GridView { get { return Grid != null ? Grid.View : null; } }
        public GridControl Grid { get { return AssociatedObject; } }

        bool _IsAttached = false;
        bool IsGridEnabled = true;
        bool IsFirstAttaching = true;
        
        void OnAllowEditingChanged() {
            if(!IsGridEnabled || GridView == null) return;
            GridView.AllowEditing = AllowEditing;
        }
        void OnAllowGroupingChanged() {
            if(!IsGridEnabled || GridView == null) return;
            if(GridView is TableView) ((TableView)GridView).AllowGrouping = AllowGrouping;
        }
        void OnAllowSortingChanged() {
            if(!IsGridEnabled || GridView == null) return;
            GridView.AllowSorting = AllowSorting;
        }
        void OnAllowColumnFilteringChanged() {
            if(!IsGridEnabled || GridView == null) return;
            GridView.AllowColumnFiltering = AllowColumnFiltering;
        }
        void OnAllowFilterEditorChanged() {
            if(!IsGridEnabled || GridView == null) return;
            GridView.AllowFilterEditor = AllowFilterEditor;
        }
        void OnShowTotalSummaryChanged() {
            if(!IsGridEnabled || GridView == null) return;
            GridView.ShowTotalSummary = ShowTotalSummary;
        }
        void OnShowFilterPanelModeChanged() {
            if(!IsGridEnabled || GridView == null) return;
            GridView.ShowFilterPanelMode = ShowFilterPanelMode;
        }

        protected override void OnAttached() {
            IsFirstAttaching = true;
            base.OnAttached();
            SetAsynchronousDataLoadingBehavior(AssociatedObject, this);
            if(GridView == null) {
                Grid.Loaded += OnGridLoaded;
                return;
            }
            TryAttach();
        }
        protected override void OnDetaching() {
            SetAsynchronousDataLoadingBehavior(AssociatedObject, null);
            Grid.Loaded -= OnGridLoaded;
            TryDetach();
            base.OnDetaching();
        }
        void OnDataSourceManagerChanged(DataSourceManager oldManager) {
            if(oldManager != null)
                throw new InvalidOperationException("The OnDataSourceManager property cannot be initialized twice.");
            TryAttach();
        }
        void OnGridLoaded(object sender, RoutedEventArgs e) {
            ((GridControl)sender).Loaded -= OnGridLoaded;
            TryAttach();
        }
        void TryAttach() {
            if(DataSourceManager == null || Grid == null || GridView == null) return;
            if(_IsAttached) return;
            _IsAttached = true;
            BindingOperations.SetBinding(Grid, GridControl.ItemsSourceProperty, new Binding("DataSource") { Source = DataSourceManager });
            DataSourceManager.LoadingCompleted += OnLoadingCompleted;
            DataSourceManager.DataSourceInitialized += OnDataSourceManagerDataSourceInitialized;
            DataSourceManager.DataSourceUninitialized += OnDataSourceManagerDataSourceUninitialized;
            if(DataSourceManager.IsDataSourceInitialized) DisableGrid();
            if(DataSourceManager.IsLoadingCompleted) EnableGrid();
        }
        void TryDetach() {
            if(DataSourceManager == null || Grid == null || GridView == null) return;
            if(!_IsAttached) return;
            _IsAttached = false;
            Grid.ClearValue(GridControl.ItemsSourceProperty);
            EnableGrid();
            DataSourceManager.LoadingCompleted -= OnLoadingCompleted;
            DataSourceManager.DataSourceInitialized -= OnDataSourceManagerDataSourceInitialized;
            DataSourceManager.DataSourceUninitialized -= OnDataSourceManagerDataSourceUninitialized;
        }

        void OnDataSourceManagerDataSourceInitialized(DataSourceManagerBase sender, AsynchronousCollectionBase oldDataSource) {
            DisableGrid();
        }
        void OnDataSourceManagerDataSourceUninitialized(DataSourceManagerBase sender, AsynchronousCollectionBase oldDataSource) {
            DisableGrid();
        }
        void OnLoadingCompleted(DataSourceManagerBase sender, AsynchronousCollectionBase dataSource) {
            EnableGrid();
        }

        void DisableGrid() {
            IsGridEnabled = false;
            SetIsEnabledGridCore(false);
            if(!IsFirstAttaching)
                StoreColumnsProperties();
            if(AutoDisableGridControl) {
                Grid.ClearSorting();
                Grid.ClearGrouping();
            }
            IsFirstAttaching = false;
        }
        void EnableGrid() {
            if(NeverEnableGridControl)
                return;
            IsGridEnabled = true;
            SetIsEnabledGridCore(true);
            RestoreColumnsProperties();
        }
        void SetIsEnabledGridCore(bool isEnabled) {
            TableView tableView = GridView as TableView;
            bool isEn = isEnabled || !AutoDisableGridControl;
            GridView.AllowEditing = isEn ? AllowEditing : false;
            GridView.AllowSorting = isEn ? AllowSorting : false;
            GridView.AllowColumnFiltering = isEn ? AllowColumnFiltering : false;
            GridView.AllowFilterEditor = isEn ? AllowFilterEditor : DefaultBoolean.False;
            GridView.ShowTotalSummary = isEn ? ShowTotalSummary : false;
            GridView.ShowFilterPanelMode = isEn ? ShowFilterPanelMode : ShowFilterPanelMode.Never;
            if(tableView != null) {
                tableView.AllowGrouping = isEn ? AllowGrouping : false;
            }
        }

        void StoreColumnsProperties() {
            foreach(GridColumn column in Grid.Columns) {
                SetGroupIndex(column, column.GroupIndex);
                SetSortIndex(column, column.SortIndex);
            }
        }
        void RestoreColumnsProperties() {
            if(!AutoDisableGridControl) { 
                StoreColumnsProperties();
                return;
            }
            foreach(GridColumn column in Grid.Columns) {
                column.GroupIndex = GetGroupIndex(column);
                column.SortIndex = GetSortIndex(column);
            }
        }
    }
}