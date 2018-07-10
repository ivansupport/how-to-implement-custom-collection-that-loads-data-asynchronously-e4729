Imports AsyncDataLoading.ServiceReference1
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Namespace AsyncDataLoading
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow
        Inherits Window

        Private srv As ServiceReference1.OrderServiceClient
        Public Sub New()
            srv = New ServiceReference1.OrderServiceClient()
            AddHandler srv.GetRecordsCountCompleted, AddressOf srv_GetRecordsCountCompleted
            AddHandler srv.GetRecordsCompleted, AddressOf srv_GetRecordsCompleted
            InitializeComponent()
        End Sub
        Private Sub OnRequestCount(ByVal request As RequestCountArgs, ByVal feedback As RequestCountResult)
            srv.GetRecordsCountAsync(New GetRecordsCountRequest(), feedback)
        End Sub
        Private Sub srv_GetRecordsCountCompleted(ByVal sender As Object, ByVal e As GetRecordsCountCompletedEventArgs)
            Dim feedback As RequestCountResult = TryCast(e.UserState, RequestCountResult)
            If feedback Is Nothing Then
                Return
            End If
            feedback.RaiseFeedback(e.Result.GetRecordsCountResult)
        End Sub

        Private Sub OnRequestData(ByVal request As RequestDataArgs, ByVal feedback As RequestDataResult)
            srv.GetRecordsAsync(New GetRecordsRequest(request.SkipCount, request.TakeCount), feedback)
        End Sub
        Private Sub srv_GetRecordsCompleted(ByVal sender As Object, ByVal e As GetRecordsCompletedEventArgs)
            Dim feedback As RequestDataResult = TryCast(e.UserState, RequestDataResult)
            If feedback Is Nothing Then
                Return
            End If
            feedback.RaiseFeedback(e.Result.GetRecordsResult)
        End Sub
    End Class
End Namespace
