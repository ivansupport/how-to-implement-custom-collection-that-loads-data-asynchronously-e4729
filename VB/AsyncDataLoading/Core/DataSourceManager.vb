Imports Microsoft.VisualBasic
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
        Public Shared ReadOnly EntityTypeProperty As DependencyProperty = DependencyProperty.Register("EntityType", GetType(Type), GetType(DataSourceManager), New UIPropertyMetadata(Sub(d, e)
                                                                                                                                                                                          CType(d, DataSourceManager).OnEntityTypeChanged()
                                                                                                                                                                                      End Sub))
		Public Shared ReadOnly CountProperty As DependencyProperty = DependencyProperty.Register("Count", GetType(Integer), GetType(DataSourceManager), New PropertyMetadata(0))
		Public Shared ReadOnly LoadedCountProperty As DependencyProperty = DependencyProperty.Register("LoadedCount", GetType(Integer), GetType(DataSourceManager), New PropertyMetadata(0))
		Public Shared ReadOnly AutoInitializeProperty As DependencyProperty = DependencyProperty.Register("AutoInitialize", GetType(Boolean), GetType(DataSourceManager), New PropertyMetadata(True))
		Public Event RequestCount As RequestCount
		Public Event RequestData As RequestData
		Public Event SubmitChanges As SubmitChanges
		Public Property EntityType() As Type
			Get
				Return CType(GetValue(EntityTypeProperty), Type)
			End Get
			Set(ByVal value As Type)
				SetValue(EntityTypeProperty, value)
			End Set
		End Property
		Public Property Count() As Integer
			Get
				Return CInt(Fix(GetValue(CountProperty)))
			End Get
			Private Set(ByVal value As Integer)
				SetValue(CountProperty, value)
			End Set
		End Property
		Public Property LoadedCount() As Integer
			Get
				Return CInt(Fix(GetValue(LoadedCountProperty)))
			End Get
			Private Set(ByVal value As Integer)
				SetValue(LoadedCountProperty, value)
			End Set
		End Property
		Public Property AutoInitialize() As Boolean
			Get
				Return CBool(GetValue(AutoInitializeProperty))
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
			If (Not IsDataSourceInitialized) Then
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
				RemoveHandler (CType(Me.DataSource, INotifyPropertyChanged)).PropertyChanged, AddressOf OnDataSourcePropertyChanged
			End If
			MyBase.SetDataSource(dataSource)
			If Me.DataSource IsNot Nothing Then
				AddHandler (CType(Me.DataSource, INotifyPropertyChanged)).PropertyChanged, AddressOf OnDataSourcePropertyChanged
			End If
		End Sub
		Private Sub OnDataSourcePropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
			If e.PropertyName = "Count" OrElse e.PropertyName = "LoadedCount" Then
				Count = (CType(sender, AsynchronousCollectionBase)).Count
				LoadedCount = (CType(sender, AsynchronousCollectionBase)).LoadedCount
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
			If RequestCountEvent Is Nothing OrElse RequestDataEvent Is Nothing Then
				Return
			End If
            Dim createCollectionMethodArgs() As Type = {GetType(RequestCount), GetType(RequestData), GetType(SubmitChanges), GetType(AsynchronousCollectionSettings)}
			Dim createCollectionMethod As MethodInfo = GetType(AsynchronousCollectionFactory).GetMethod("CreateCollection", createCollectionMethodArgs)
			createCollectionMethod = createCollectionMethod.MakeGenericMethod(New Type() { EntityType })
			Dim col As Object
			If SubmitChangesEvent IsNot Nothing Then
                col = createCollectionMethod.Invoke(Nothing, New Object() {New RequestCount(AddressOf OnRequestCount), New RequestData(AddressOf OnRequestData), New SubmitChanges(AddressOf OnSubmitChanges), Settings})
			Else
                col = createCollectionMethod.Invoke(Nothing, New Object() {New RequestCount(AddressOf OnRequestCount), New RequestData(AddressOf OnRequestData), Nothing, Settings})
			End If
			SetDataSource(CType(col, AsynchronousCollectionBase))
		End Sub

		Private Sub OnRequestCount(ByVal request As RequestCountArgs, ByVal feedback As RequestCountResult)
			RaiseEvent RequestCount(request, feedback)
		End Sub
		Private Sub OnRequestData(ByVal request As RequestDataArgs, ByVal feedback As RequestDataResult)
			RaiseEvent RequestData(request, feedback)
		End Sub
		Private Sub OnSubmitChanges(ByVal args As SubmitChangesArgs, ByVal feedback As SubmitChangesResult)
			RaiseEvent SubmitChanges(args, feedback)
		End Sub
	End Class
End Namespace