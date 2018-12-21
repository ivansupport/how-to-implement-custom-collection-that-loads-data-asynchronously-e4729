Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.Diagnostics
Imports System.Threading
Imports System.ComponentModel

Namespace AsyncDataLoading
    Public Enum RequestDataMode
        OnDemand
        InBackgroundThread
    End Enum
    Public NotInheritable Class AsynchronousCollectionFactory

        Private Sub New()
        End Sub

        Public Shared Function CreateCollection(Of T)(ByVal requestCount As RequestCount, ByVal requestData As RequestData, ByVal requestDataMode As RequestDataMode) As AsynchronousCollection(Of T)
            If requestDataMode = AsyncDataLoading.RequestDataMode.OnDemand Then
                Return New AsynchronousCollection(Of T)(requestCount, requestData)
            Else
                Return New AsynchronousCollection2(Of T)(requestCount, requestData)
            End If
        End Function
        Public Shared Function CreateCollection(Of T)(ByVal requestCount As RequestCount, ByVal requestData As RequestData, ByVal settings As AsynchronousCollectionSettings) As AsynchronousCollection(Of T)
            Return CreateCollection(Of T)(requestCount, requestData, Nothing, settings)
        End Function
        Public Shared Function CreateCollection(Of T)(ByVal requestCount As RequestCount, ByVal requestData As RequestData, ByVal submitChanges As SubmitChanges, ByVal settings As AsynchronousCollectionSettings) As AsynchronousCollection(Of T)
            If settings.GetType() Is GetType(AsynchronousCollection2Settings) Then
                Return New AsynchronousCollection2(Of T)(requestCount, requestData, submitChanges, DirectCast(settings, AsynchronousCollection2Settings))
            End If
            If settings.GetType() Is GetType(AsynchronousCollectionSettings) Then
                Return New AsynchronousCollection(Of T)(requestCount, requestData, submitChanges, settings)
            End If
            Return Nothing
        End Function
    End Class
End Namespace
