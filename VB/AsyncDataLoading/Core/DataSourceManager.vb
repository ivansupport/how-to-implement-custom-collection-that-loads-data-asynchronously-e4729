Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Reflection
Imports System.Windows.Controls
Imports System.Collections
Imports System.ServiceModel
Imports System.Windows
Namespace AsyncDataLoading
    Public Class DataSourceManager
        Inherits DataSourceManagerBase

        Public Shared ReadOnly EntityTypeProperty As DependencyProperty = DependencyProperty.Register("EntityType", GetType(Type), GetType(DataSourceManager), New UIPropertyMetadata(Sub(d, e) CType(d, DataSourceManager).OnEntityTypeChanged()))
        Public Shared ReadOnly CountProperty As DependencyProperty = DependencyProperty.Register("Count", GetType(Integer), GetType(DataSourceManager), New PropertyMetadata(0))
        Public Shared ReadOnly LoadedCountProperty As DependencyProperty = DependencyProperty.Register("LoadedCount", GetType(Integer), GetType(DataSourceManager), New PropertyMetadata(0))
        Public Shared ReadOnly AutoInitializeProperty As DependencyProperty = DependencyProperty.Register("AutoInitialize", GetType(Boolean), GetType(DataSourceManager), New PropertyMetadata(True))
        Public Event RequestCount_Renamed As RequestCount
        Public Event RequestData_Renamed As RequestData
        Public Event SubmitChanges_Renamed As SubmitChanges
        Public Property EntityType() As Type
            Get
                Return DirectCast(GetValue(EntityTypeProperty), Type)
            End Get
            Set(ByVal value As Type)
                SetValue(EntityTypeProperty, value)
            End Set
        End Property
        Public Property Count() As Integer
            Get
                Return DirectCast(GetValue(CountProperty), Integer)
            End Get
            Private Set(ByVal value As Integer)
                SetValue(CountProperty, value)
            End Set
        End Property
        Public Property LoadedCount() As Integer
            Get
                Return DirectCast(GetValue(LoadedCountProperty), Integer)
            End Get
            Private Set(ByVal value As Integer)
                SetValue(LoadedCountProperty, value)
            End Set
        End Property
        Public Property AutoInitialize() As Boolean
            Get
                Return DirectCast(GetValue(AutoInitializeProperty), Boolean)
            End Get
            Set(ByVal value As Boolean)
                SetValue(AutoInitializeProperty, value)
            End Set
        End Property

        Public Sub New()
            MyBase.New()
            DefaultStyleKey = GetType(DataSourceManager)
        End Sub
        Public Overrides Sub Initialize()
            If IsDataSourceInitialized Then
                Return
            End If
            InitializeCore()
        End Sub
        Public Overrides Sub Uninitialize()
            If Not IsDataSourceInitialized Then
                Return
            End If
            If DataSource IsNot Nothing Then
                DataSource.Dispose()
            End If
            SetDataSource(Nothing)
        End Sub

        Protected Overridable Sub OnEntityTypeChanged()
            If IsDataSourceInitialized Then
                Throw New InvalidOperationException("Initialization has completed. Cannot set the EntityType property after initialization.")
            End If
            TryInitialize()
        End Sub
        Protected Overrides Sub OnSettingsChanged()
            MyBase.OnSettingsChanged()
            TryInitialize()
        End Sub
        Protected Overrides Sub SetDataSource(ByVal dataSource As AsynchronousCollectionBase)
            If Me.DataSource IsNot Nothing Then
                RemoveHandler DirectCast(Me.DataSource, INotifyPropertyChanged).PropertyChanged, AddressOf OnDataSourcePropertyChanged
            End If
            MyBase.SetDataSource(dataSource)
            If Me.DataSource IsNot Nothing Then
                AddHandler DirectCast(Me.DataSource, INotifyPropertyChanged).PropertyChanged, AddressOf OnDataSourcePropertyChanged
            End If
        End Sub
        Private Sub OnDataSourcePropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
            If e.PropertyName = "Count" OrElse e.PropertyName = "LoadedCount" Then
                Count = DirectCast(sender, AsynchronousCollectionBase).Count
                LoadedCount = DirectCast(sender, AsynchronousCollectionBase).LoadedCount
            End If
        End Sub

        Private Sub TryInitialize()
            If IsDataSourceInitialized Then
                Return
            End If
            If AutoInitialize Then
                InitializeCore()
            End If
        End Sub
        Private Sub InitializeCore()
            If EntityType Is Nothing OrElse Settings Is Nothing Then
                Return
            End If
            If RequestCount_RenamedEvent Is Nothing OrElse RequestData_RenamedEvent Is Nothing Then
                Return
            End If
            Dim createCollectionMethodArgs() As Type = { GetType(RequestCount), GetType(RequestData), GetType(SubmitChanges), GetType(AsynchronousCollectionSettings) }
            Dim createCollectionMethod As MethodInfo = GetType(AsynchronousCollectionFactory).GetMethod("CreateCollection", createCollectionMethodArgs)
            createCollectionMethod = createCollectionMethod.MakeGenericMethod(New Type() { EntityType })
            Dim col As Object
            If SubmitChanges_RenamedEvent IsNot Nothing Then
                col = createCollectionMethod.Invoke(Nothing, New Object() { _
                    New RequestCount(AddressOf OnRequestCount), _
                    New RequestData(AddressOf OnRequestData), _
                    New SubmitChanges(AddressOf OnSubmitChanges), _
                    Settings _
                })
            Else
                col = createCollectionMethod.Invoke(Nothing, New Object() { _
                    New RequestCount(AddressOf OnRequestCount), _
                    New RequestData(AddressOf OnRequestData), _
                    Nothing, _
                    Settings _
                })
            End If
            SetDataSource(DirectCast(col, AsynchronousCollectionBase))
        End Sub

        Private Sub OnRequestCount(ByVal request As RequestCountArgs, ByVal feedback As RequestCountResult)
            RaiseEvent RequestCount_Renamed(request, feedback)
        End Sub
        Private Sub OnRequestData(ByVal request As RequestDataArgs, ByVal feedback As RequestDataResult)
            RaiseEvent RequestData_Renamed(request, feedback)
        End Sub
        Private Sub OnSubmitChanges(ByVal args As SubmitChangesArgs, ByVal feedback As SubmitChangesResult)
            RaiseEvent SubmitChanges_Renamed(args, feedback)
        End Sub
    End Class
End Namespace