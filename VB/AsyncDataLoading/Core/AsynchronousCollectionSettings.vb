Imports Microsoft.VisualBasic
Imports System
Namespace AsyncDataLoading
	Public NotInheritable Class AsynchronousCollectionSettingsFactory
		Private Sub New()
		End Sub
		Public Shared Function CreateOnDemandRequestDataModeSettings() As AsynchronousCollectionSettings
			Return New AsynchronousCollectionSettings()
		End Function
		Public Shared Function CreateOnDemandRequestDataModeSettings(ByVal requestDataRate As Integer) As AsynchronousCollectionSettings
			Return New AsynchronousCollectionSettings(requestDataRate)
		End Function

		Public Shared Function CreateInBackgroundThreadRequestDataModeSettings() As AsynchronousCollection2Settings
			Return New AsynchronousCollection2Settings()
		End Function
		Public Shared Function CreateInBackgroundThreadRequestDataModeSettings(ByVal requestDataRate As Integer) As AsynchronousCollectionSettings
			Return New AsynchronousCollection2Settings(requestDataRate)
		End Function
		Public Shared Function CreateInBackgroundThreadRequestDataModeSettings(ByVal requestDataRate As Integer, ByVal backgroundRequestDataRate As Integer, ByVal backgroundRequestDataInterval As TimeSpan) As AsynchronousCollection2Settings
			Return New AsynchronousCollection2Settings(requestDataRate, backgroundRequestDataRate, backgroundRequestDataInterval)
		End Function
	End Class
	Public Class AsynchronousCollectionSettings
		Public ReadOnly Shared [Default] As New AsynchronousCollectionSettings()
        Private privateRequestDataRate As Integer
        Public Property RequestDataRate() As Integer
            Get
                Return privateRequestDataRate
            End Get
            Set(ByVal value As Integer)
                privateRequestDataRate = value
            End Set
        End Property


		Public Overridable ReadOnly Property Mode() As RequestDataMode
			Get
				Return RequestDataMode.OnDemand
			End Get
		End Property
		Public Sub New()
			RequestDataRate = 40
		End Sub
		Public Sub New(ByVal requestDataRate As Integer)
			RequestDataRate = requestDataRate
		End Sub

		Protected Friend Overridable Sub Apply(Of T)(ByVal col As AsynchronousCollection(Of T))
			col.RequestDataRate = RequestDataRate
		End Sub
	End Class
	Public Class AsynchronousCollection2Settings
		Inherits AsynchronousCollectionSettings
		Public ReadOnly Shadows Shared [Default] As New AsynchronousCollection2Settings()
		Public Overrides ReadOnly Property Mode() As RequestDataMode
			Get
				Return RequestDataMode.OnDemand
			End Get
		End Property
		Private privateBackgroundRequestDataRate As Integer
		Public Property BackgroundRequestDataRate() As Integer
			Get
				Return privateBackgroundRequestDataRate
			End Get
			Set(ByVal value As Integer)
				privateBackgroundRequestDataRate = value
			End Set
		End Property
		Private privateBackgroundRequestDataInterval As TimeSpan
		Public Property BackgroundRequestDataInterval() As TimeSpan
			Get
				Return privateBackgroundRequestDataInterval
			End Get
			Set(ByVal value As TimeSpan)
				privateBackgroundRequestDataInterval = value
			End Set
		End Property

		Public Sub New()
			MyBase.New()
			BackgroundRequestDataRate = 10
			BackgroundRequestDataInterval = TimeSpan.FromSeconds(0.1)
		End Sub
		Public Sub New(ByVal requestDataRate As Integer)
			MyBase.New(requestDataRate)
			BackgroundRequestDataRate = 10
			BackgroundRequestDataInterval = TimeSpan.FromSeconds(0.1)
		End Sub
		Public Sub New(ByVal requestDataRate As Integer, ByVal backgroundRequestDataRate As Integer, ByVal backgroundRequestDataInterval As TimeSpan)
			MyBase.New(requestDataRate)
			BackgroundRequestDataRate = backgroundRequestDataRate
			BackgroundRequestDataInterval = backgroundRequestDataInterval
		End Sub
		Protected Friend Overrides Sub Apply(Of T)(ByVal col As AsynchronousCollection(Of T))
			MyBase.Apply(Of T)(col)
			Dim col1 As AsynchronousCollection2(Of T) = CType(col, AsynchronousCollection2(Of T))
			col1.BackgroundRequestDataRate = BackgroundRequestDataRate
			col1.BackgroundRequestDataInterval = BackgroundRequestDataInterval
		End Sub
	End Class
End Namespace