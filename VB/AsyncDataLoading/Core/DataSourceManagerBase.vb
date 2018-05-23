Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Controls
Namespace AsyncDataLoading
	Public Delegate Sub DataSourceInitializedEventHander(ByVal sender As DataSourceManagerBase, ByVal newDataSource As AsynchronousCollectionBase)
	Public Delegate Sub DataSourceUninitializedEventHander(ByVal sender As DataSourceManagerBase, ByVal oldDataSource As AsynchronousCollectionBase)
	Public Delegate Sub DataSourceLoadingCompleted(ByVal sender As DataSourceManagerBase, ByVal dataSource As AsynchronousCollectionBase)
	<Browsable(False)> _
	Public MustInherit Class DataSourceManagerBase
		Inherits Control
		Implements INotifyPropertyChanged
		Private dataSource_Renamed As AsynchronousCollectionBase = Nothing

        Public Shared ReadOnly SettingsProperty As DependencyProperty = DependencyProperty.Register("Settings", GetType(AsynchronousCollectionSettings), GetType(DataSourceManagerBase), New PropertyMetadata(Sub(d, e)
                                                                                                                                                                                                                  CType(d, DataSourceManagerBase).OnSettingsChanged()
                                                                                                                                                                                                              End Sub))
        Public Shared ReadOnly IsDataSourceInitializedProperty As DependencyProperty = DependencyProperty.Register("IsDataSourceInitialized", GetType(Boolean), GetType(DataSourceManagerBase), New PropertyMetadata(False, Sub(d, e)
                                                                                                                                                                                                                                CType(d, DataSourceManagerBase).OnIsDataSourceInitializedChanged()
                                                                                                                                                                                                                            End Sub))
		Public Property Settings() As AsynchronousCollectionSettings
			Get
				Return CType(GetValue(SettingsProperty), AsynchronousCollectionSettings)
			End Get
			Set(ByVal value As AsynchronousCollectionSettings)
				SetValue(SettingsProperty, value)
			End Set
		End Property
		Public Property IsDataSourceInitialized() As Boolean
			Get
				Return CBool(GetValue(IsDataSourceInitializedProperty))
			End Get
			Private Set(ByVal value As Boolean)
				SetValue(IsDataSourceInitializedProperty, value)
			End Set
		End Property
		Public Property DataSource() As AsynchronousCollectionBase
			Get
				Return GetDataSource()
			End Get
			Private Set(ByVal value As AsynchronousCollectionBase)
				dataSource_Renamed = value
				OnPropertyChanged("DataSource")
			End Set
		End Property
		Public Event DataSourceInitialized As DataSourceInitializedEventHander
		Public Event DataSourceUninitialized As DataSourceUninitializedEventHander
		Public Event LoadingCompleted As DataSourceLoadingCompleted
		Public ReadOnly Property IsLoadingCompleted() As Boolean
			Get
				Return If(DataSource IsNot Nothing, DataSource.IsLoadingCompleted, False)
			End Get
		End Property

		Public Sub New()
			Settings = AsynchronousCollectionSettingsFactory.CreateOnDemandRequestDataModeSettings()
		End Sub
		Public MustOverride Sub Initialize()
		Public MustOverride Sub Uninitialize()
		Public Sub UpdateCount()
			If DataSource IsNot Nothing Then
				DataSource.UpdateCount()
			End If
		End Sub
		Public Sub UpdateData()
			If DataSource IsNot Nothing Then
				DataSource.UpdateData()
			End If
		End Sub
		Public Sub UpdateData(ByVal skipCount As Integer, ByVal takeCount As Integer)
			If DataSource IsNot Nothing Then
				DataSource.UpdateData(skipCount, takeCount)
			End If
		End Sub
		Public Sub RaiseDataChanges(ByVal skipCount As Integer, ByVal takeCount As Integer)
			If DataSource IsNot Nothing Then
				DataSource.RaiseDataChanges(skipCount, takeCount)
			End If
		End Sub
		Public Sub RaiseDataChanges(ByVal record As Object)
			If DataSource IsNot Nothing Then
				DataSource.RaiseDataChanges(record)
			End If
		End Sub

		Protected Overridable Sub OnSettingsChanged()
			If IsDataSourceInitialized Then
				Uninitialize()
			End If
			Initialize()
		End Sub
		Protected Overridable Sub OnIsDataSourceInitializedChanged()
		End Sub
		Protected Overridable Overloads Sub OnPropertyChanged(ByVal propertyName As String)
			RaiseEvent propertyChanged(Me, New PropertyChangedEventArgs(propertyName))
		End Sub
		Protected Overridable Function GetDataSource() As AsynchronousCollectionBase
			Return dataSource_Renamed
		End Function
		Protected Overridable Sub SetDataSource(ByVal dataSource As AsynchronousCollectionBase)
			Dim oldDataSource As AsynchronousCollectionBase = Me.DataSource
			Dim newDataSource As AsynchronousCollectionBase = dataSource
			Me.DataSource = dataSource
			IsDataSourceInitialized = Me.DataSource IsNot Nothing
			If oldDataSource IsNot Nothing Then
				RemoveHandler oldDataSource.LoadingCompleted, AddressOf OnDataSourceOnLoadingCompleted
			End If
			If newDataSource IsNot Nothing Then
				AddHandler newDataSource.LoadingCompleted, AddressOf OnDataSourceOnLoadingCompleted
				If newDataSource.IsLoadingCompleted Then
					OnDataSourceOnLoadingCompleted(newDataSource, EventArgs.Empty)
				End If
			End If
			If IsDataSourceInitialized Then
				RaiseEvent DataSourceInitialized(Me, newDataSource)
			Else
				RaiseEvent DataSourceUninitialized(Me, oldDataSource)
			End If
		End Sub
		Private Sub OnDataSourceOnLoadingCompleted(ByVal sender As Object, ByVal e As EventArgs)
			RaiseEvent LoadingCompleted(Me, CType(sender, AsynchronousCollectionBase))
		End Sub

        Private Event _propertyChanged As PropertyChangedEventHandler
		Private Custom Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
			AddHandler(ByVal value As PropertyChangedEventHandler)
                AddHandler _propertyChanged, value
			End AddHandler
			RemoveHandler(ByVal value As PropertyChangedEventHandler)
                RemoveHandler _propertyChanged, value
			End RemoveHandler
			RaiseEvent(ByVal sender As System.Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
			End RaiseEvent
		End Event
	End Class
End Namespace