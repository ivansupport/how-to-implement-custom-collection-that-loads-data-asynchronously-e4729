Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.Diagnostics
Imports System.Threading
Imports System.ComponentModel
Imports System.Windows.Threading
Imports DevExpress.Xpf.Core.Native

Namespace AsyncDataLoading
    Public MustInherit Class AsynchronousCollectionBase
        Implements IList, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable, ILoaded

#Region "INotifyPropertyChanged"
        Private Event _propertyChanged As PropertyChangedEventHandler
        Private Custom Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
            AddHandler(ByVal value As PropertyChangedEventHandler)
                AddHandler _propertyChanged, value
            End AddHandler
            RemoveHandler(ByVal value As PropertyChangedEventHandler)
                RemoveHandler _propertyChanged, value
            End RemoveHandler
            RaiseEvent(ByVal sender As System.Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
                RaiseEvent _propertyChanged(sender, e)
            End RaiseEvent
        End Event

        Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub
        #End Region ' INotifyPropertyChanged;
        #Region "INotifyCollectionChanged"
        Public Event CollectionChanged As NotifyCollectionChangedEventHandler Implements INotifyCollectionChanged.CollectionChanged

        Protected Overridable Sub RaiseCollectionAdd(ByVal item As Object, ByVal index As Integer)
            If CollectionChangedEvent Is Nothing Then
                Return
            End If
            Dim args As New NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index)
            RaiseEvent CollectionChanged(Me, args)
        End Sub
        Protected Overridable Sub RaiseCollectionRemove(ByVal item As Object, ByVal index As Integer)
            If CollectionChangedEvent Is Nothing Then
                Return
            End If
            Dim args As New NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index)
            RaiseEvent CollectionChanged(Me, args)
        End Sub
        Protected Overridable Sub RaiseCollectionReset()
            If CollectionChangedEvent Is Nothing Then
                Return
            End If
            Dim args As New NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
            RaiseEvent CollectionChanged(Me, args)
        End Sub
        #End Region
        #Region "IList"
        Public Property IList_Item(ByVal index As Integer) As Object Implements IList.Item
            Get
                Return GetItem(index)
            End Get
            Set(ByVal value As Object)
                SetItem(index, value)
            End Set
        End Property
        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumeratorCore()
        End Function
        Private Function IList_IndexOf(ByVal value As Object) As Integer Implements IList.IndexOf
            Return IndexOf_Core(value)
        End Function
        Private Function IList_Contains(ByVal value As Object) As Boolean Implements IList.Contains
            Return IndexOf_Core(value) > -1
        End Function
        Private Function IList_Add(ByVal value As Object) As Integer Implements IList.Add
            Insert_Core(Count, value)
            Return Count
        End Function
        Private Sub IList_Insert(ByVal index As Integer, ByVal value As Object) Implements IList.Insert
            Insert_Core(index, value)
        End Sub
        Private Sub IList_Remove(ByVal value As Object) Implements IList.Remove
            Dim index As Integer = IndexOf_Core(value)
            RemoveAt_Core(index)
        End Sub
        Private Sub IList_RemoveAt(ByVal index As Integer) Implements IList.RemoveAt
            RemoveAt_Core(index)
        End Sub
        #End Region
        Public Property RequestDataRate() As Integer
            Get
                Return requestDataRate_Renamed
            End Get
            Friend Set(ByVal value As Integer)
                If value <= 0 Then
                    Throw New ArgumentException("Value must be more than zero")
                End If
                requestDataRate_Renamed = value
            End Set
        End Property
        <Bindable(BindableSupport.Yes, BindingDirection.OneWay)>
        Public Property LoadedCount() As Integer
            Get
                Return loadedItemsCount
            End Get
            Protected Set(ByVal value As Integer)
                loadedItemsCount = value
                OnPropertyChanged("LoadedCount")
            End Set
        End Property
        <Bindable(BindableSupport.Yes, BindingDirection.OneWay)>
        Public MustOverride ReadOnly Property Count() As Integer Implements System.Collections.ICollection.Count
        Public ReadOnly Property IsLoadingCompleted() As Boolean
            Get
                Return Count = LoadedCount AndAlso Count > 0
            End Get
        End Property
        Public ReadOnly Property IsReadOnly() As Boolean Implements IList.IsReadOnly
            Get
                Return IsReadOnlyCore
            End Get
        End Property
        Public ReadOnly Property IsFixedSize() As Boolean Implements IList.IsFixedSize
            Get
                Return IsReadOnlyCore
            End Get
        End Property
        Public Event LoadedItemsCountUpdated As EventHandler
        Public Event LoadingCompleted As EventHandler

        Public Sub New(ByVal requestCount As RequestCount, ByVal requestData As RequestData)
            Me.New(requestCount, requestData, Nothing)
        End Sub
        Public Sub New(ByVal requestCount As RequestCount, ByVal requestData As RequestData, ByVal submitChanges As SubmitChanges)
                Storage = New DataStorage()
                Me.SubmitChanges = submitChanges
                Me.RequestData = requestData
                Me.RequestCount = requestCount
        End Sub
        Public Overridable Sub Clear() Implements IList.Clear
            LoadedCount = 0
            RaiseSubmitChanges(TypeSubmitChanges.Clear, Nothing, Nothing, -1)
        End Sub
        Public Overridable Sub Dispose() Implements IDisposable.Dispose
            LoadedCount = 0
            Storage.Dispose()
            Storage = Nothing
            RequestData = Nothing
            RequestCount = Nothing
        End Sub
        Public MustOverride Sub UpdateCount()
        Public MustOverride Sub UpdateData(ByVal startIndex As Integer, ByVal count As Integer)
        Public Sub UpdateData()
            UpdateData(0, Count)
        End Sub
        Public Sub RaiseDataChanges(ByVal startIndex As Integer, ByVal count As Integer)
            If IsReadOnlyCore Then
                Throw New InvalidOperationException("This is read only collection.")
            End If
            For i As Integer = startIndex To (startIndex + count) - 1
                RaiseSubmitChanges(TypeSubmitChanges.Update, Nothing, Storage(i), i)
            Next i
        End Sub
        Public Sub RaiseDataChanges(ByVal record As Object)
            RaiseSubmitChanges(TypeSubmitChanges.Update, Nothing, record, -1)
        End Sub
        Public Function IsItemLoaded(ByVal index As Integer) As Boolean Implements ILoaded.IsItemLoaded
            If Storage Is Nothing Then
                Return False
            End If
            Return Storage.GetIsLoadedItem(index)
        End Function

        Private privateStorage As DataStorage
        Protected Property Storage() As DataStorage
            Get
                Return privateStorage
            End Get
            Private Set(ByVal value As DataStorage)
                privateStorage = value
            End Set
        End Property
        Private privateRequestData As RequestData
        Protected Property RequestData() As RequestData
            Get
                Return privateRequestData
            End Get
            Private Set(ByVal value As RequestData)
                privateRequestData = value
            End Set
        End Property
        Private privateRequestCount As RequestCount
        Protected Property RequestCount() As RequestCount
            Get
                Return privateRequestCount
            End Get
            Private Set(ByVal value As RequestCount)
                privateRequestCount = value
            End Set
        End Property
        Private privateSubmitChanges As SubmitChanges
        Protected Property SubmitChanges() As SubmitChanges
            Get
                Return privateSubmitChanges
            End Get
            Private Set(ByVal value As SubmitChanges)
                privateSubmitChanges = value
            End Set
        End Property
        Protected ReadOnly Property IsReadOnlyCore() As Boolean
            Get
                Return SubmitChanges Is Nothing
            End Get
        End Property

        Protected MustOverride Function GetItem(ByVal index As Integer) As Object
        Protected MustOverride Sub SetItem(ByVal index As Integer, ByVal value As Object)
        Protected Overridable Sub RaiseOnLoadedItemsCountUpdated()
            RaiseEvent LoadedItemsCountUpdated(Me, EventArgs.Empty)
        End Sub
        Protected Overridable Sub RaiseOnLoadingCompleted()
            RaiseEvent LoadingCompleted(Me, EventArgs.Empty)
        End Sub

        Protected MustOverride Function GetEnumeratorCore() As IEnumerator
        Protected Overridable Function IndexOf_Core(ByVal value As Object) As Integer
            If value Is Nothing Then
                Return -1
            End If
            For i As Integer = 0 To Count - 1
                If Storage(i) Is value Then
                    Return i
                End If
            Next i
            Return -1
        End Function
        Protected Overridable Sub Insert_Core(ByVal index As Integer, ByVal value As Object)
            If IsReadOnlyCore Then
                Throw New InvalidOperationException("This is read only collection. To enable editing you should create a collection with submitChanges event hadler.")
            End If
            RaiseSubmitChanges(TypeSubmitChanges.Add, Nothing, value, index)
        End Sub
        Protected Overridable Sub RemoveAt_Core(ByVal index As Integer)
            If IsReadOnlyCore Then
                Throw New InvalidOperationException("This is read only collection. To enable editing you should create a collection with submitChanges event hadler.")
            End If
            RaiseSubmitChanges(TypeSubmitChanges.Remove, Storage(index), Nothing, index)
        End Sub

        Protected Overridable Sub RaiseGetData(ByVal skipCount As Integer, ByVal takeCount As Integer, Optional ByVal isBackgroundRequest As Boolean = False)
            Dim startIndex As Integer = Storage.IndexOfFirstNotInitializedItemOnInterval(skipCount, takeCount)
            Dim endIndex As Integer = startIndex + takeCount
            If endIndex > Count Then
                endIndex = Count
            End If
            endIndex = Storage.IndexOfFirstInitializedItemOnInterval(startIndex, endIndex - startIndex)
            skipCount = startIndex
            takeCount = endIndex - startIndex
            If takeCount = 0 Then
                Return
            End If
            Storage.InitializeItemsLoading(skipCount, takeCount)
            Dim requestArgs As New RequestDataArgs(skipCount, takeCount, isBackgroundRequest)
            Dim requestResult As New RequestDataResult(requestArgs, AddressOf RequestDataFeedback)
            RequestData()(requestArgs, requestResult)
        End Sub
        Protected Overridable Sub RaiseGetCount()
            Dim requestArgs As New RequestCountArgs()
            Dim requestResult As New RequestCountResult(requestArgs, AddressOf RequestCountFeedback)
            RequestCount()(requestArgs, requestResult)
        End Sub
        Protected Overridable Sub RaiseSubmitChanges(ByVal type As TypeSubmitChanges, ByVal oldItem As Object, ByVal newItem As Object, ByVal index As Integer)
            Dim args As New SubmitChangesArgs(TypeSubmitChanges.Add, oldItem, newItem, index)
            Dim result As New SubmitChangesResult(args, AddressOf SubmitChangesFeedback)
            SubmitChanges()(args, result)
        End Sub

        Protected MustOverride Sub RequestCountFeedback(ByVal requestResult As RequestCountResult)
        Protected MustOverride Sub RequestDataFeedback(ByVal requestResult As RequestDataResult)
        Protected MustOverride Sub SubmitChangesFeedback(ByVal resultFeedback As SubmitChangesResult)


        Private requestDataRate_Renamed As Integer = -1
        Private loadedItemsCount As Integer = 0

        #Region "NotImplementedException"
        Private Sub ICollection_CopyTo(ByVal array As Array, ByVal index As Integer) Implements ICollection.CopyTo
            Throw New NotImplementedException()
        End Sub
        Private ReadOnly Property ICollection_IsSynchronized() As Boolean Implements ICollection.IsSynchronized
            Get
                Throw New NotImplementedException()
            End Get
        End Property
        Private ReadOnly Property ICollection_SyncRoot() As Object Implements ICollection.SyncRoot
            Get
                Throw New NotImplementedException()
            End Get
        End Property
        #End Region
        #Region "inner classes"
        Protected Class DataStorageItem
            Private privateData As Object
            Public Property Data() As Object
                Get
                    Return privateData
                End Get
                Private Set(ByVal value As Object)
                    privateData = value
                End Set
            End Property
            Private privateIsLoaded As Boolean
            Public Property IsLoaded() As Boolean
                Get
                    Return privateIsLoaded
                End Get
                Private Set(ByVal value As Boolean)
                    privateIsLoaded = value
                End Set
            End Property
            Private privateIsLoading As Boolean
            Public Property IsLoading() As Boolean
                Get
                    Return privateIsLoading
                End Get
                Private Set(ByVal value As Boolean)
                    privateIsLoading = value
                End Set
            End Property
            Public Sub StartLoading()
                Data = Nothing
                IsLoaded = False
                IsLoading = True
            End Sub
            Public Sub EndLoading(ByVal data As Object)
                If (Not IsLoading) OrElse IsLoaded Then
                    Debug.WriteLine("Cannot finish DataItem loading process before loading process initialization. Call the StartLoading method first.")
                End If
                Me.Data = data
                IsLoaded = True
                IsLoading = False
            End Sub
            Public Sub Clear()
                Data = Nothing
                IsLoaded = False
                IsLoading = False
            End Sub
            Public Sub New()
                Data = Nothing
                IsLoaded = False
                IsLoading = False
            End Sub
        End Class
        Protected Class DataStorage
            Implements IDisposable

            Private privateIsInitialized As Boolean
            Public Property IsInitialized() As Boolean
                Get
                    Return privateIsInitialized
                End Get
                Private Set(ByVal value As Boolean)
                    privateIsInitialized = value
                End Set
            End Property
            Private privateIsCompleted As Boolean
            Public Property IsCompleted() As Boolean
                Get
                    Return privateIsCompleted
                End Get
                Private Set(ByVal value As Boolean)
                    privateIsCompleted = value
                End Set
            End Property
            Public Event OnCompleted As EventHandler
            Public Event OnInitialized As EventHandler

            Default Public Property Item(ByVal index As Integer) As Object
                Get
                    Return Items(index).Data
                End Get
                Set(ByVal value As Object)
                    Items(index).EndLoading(value)
                End Set
            End Property
            Public ReadOnly Property Count() As Integer
                Get
                    Return count_Renamed
                End Get
            End Property

            Public Sub New()
                IsInitialized = False
            End Sub
            Public Sub Dispose() Implements IDisposable.Dispose
                Uninitialize()
                If Items IsNot Nothing Then
                    Items.Clear()
                End If
            End Sub
            Public Sub Initialize(ByVal count As Integer)
                If Items IsNot Nothing Then
                    If count < Me.Count Then
                        Items.RemoveRange(count, Me.Count - count)
                    End If
                    If count > Me.Count Then
                        Dim i As Integer = Me.Count
                        Do While i < count
                            Items.Add(New DataStorageItem())
                            i += 1
                        Loop
                    End If
                    If Items.Count <> count Then
                        Throw New InvalidOperationException("What a fuck!!")
                    End If
                End If
                Me.count_Renamed = count
                If Items Is Nothing Then
                    CreateItemsList()
                End If
                IsInitialized = True
                RaiseEvent OnInitialized(Me, EventArgs.Empty)
                CheckCompleted()
            End Sub
            Public Sub Uninitialize()
                If Not IsInitialized Then
                    Return
                End If
                IsCompleted = False
                IsInitialized = False
            End Sub
            Public Sub LoadData(ByVal startIndex As Integer, ByVal dataEnumerator As IEnumerator)
                dataEnumerator.Reset()
                Dim index As Integer = startIndex
                Do While dataEnumerator.MoveNext()
                    Me(index) = dataEnumerator.Current
                    index += 1
                Loop
                CheckCompleted()
            End Sub
            Public Sub UnloadData(ByVal startIndex As Integer, ByVal count As Integer)
                If startIndex + count > Me.Count Then
                    count = Me.Count - startIndex
                End If
                For i As Integer = startIndex To count - 1
                    Items(i).Clear()
                Next i
                IsCompleted = False
            End Sub
            Public Function GetIsLoadedItem(ByVal index As Integer) As Boolean
                Return Items(index).IsLoaded
            End Function
            Public Function GetIsLoadingItem(ByVal index As Integer) As Boolean
                Return Items(index).IsLoading
            End Function
            Public Sub InitializeItemsLoading(ByVal index As Integer, ByVal count As Integer)
                For i As Integer = index To (index + count) - 1
                    InitializeItemLoading(i)
                Next i
            End Sub
            Public Sub InitializeItemLoading(ByVal index As Integer)
                Items(index).StartLoading()
            End Sub
            Public Function IndexOfFirstInitializedItemOnInterval(ByVal index As Integer, ByVal count As Integer) As Integer
                Dim endIndex As Integer = index + count
                If endIndex > Me.Count Then
                    endIndex = Me.Count
                End If
                For i As Integer = index To endIndex - 1
                    If Items(i).IsLoaded OrElse Items(i).IsLoading Then
                        Return i
                    End If
                Next i
                Return endIndex
            End Function
            Public Function IndexOfFirstNotInitializedItemOnInterval(ByVal index As Integer, ByVal count As Integer) As Integer
                Dim endIndex As Integer = index + count
                If endIndex > Me.Count Then
                    endIndex = Me.Count
                End If
                For i As Integer = index To (index + count) - 1
                    If (Not Items(i).IsLoaded) OrElse (Not Items(i).IsLoading) Then
                        Return i
                    End If
                Next i
                Return Me.Count
            End Function
            Public Function IndexOfFirstNotLoadedItem() As Integer
                For i As Integer = 0 To Count - 1
                    If Not Items(i).IsLoaded Then
                        Return i
                    End If
                Next i
                Return Count
            End Function
            Public Function GetLoadedItemsCount() As Integer
                Dim res As Integer = 0
                For i As Integer = 0 To Count - 1
                    If GetIsLoadedItem(i) Then
                        res += 1
                    End If
                Next i
                Return res
            End Function
            Private Sub CheckCompleted()
                For i As Integer = 0 To Count - 1
                    If Not GetIsLoadedItem(i) Then
                        Return
                    End If
                Next i
                IsCompleted = True
                RaiseEvent OnCompleted(Me, EventArgs.Empty)
            End Sub
            Private Sub CreateItemsList()
                Items = New List(Of DataStorageItem)()
                For i As Integer = 0 To count_Renamed - 1
                    Items.Add(New DataStorageItem())
                Next i
            End Sub

            Private Items As List(Of DataStorageItem)

            Private count_Renamed As Integer
        End Class
        Protected Class EnumeratorBase
            Implements IEnumerator

            Public Sub New(ByVal owner As AsynchronousCollectionBase)
                Me.owner = owner
                Me.index = -1
                current_Renamed = Nothing
            End Sub
            Public Sub Dispose()
                current_Renamed = Nothing
                owner = Nothing
            End Sub
            Private ReadOnly Property IEnumerator_Current() As Object Implements IEnumerator.Current
                Get
                    Return current_Renamed
                End Get
            End Property
            Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
                index += 1
                Dim listOwner As IList = (DirectCast(owner, IList))
                If index < owner.Count Then
                    current_Renamed = listOwner(index)
                    Return True
                End If
                current_Renamed = Nothing
                Return False
            End Function
            Public Sub Reset() Implements IEnumerator.Reset
                index = -1
                current_Renamed = Nothing
            End Sub

            Private owner As AsynchronousCollectionBase
            Private index As Integer

            Private current_Renamed As Object
        End Class
        #End Region
    End Class

    ''' <summary>
    ''' Represents a collection of objects that can be asynchronously loaded on demand.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class AsynchronousCollection(Of T)
        Inherits AsynchronousCollectionBase
        Implements IList(Of T)

        Public Overrides ReadOnly Property Count() As Integer Implements System.Collections.Generic.ICollection(Of T).Count
            Get
                Return If(Storage IsNot Nothing, Storage.Count, 0)
            End Get
        End Property
        Default Public Overloads Property Item(ByVal index As Integer) As T Implements IList(Of T).Item
            Get
                Dim [me] As IList = DirectCast(Me, IList)
                If [me](index) Is Nothing Then
                    Return Nothing
                End If
                Return DirectCast([me](index), T)
            End Get
            Set(ByVal value As T)
                Dim [me] As IList = DirectCast(Me, IList)
                [me](index) = value
            End Set
        End Property

        Private ReadOnly Property ICollection_IsReadOnly As Boolean Implements ICollection(Of T).IsReadOnly
            Get
                Return IsReadOnly
            End Get
        End Property

        Public Sub New(ByVal requestCount As RequestCount, ByVal requestData As RequestData)
            Me.New(requestCount, requestData, Nothing, AsynchronousCollectionSettings.Default)
        End Sub
        Public Sub New(ByVal requestCount As RequestCount, ByVal requestData As RequestData, ByVal settings As AsynchronousCollectionSettings)
            Me.New(requestCount, requestData, Nothing, settings)
        End Sub
        Public Sub New(ByVal requestCount As RequestCount, ByVal requestData As RequestData, ByVal submitChanges As SubmitChanges)
            Me.New(requestCount, requestData, submitChanges, AsynchronousCollectionSettings.Default)
        End Sub
        Public Sub New(ByVal requestCount As RequestCount, ByVal requestData As RequestData, ByVal submitChanges As SubmitChanges, ByVal settings As AsynchronousCollectionSettings)
            MyBase.New(requestCount, requestData, submitChanges)
            settings.Apply(Of T)(Me)
            AddHandler Storage.OnInitialized, AddressOf OnStorageInitialized
            AddHandler Storage.OnCompleted, AddressOf OnStorageCompleted
            RaiseGetCount()
        End Sub
        Public Overrides Sub Dispose()
            RemoveHandler Storage.OnInitialized, AddressOf OnStorageInitialized
            RemoveHandler Storage.OnCompleted, AddressOf OnStorageCompleted
            MyBase.Dispose()
        End Sub
        Public Overrides Sub UpdateCount()
            RaiseGetCount()
            UpdateLoadedItemsCount()
        End Sub
        Public Overrides Sub UpdateData(ByVal startIndex As Integer, ByVal count As Integer)
            Storage.UnloadData(startIndex, count)
            UpdateLoadedItemsCount()
            For i As Integer = startIndex To (startIndex + count) - 1
                RaiseCollectionRemove(Me(i), i)
            Next i
        End Sub
        Public Overloads Function GetEnumerator() As IEnumerator(Of T) Implements System.Collections.Generic.IEnumerable(Of T).GetEnumerator
            Return DirectCast(GetEnumeratorCore(), IEnumerator(Of T))
        End Function
        Public Overloads Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf
            Dim [me] As IList = Me
            Return [me].IndexOf(item)
        End Function
        Public Overloads Sub Add(ByVal item As T) Implements System.Collections.Generic.ICollection(Of T).Add
            Dim [me] As IList = Me
            [me].Add(item)
        End Sub
        Public Overloads Function Contains(ByVal item As T) As Boolean Implements System.Collections.Generic.ICollection(Of T).Contains
            Dim [me] As IList = Me
            Return [me].Contains(item)
        End Function
        Public Overloads Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert
            Dim [me] As IList = Me
            [me].Insert(index, item)
        End Sub
        Public Overloads Function Remove(ByVal item As T) As Boolean Implements System.Collections.Generic.ICollection(Of T).Remove
            If Contains(item) Then
                Dim [me] As IList = Me
                [me].Remove(item)
                Return True
            End If
            Return False
        End Function
        Public Overloads Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt
            Dim [me] As IList = Me
            [me].RemoveAt(index)
        End Sub

        Protected Overrides Function GetEnumeratorCore() As IEnumerator
            Return New Enumerator(Me)
        End Function
        Protected Overrides Function GetItem(ByVal index As Integer) As Object
            If Not Storage.GetIsLoadedItem(index) Then
                If Storage.GetIsLoadingItem(index) Then
                    Return Nothing
                End If
                Dim interval As Integer = If(index + RequestDataRate < Count, RequestDataRate, Count - index)
                Dim endIndex As Integer = Storage.IndexOfFirstInitializedItemOnInterval(index, interval)
                RaiseGetData(index, endIndex - index)
                Return Nothing
            End If
            If Storage(index) Is Nothing Then
                Return Nothing
            End If
            Return DirectCast(Storage(index), T)
        End Function
        Protected Overrides Sub SetItem(ByVal index As Integer, ByVal value As Object)
            If IsReadOnlyCore Then
                Throw New InvalidOperationException("This is read only collection.")
            End If
            RaiseSubmitChanges(TypeSubmitChanges.Replace, Storage(index), value, index)
        End Sub

        Protected Overridable Sub OnStorageInitialized(ByVal sender As Object, ByVal e As EventArgs)
            UpdateLoadedItemsCount()
            OnPropertyChanged("Count")
        End Sub
        Protected Overridable Sub OnStorageCompleted(ByVal sender As Object, ByVal e As EventArgs)
        End Sub

        Protected Overridable Sub Initialize(ByVal count As Integer)
            If Storage Is Nothing Then
                Return
            End If
            Storage.Uninitialize()
            Storage.Initialize(count)
            RaiseCollectionReset()
        End Sub

        Protected Overrides Sub RequestCountFeedback(ByVal requestResult As RequestCountResult)
            Initialize(requestResult.Count)
        End Sub
        Protected Overrides Sub RequestDataFeedback(ByVal requestResult As RequestDataResult)
            LoadData(DirectCast(requestResult.RequestArgs, RequestDataArgs).SkipCount, requestResult.Data.GetEnumerator())
        End Sub
        Protected Overrides Sub SubmitChangesFeedback(ByVal resultFeedback As SubmitChangesResult)
            If resultFeedback.Cancel Then
                Return
            End If
            Dim args As SubmitChangesArgs = DirectCast(resultFeedback.RequestArgs, SubmitChangesArgs)
            UpdateCount()
            If args.Type = TypeSubmitChanges.Replace OrElse args.Type = TypeSubmitChanges.Update Then
                UpdateData(args.Index, 1)
            End If
        End Sub

        Protected Overridable Sub LoadData(ByVal startIndex As Integer, ByVal dataEnumerator As IEnumerator)
            If Storage Is Nothing Then
                Return
            End If
            Dim data As New List(Of Object)()
            dataEnumerator.Reset()
            Do While dataEnumerator.MoveNext()
                data.Add(dataEnumerator.Current)
            Loop
            Storage.LoadData(startIndex, dataEnumerator)
            RaiseCollectionAdd(data, startIndex)
            UpdateLoadedItemsCount()
        End Sub
        Protected Overridable Sub UpdateLoadedItemsCount()
            LoadedCount = Storage.GetLoadedItemsCount()
            RaiseOnLoadedItemsCountUpdated()
            If Storage.IsCompleted Then
                RaiseOnLoadingCompleted()
            End If
        End Sub

