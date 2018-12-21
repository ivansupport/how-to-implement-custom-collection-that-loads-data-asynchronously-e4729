Imports System.Windows
Imports System
Imports System.Windows.Data
Imports DevExpress.Xpf.Grid
Imports DevExpress.Mvvm.UI.Interactivity
Imports DevExpress.Utils

Namespace AsyncDataLoading
    Public Class AsynchronousDataLoadingBehavior
        Inherits Behavior(Of GridControl)

        #Region "DP"
        Public Shared ReadOnly AutoDisableGridControlProperty As DependencyProperty = DependencyProperty.Register("AutoDisableGridControl", GetType(Boolean), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(True))
        Public Shared ReadOnly NeverEnableGridControlProperty As DependencyProperty = DependencyProperty.Register("NeverEnableGridControl", GetType(Boolean), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(False))
        Public Shared ReadOnly DataSourceManagerProperty As DependencyProperty = DependencyProperty.Register("DataSourceManager", GetType(DataSourceManager), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(Nothing, Sub(d, e) CType(d, AsynchronousDataLoadingBehavior).OnDataSourceManagerChanged(CType(e.OldValue, DataSourceManager))))
        Public Shared ReadOnly AllowEditingProperty As DependencyProperty = DependencyProperty.Register("AllowEditing", GetType(Boolean), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(True, Sub(d, e) CType(d, AsynchronousDataLoadingBehavior).OnAllowEditingChanged()))
        Public Shared ReadOnly AllowGroupingProperty As DependencyProperty = DependencyProperty.Register("AllowGrouping", GetType(Boolean), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(True, Sub(d, e) CType(d, AsynchronousDataLoadingBehavior).OnAllowGroupingChanged()))
        Public Shared ReadOnly AllowSortingProperty As DependencyProperty = DependencyProperty.Register("AllowSorting", GetType(Boolean), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(True, Sub(d, e) CType(d, AsynchronousDataLoadingBehavior).OnAllowSortingChanged()))
        Public Shared ReadOnly AllowColumnFilteringProperty As DependencyProperty = DependencyProperty.Register("AllowColumnFiltering", GetType(Boolean), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(True, Sub(d, e) CType(d, AsynchronousDataLoadingBehavior).OnAllowColumnFilteringChanged()))
        Public Shared ReadOnly AllowFilterEditorProperty As DependencyProperty = DependencyProperty.Register("AllowFilterEditor", GetType(DefaultBoolean), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(DefaultBoolean.True, Sub(d, e) CType(d, AsynchronousDataLoadingBehavior).OnAllowFilterEditorChanged()))
        Public Shared ReadOnly ShowTotalSummaryProperty As DependencyProperty = DependencyProperty.Register("ShowTotalSummary", GetType(Boolean), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(False, Sub(d, e) CType(d, AsynchronousDataLoadingBehavior).OnShowTotalSummaryChanged()))
        Public Shared ReadOnly ShowFilterPanelModeProperty As DependencyProperty = DependencyProperty.Register("ShowFilterPanelMode", GetType(ShowFilterPanelMode), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(ShowFilterPanelMode.Default, Sub(d, e) CType(d, AsynchronousDataLoadingBehavior).OnShowFilterPanelModeChanged()))

        Public Shared ReadOnly GroupIndexProperty As DependencyProperty = DependencyProperty.RegisterAttached("GroupIndex", GetType(Integer), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(-1, New PropertyChangedCallback(AddressOf OnGroupIndexChanged)))
        Public Shared ReadOnly SortIndexProperty As DependencyProperty = DependencyProperty.RegisterAttached("SortIndex", GetType(Integer), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(-1, New PropertyChangedCallback(AddressOf OnSortIndexChanged)))
        Public Shared Function GetGroupIndex(ByVal obj As GridColumn) As Integer
            Return DirectCast(obj.GetValue(GroupIndexProperty), Integer)
        End Function
        Public Shared Sub SetGroupIndex(ByVal obj As GridColumn, ByVal value As Integer)
            obj.SetValue(GroupIndexProperty, value)
        End Sub
        Public Shared Function GetSortIndex(ByVal obj As GridColumn) As Integer
            Return DirectCast(obj.GetValue(SortIndexProperty), Integer)
        End Function
        Public Shared Sub SetSortIndex(ByVal obj As GridColumn, ByVal value As Integer)
            obj.SetValue(SortIndexProperty, value)
        End Sub
        Private Shared Sub OnGroupIndexChanged(ByVal obj As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
            Dim column As GridColumn = CType(obj, GridColumn)
            If column.View Is Nothing Then
                Return
            End If

            Dim grid_Renamed As GridControl = CType(column.View.DataControl, GridControl)
            Dim bhvr As AsynchronousDataLoadingBehavior = GetAsynchronousDataLoadingBehavior(grid_Renamed)
            If Not bhvr.IsGridEnabled Then
                Return
            End If
            column.GroupIndex = DirectCast(e.NewValue, Integer)
        End Sub
        Private Shared Sub OnSortIndexChanged(ByVal obj As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
            Dim column As GridColumn = CType(obj, GridColumn)
            If column.View Is Nothing Then
                Return
            End If

            Dim grid_Renamed As GridControl = CType(column.View.DataControl, GridControl)
            Dim bhvr As AsynchronousDataLoadingBehavior = GetAsynchronousDataLoadingBehavior(grid_Renamed)
            If Not bhvr.IsGridEnabled Then
                Return
            End If
            column.SortIndex = DirectCast(e.NewValue, Integer)
        End Sub

        Private Shared ReadOnly AsynchronousDataLoadingBehaviorProperty As DependencyProperty = DependencyProperty.RegisterAttached("AsynchronousDataLoadingBehavior", GetType(AsynchronousDataLoadingBehavior), GetType(AsynchronousDataLoadingBehavior), New PropertyMetadata(Nothing))
        Shared Function GetAsynchronousDataLoadingBehavior(ByVal obj As GridControl) As AsynchronousDataLoadingBehavior
            Return DirectCast(obj.GetValue(AsynchronousDataLoadingBehaviorProperty), AsynchronousDataLoadingBehavior)
        End Function
        Private Shared Sub SetAsynchronousDataLoadingBehavior(ByVal obj As GridControl, ByVal value As AsynchronousDataLoadingBehavior)
            obj.SetValue(AsynchronousDataLoadingBehaviorProperty, value)
        End Sub
        #End Region
        Public Property AutoDisableGridControl() As Boolean
            Get
                Return DirectCast(GetValue(AutoDisableGridControlProperty), Boolean)
            End Get
            Set(ByVal value As Boolean)
                SetValue(AutoDisableGridControlProperty, value)
            End Set
        End Property
        Public Property NeverEnableGridControl() As Boolean
            Get
                Return DirectCast(GetValue(NeverEnableGridControlProperty), Boolean)
            End Get
            Set(ByVal value As Boolean)
                SetValue(NeverEnableGridControlProperty, value)
            End Set
        End Property
        Public Property DataSourceManager() As DataSourceManager
            Get
                Return DirectCast(GetValue(DataSourceManagerProperty), DataSourceManager)
            End Get
            Set(ByVal value As DataSourceManager)
                SetValue(DataSourceManagerProperty, value)
            End Set
        End Property
        Public Property AllowEditing() As Boolean
            Get
                Return DirectCast(GetValue(AllowEditingProperty), Boolean)
            End Get
            Set(ByVal value As Boolean)
                SetValue(AllowEditingProperty, value)
            End Set
        End Property
        Public Property AllowGrouping() As Boolean
            Get
                Return DirectCast(GetValue(AllowGroupingProperty), Boolean)
            End Get
            Set(ByVal value As Boolean)
                SetValue(AllowGroupingProperty, value)
            End Set
        End Property
        Public Property AllowSorting() As Boolean
            Get
                Return DirectCast(GetValue(AllowSortingProperty), Boolean)
            End Get
            Set(ByVal value As Boolean)
                SetValue(AllowSortingProperty, value)
            End Set
        End Property
        Public Property AllowColumnFiltering() As Boolean
            Get
                Return DirectCast(GetValue(AllowColumnFilteringProperty), Boolean)
            End Get
            Set(ByVal value As Boolean)
                SetValue(AllowColumnFilteringProperty, value)
            End Set
        End Property
        Public Property AllowFilterEditor() As DefaultBoolean
            Get
                Return DirectCast(GetValue(AllowFilterEditorProperty), DefaultBoolean)
            End Get
            Set(ByVal value As DefaultBoolean)
                SetValue(AllowFilterEditorProperty, value)
            End Set
        End Property
        Public Property ShowTotalSummary() As Boolean
            Get
                Return DirectCast(GetValue(ShowTotalSummaryProperty), Boolean)
            End Get
            Set(ByVal value As Boolean)
                SetValue(ShowTotalSummaryProperty, value)
            End Set
        End Property
        Public Property ShowFilterPanelMode() As ShowFilterPanelMode
            Get
                Return DirectCast(GetValue(ShowFilterPanelModeProperty), ShowFilterPanelMode)
            End Get
            Set(ByVal value As ShowFilterPanelMode)
                SetValue(ShowFilterPanelModeProperty, value)
            End Set
        End Property

        Public ReadOnly Property GridView() As DataViewBase
            Get
                Return If(Grid IsNot Nothing, Grid.View, Nothing)
            End Get
        End Property
        Public ReadOnly Property Grid() As GridControl
            Get
                Return AssociatedObject
            End Get
        End Property

        Private _IsAttached As Boolean = False
        Private IsGridEnabled As Boolean = True
        Private IsFirstAttaching As Boolean = True

        Private Sub OnAllowEditingChanged()
            If (Not IsGridEnabled) OrElse GridView Is Nothing Then
                Return
            End If
            GridView.AllowEditing = AllowEditing
        End Sub
        Private Sub OnAllowGroupingChanged()
            If (Not IsGridEnabled) OrElse GridView Is Nothing Then
                Return
            End If
            If TypeOf GridView Is TableView Then
                CType(GridView, TableView).AllowGrouping = AllowGrouping
            End If
        End Sub
        Private Sub OnAllowSortingChanged()
            If (Not IsGridEnabled) OrElse GridView Is Nothing Then
                Return
            End If
            GridView.AllowSorting = AllowSorting
        End Sub
        Private Sub OnAllowColumnFilteringChanged()
            If (Not IsGridEnabled) OrElse GridView Is Nothing Then
                Return
            End If
            GridView.AllowColumnFiltering = AllowColumnFiltering
        End Sub
        Private Sub OnAllowFilterEditorChanged()
            If (Not IsGridEnabled) OrElse GridView Is Nothing Then
                Return
            End If
            GridView.AllowFilterEditor = AllowFilterEditor
        End Sub
        Private Sub OnShowTotalSummaryChanged()
            If (Not IsGridEnabled) OrElse GridView Is Nothing Then
                Return
            End If
            GridView.ShowTotalSummary = ShowTotalSummary
        End Sub
        Private Sub OnShowFilterPanelModeChanged()
            If (Not IsGridEnabled) OrElse GridView Is Nothing Then
                Return
            End If
            GridView.ShowFilterPanelMode = ShowFilterPanelMode
        End Sub

        Protected Overrides Sub OnAttached()
            IsFirstAttaching = True
            MyBase.OnAttached()
            SetAsynchronousDataLoadingBehavior(AssociatedObject, Me)
            If GridView Is Nothing Then
                AddHandler Grid.Loaded, AddressOf OnGridLoaded
                Return
            End If
            TryAttach()
        End Sub
        Protected Overrides Sub OnDetaching()
            SetAsynchronousDataLoadingBehavior(AssociatedObject, Nothing)
            RemoveHandler Grid.Loaded, AddressOf OnGridLoaded
            TryDetach()
            MyBase.OnDetaching()
        End Sub
        Private Sub OnDataSourceManagerChanged(ByVal oldManager As DataSourceManager)
            If oldManager IsNot Nothing Then
                Throw New InvalidOperationException("The OnDataSourceManager property cannot be initialized twice.")
            End If
            TryAttach()
        End Sub
        Private Sub OnGridLoaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
            RemoveHandler DirectCast(sender, GridControl).Loaded, AddressOf OnGridLoaded
            TryAttach()
        End Sub
        Private Sub TryAttach()
            If DataSourceManager Is Nothing OrElse Grid Is Nothing OrElse GridView Is Nothing Then
                Return
            End If
            If _IsAttached Then
                Return
            End If
            _IsAttached = True
            BindingOperations.SetBinding(Grid, GridControl.ItemsSourceProperty, New Binding("DataSource") With {.Source = DataSourceManager})
            AddHandler DataSourceManager.LoadingCompleted, AddressOf OnLoadingCompleted
            AddHandler DataSourceManager.DataSourceInitialized, AddressOf OnDataSourceManagerDataSourceInitialized
            AddHandler DataSourceManager.DataSourceUninitialized, AddressOf OnDataSourceManagerDataSourceUninitialized
            If DataSourceManager.IsDataSourceInitialized Then
                DisableGrid()
            End If
            If DataSourceManager.IsLoadingCompleted Then
                EnableGrid()
            End If
        End Sub
        Private Sub TryDetach()
            If DataSourceManager Is Nothing OrElse Grid Is Nothing OrElse GridView Is Nothing Then
                Return
            End If
            If Not _IsAttached Then
                Return
            End If
            _IsAttached = False
            Grid.ClearValue(GridControl.ItemsSourceProperty)
            EnableGrid()
            RemoveHandler DataSourceManager.LoadingCompleted, AddressOf OnLoadingCompleted
            RemoveHandler DataSourceManager.DataSourceInitialized, AddressOf OnDataSourceManagerDataSourceInitialized
            RemoveHandler DataSourceManager.DataSourceUninitialized, AddressOf OnDataSourceManagerDataSourceUninitialized
        End Sub

        Private Sub OnDataSourceManagerDataSourceInitialized(ByVal sender As DataSourceManagerBase, ByVal oldDataSource As AsynchronousCollectionBase)
            DisableGrid()
        End Sub
        Private Sub OnDataSourceManagerDataSourceUninitialized(ByVal sender As DataSourceManagerBase, ByVal oldDataSource As AsynchronousCollectionBase)
            DisableGrid()
        End Sub
        Private Sub OnLoadingCompleted(ByVal sender As DataSourceManagerBase, ByVal dataSource As AsynchronousCollectionBase)
            EnableGrid()
        End Sub

        Private Sub DisableGrid()
            IsGridEnabled = False
            SetIsEnabledGridCore(False)
            If Not IsFirstAttaching Then
                StoreColumnsProperties()
            End If
            If AutoDisableGridControl Then
                Grid.ClearSorting()
                Grid.ClearGrouping()
            End If
            IsFirstAttaching = False
        End Sub
        Private Sub EnableGrid()
            If NeverEnableGridControl Then
                Return
            End If
            IsGridEnabled = True
            SetIsEnabledGridCore(True)
            RestoreColumnsProperties()
        End Sub
        Private Sub SetIsEnabledGridCore(ByVal isEnabled As Boolean)
            Dim tableView As TableView = TryCast(GridView, TableView)
            Dim isEn As Boolean = isEnabled OrElse Not AutoDisableGridControl
            GridView.AllowEditing = If(isEn, AllowEditing, False)
            GridView.AllowSorting = If(isEn, AllowSorting, False)
            GridView.AllowColumnFiltering = If(isEn, AllowColumnFiltering, False)
            GridView.AllowFilterEditor = If(isEn, AllowFilterEditor, DefaultBoolean.False)
            GridView.ShowTotalSummary = If(isEn, ShowTotalSummary, False)
            GridView.ShowFilterPanelMode = If(isEn, ShowFilterPanelMode, ShowFilterPanelMode.Never)
            If tableView IsNot Nothing Then
                tableView.AllowGrouping = If(isEn, AllowGrouping, False)
            End If
        End Sub

        Private Sub StoreColumnsProperties()
            For Each column As GridColumn In Grid.Columns
                SetGroupIndex(column, column.GroupIndex)
                SetSortIndex(column, column.SortIndex)
            Next column
        End Sub
        Private Sub RestoreColumnsProperties()
            If Not AutoDisableGridControl Then
                StoreColumnsProperties()
                Return
            End If
            For Each column As GridColumn In Grid.Columns
                column.GroupIndex = GetGroupIndex(column)
                column.SortIndex = GetSortIndex(column)
            Next column
        End Sub
    End Class
End Namespace
