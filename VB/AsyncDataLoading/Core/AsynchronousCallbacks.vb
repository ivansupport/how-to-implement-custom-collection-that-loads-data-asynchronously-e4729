Imports System.Collections
Imports System.Diagnostics
Imports System

Namespace AsyncDataLoading
    Public MustInherit Class RequestArgs
    End Class
    Public MustInherit Class RequestResult
        Private privateRequestArgs As RequestArgs
        Public Property RequestArgs() As RequestArgs
            Get
                Return privateRequestArgs
            End Get
            Private Set(ByVal value As RequestArgs)
                privateRequestArgs = value
            End Set
        End Property

        Public Sub New(ByVal requestArgs As RequestArgs)
            If requestArgs Is Nothing Then
                Throw New ArgumentNullException("requestArgs")
            End If
            Me.RequestArgs = requestArgs
        End Sub
        Public MustOverride Sub RaiseFeedback(ByVal parameter As Object)
    End Class

    Public Class RequestDataArgs
        Inherits RequestArgs

        Private privateSkipCount As Integer
        Public Property SkipCount() As Integer
            Get
                Return privateSkipCount
            End Get
            Private Set(ByVal value As Integer)
                privateSkipCount = value
            End Set
        End Property
        Private privateTakeCount As Integer
        Public Property TakeCount() As Integer
            Get
                Return privateTakeCount
            End Get
            Private Set(ByVal value As Integer)
                privateTakeCount = value
            End Set
        End Property
        Private privateIsBackgroundRequest As Boolean
        Public Property IsBackgroundRequest() As Boolean
            Get
                Return privateIsBackgroundRequest
            End Get
            Private Set(ByVal value As Boolean)
                privateIsBackgroundRequest = value
            End Set
        End Property

        Public Sub New(ByVal skipCount As Integer, ByVal takeCount As Integer, Optional ByVal isBackgroundRequest As Boolean = False)
            Me.IsBackgroundRequest = isBackgroundRequest
            Me.SkipCount = skipCount
            Me.TakeCount = takeCount
        End Sub
        Friend Shared ReadOnly Empty As New RequestDataArgs(0, 0)
    End Class
    Public Class RequestDataResult
        Inherits RequestResult

        Private privateData As IEnumerable
        Public Property Data() As IEnumerable
            Get
                Return privateData
            End Get
            Private Set(ByVal value As IEnumerable)
                privateData = value
            End Set
        End Property
        Private ReadOnly Feedback As RequestDataFeedback

        Friend Sub New(ByVal requestArgs As RequestDataArgs, ByVal feedback As RequestDataFeedback)
            MyBase.New(requestArgs)

            If feedback Is Nothing Then
                Throw New ArgumentNullException("feedback")
            End If
            Me.Feedback = feedback
        End Sub
        Public Overrides Sub RaiseFeedback(ByVal data As Object)
            If Not(TypeOf data Is IEnumerable) Then
                Throw New ArgumentException("The data parameter must be IEnumerable type")
            End If
            Me.Data = DirectCast(data, IEnumerable)
            Feedback(Me)
        End Sub
    End Class
    Public Delegate Sub RequestData(ByVal request As RequestDataArgs, ByVal feedback As RequestDataResult)
    Friend Delegate Sub RequestDataFeedback(ByVal resultFeedback As RequestDataResult)

    Public Class RequestCountArgs
        Inherits RequestArgs

    End Class
    Public Class RequestCountResult
        Inherits RequestResult

        Private privateCount As Integer
        Public Property Count() As Integer
            Get
                Return privateCount
            End Get
            Private Set(ByVal value As Integer)
                privateCount = value
            End Set
        End Property
        Private ReadOnly Feedback As RequestCountFeedback

        Friend Sub New(ByVal requestArgs As RequestCountArgs, ByVal feedback As RequestCountFeedback)
            MyBase.New(requestArgs)
            If feedback Is Nothing Then
                Throw New ArgumentNullException("feedback")
            End If
            Me.Feedback = feedback
        End Sub
        Public Overrides Sub RaiseFeedback(ByVal count As Object)
            If Not(TypeOf count Is Integer) Then
                Throw New ArgumentException("The count parameter must be integer type")
            End If
            Me.Count = DirectCast(count, Integer)
            Feedback(Me)
        End Sub
    End Class
    Public Delegate Sub RequestCount(ByVal request As RequestCountArgs, ByVal feedback As RequestCountResult)
    Friend Delegate Sub RequestCountFeedback(ByVal resultFeedback As RequestCountResult)

    Public Enum TypeSubmitChanges
        Add
        Remove
        Clear
        Replace
        Update
    End Enum
    Public Class SubmitChangesArgs
        Inherits RequestArgs

        Public ReadOnly Type As TypeSubmitChanges
        Private privateOldItem As Object
        Public Property OldItem() As Object
            Get
                Return privateOldItem
            End Get
            Private Set(ByVal value As Object)
                privateOldItem = value
            End Set
        End Property
        Private privateNewItem As Object
        Public Property NewItem() As Object
            Get
                Return privateNewItem
            End Get
            Private Set(ByVal value As Object)
                privateNewItem = value
            End Set
        End Property
        Private privateIndex As Integer
        Public Property Index() As Integer
            Get
                Return privateIndex
            End Get
            Private Set(ByVal value As Integer)
                privateIndex = value
            End Set
        End Property

        Public Sub New(ByVal Type As TypeSubmitChanges)
            OldItem = Nothing
            NewItem = Nothing
            Index = -1
        End Sub
        Public Sub New(ByVal Type As TypeSubmitChanges, ByVal oldItem As Object, ByVal newItem As Object, ByVal index As Integer)
            Me.OldItem = oldItem
            Me.NewItem = newItem
            Me.Index = index
        End Sub
    End Class
    Public Class SubmitChangesResult
        Inherits RequestResult

        Private privateCancel As Boolean
        Public Property Cancel() As Boolean
            Get
                Return privateCancel
            End Get
            Private Set(ByVal value As Boolean)
                privateCancel = value
            End Set
        End Property
        Private ReadOnly Feedback As SubmitChangesFeedback
        Friend Sub New(ByVal requestArgs As SubmitChangesArgs, ByVal feedback As SubmitChangesFeedback)
            MyBase.New(requestArgs)
                Me.Feedback = feedback
        End Sub
        Public Overrides Sub RaiseFeedback(ByVal cancel As Object)
            If Not(TypeOf cancel Is Boolean) Then
                Throw New ArgumentException("The cancel parameter must be boolean type")
            End If
            Me.Cancel = DirectCast(cancel, Boolean)
            Feedback(Me)
        End Sub
    End Class
    Public Delegate Sub SubmitChanges(ByVal request As SubmitChangesArgs, ByVal feedback As SubmitChangesResult)
    Friend Delegate Sub SubmitChangesFeedback(ByVal resultFeedback As SubmitChangesResult)
End Namespace