#Region "NotImplementedException"
        Private Sub ICollectionGeneric_CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) Implements ICollection(Of T).CopyTo
            Throw New NotImplementedException()
        End Sub

        Private Sub ICollection_Clear() Implements ICollection(Of T).Clear
            Throw New NotImplementedException()
        End Sub
#End Region
#Region "inner classes"
        Protected Class Enumerator
            Inherits EnumeratorBase
            Implements IEnumerator(Of T)

            Public Sub New(ByVal owner As AsynchronousCollectionBase)
                MyBase.New(owner)
            End Sub
            Public ReadOnly Overloads Property Current() As T Implements IEnumerator(Of T).Current
                Get
                    Dim [me] As IEnumerator = Me
                    If [me].Current Is Nothing Then
                        Return Nothing
                    End If
                    Return DirectCast([me].Current, T)
                End Get
            End Property

            Private Sub IDisposable_Dispose() Implements IDisposable.Dispose
                Dispose()
            End Sub
        End Class
#End Region
    End Class

    ''' <summary>
    ''' Represents a collection of objects that can be asynchronously loaded in background thread.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class AsynchronousCollection2(Of T)
        Inherits AsynchronousCollection(Of T)

        Public Property BackgroundRequestDataRate() As Integer
            Get
                Return backgroundRequestDataRate_Renamed
            End Get
            Set(ByVal value As Integer)
                If value <= 0 Then
                    Throw New ArgumentException("Value must be more than zero")
                End If
                backgroundRequestDataRate_Renamed = value
            End Set
        End Property

        Private backgroundRequestDataRate_Renamed As Integer = -1
        Public Property BackgroundRequestDataInterval() As TimeSpan
            Get
                Return backgroundRequestDataInterval_Renamed
            End Get
            Set(ByVal value As TimeSpan)
                backgroundRequestDataInterval_Renamed = value
            End Set
        End Property

        Private backgroundRequestDataInterval_Renamed As TimeSpan = TimeSpan.Zero

        Protected requestDataCounter As Integer = 0
        Private privateRequestDataTimer As DispatcherTimer
        Protected Property RequestDataTimer() As DispatcherTimer
            Get
                Return privateRequestDataTimer
            End Get
            Private Set(ByVal value As DispatcherTimer)
                privateRequestDataTimer = value
            End Set
        End Property


        Public Sub New(ByVal requestCount As RequestCount, ByVal requestData As RequestData)
            Me.New(requestCount, requestData, Nothing, AsynchronousCollection2Settings.Default)
        End Sub
        Public Sub New(ByVal requestCount As RequestCount, ByVal requestData As RequestData, ByVal setting As AsynchronousCollection2Settings)
            Me.New(requestCount, requestData, Nothing, setting)
        End Sub
        Public Sub New(ByVal requestCount As RequestCount, ByVal requestData As RequestData, ByVal submitChanges As SubmitChanges)
            Me.New(requestCount, requestData, submitChanges, AsynchronousCollection2Settings.Default)
        End Sub
        Public Sub New(ByVal requestCount As RequestCount, ByVal requestData As RequestData, ByVal submitChanges As SubmitChanges, ByVal setting As AsynchronousCollection2Settings)
            MyBase.New(requestCount, requestData, submitChanges, setting)
            RequestDataTimer = New DispatcherTimer()
        End Sub

        Public Overrides Sub Dispose()
            DisableRequestDataTimer()
            MyBase.Dispose()
            RequestDataTimer = Nothing
        End Sub
        Public Overrides Sub UpdateData(ByVal startIndex As Integer, ByVal count As Integer)
            MyBase.UpdateData(startIndex, count)
            If (Not Storage.IsCompleted) AndAlso (Not RequestDataTimer.IsEnabled) Then
                EnableRequestDataTimer()
            End If
        End Sub
        Protected Overridable Sub CustomizeRequestDataTimer()
            RequestDataTimer.Interval = BackgroundRequestDataInterval
        End Sub
        Protected Overrides Sub OnStorageInitialized(ByVal sender As Object, ByVal e As EventArgs)
            MyBase.OnStorageInitialized(sender, e)
            EnableRequestDataTimer()
        End Sub
        Protected Overrides Sub OnStorageCompleted(ByVal sender As Object, ByVal e As EventArgs)
            DisableRequestDataTimer()
        End Sub
        Protected Overridable Sub EnableRequestDataTimer()
            CustomizeRequestDataTimer()
            AddHandler RequestDataTimer.Tick, AddressOf OnRequestDataTimerTick
            RequestDataTimer.Start()
        End Sub
        Protected Overridable Sub DisableRequestDataTimer()
            RequestDataTimer.Stop()
            RemoveHandler RequestDataTimer.Tick, AddressOf OnRequestDataTimerTick
        End Sub
        Protected Overridable Sub OnRequestDataTimerTick(ByVal sender As Object, ByVal e As EventArgs)
            If requestDataCounter > 0 OrElse (Not Storage.IsInitialized) Then
                Return
            End If
            Dim skipCount As Integer = Storage.IndexOfFirstNotLoadedItem()
            Dim endIndex As Integer = Storage.IndexOfFirstInitializedItemOnInterval(skipCount, BackgroundRequestDataRate)
            RaiseGetData(skipCount, endIndex - skipCount, True)
        End Sub
        Protected Overrides Sub RaiseGetData(ByVal skipCount As Integer, ByVal takeCount As Integer, Optional ByVal isBackgroundRequest As Boolean = False)
            requestDataCounter += 1
            MyBase.RaiseGetData(skipCount, takeCount)
        End Sub
        Protected Overrides Sub RequestDataFeedback(ByVal requestResult As RequestDataResult)
            MyBase.RequestDataFeedback(requestResult)
            requestDataCounter -= 1
        End Sub
    End Class
End Namespace