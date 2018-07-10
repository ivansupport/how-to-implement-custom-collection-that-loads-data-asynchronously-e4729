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
Imports DevExpress.Xpf.Core
Imports DevExpress.Xpf.Grid
Imports DevExpress.Xpf.Editors
Imports System.Collections
Namespace AsyncDataLoading
    ''' <summary>
    ''' Interaction logic for Content.xaml
    ''' </summary>
    Partial Public Class Content
        Inherits UserControl

        Public Shared ReadOnly DataSourceManagerProperty As DependencyProperty = DependencyProperty.Register("DataSourceManager", GetType(DataSourceManager), GetType(Content), New PropertyMetadata(Nothing, Sub(d, e) CType(d, Content).OnDataSourceManagerChanged(CType(e.OldValue, DataSourceManager))))
        Public Shared ReadOnly IsSettingsEnabledProperty As DependencyProperty = DependencyProperty.Register("IsSettingsEnabled", GetType(Boolean), GetType(Content), New PropertyMetadata(True))
        Public Property DataSourceManager() As DataSourceManager
            Get
                Return DirectCast(GetValue(DataSourceManagerProperty), DataSourceManager)
            End Get
            Set(ByVal value As DataSourceManager)
                SetValue(DataSourceManagerProperty, value)
            End Set
        End Property
        Public Property IsSettingsEnabled() As Boolean
            Get
                Return DirectCast(GetValue(IsSettingsEnabledProperty), Boolean)
            End Get
            Set(ByVal value As Boolean)
                SetValue(IsSettingsEnabledProperty, value)
            End Set
        End Property

        Public Sub New()
            InitializeComponent()
        End Sub
        Private Sub OnDataSourceManagerChanged(ByVal oldManager As DataSourceManager)
            If DataSourceManager IsNot Nothing Then
                AddHandler DataSourceManager.LoadingCompleted, AddressOf OnLoadingCompleted
                AddHandler DataSourceManager.DataSourceUninitialized, AddressOf OnDataSourceUninitialized
            End If
            If oldManager IsNot Nothing Then
                RemoveHandler oldManager.LoadingCompleted, AddressOf OnLoadingCompleted
                RemoveHandler oldManager.DataSourceUninitialized, AddressOf OnDataSourceUninitialized
            End If

        End Sub
        Private Sub OnDataSourceUninitialized(ByVal sender As DataSourceManagerBase, ByVal oldDataSource As AsynchronousCollectionBase)
            SetIsSettingsEnabled(False)
        End Sub
        Private Sub OnLoadingCompleted(ByVal sender As DataSourceManagerBase, ByVal dataSource As AsynchronousCollectionBase)
            SetIsSettingsEnabled(True)
        End Sub
        Private Sub SetIsSettingsEnabled(ByVal value As Boolean)
            'IsSettingsEnabled = value;
        End Sub
        Private Sub ThemeButtonClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
            ThemeManager.ApplicationThemeName = DirectCast(sender, Button).Content.ToString()
        End Sub
        Private Sub ApplySettingsButtonClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim sett As AsynchronousCollectionSettings
            If sett1Check.IsChecked.Value Then
                sett = AsynchronousCollectionSettingsFactory.CreateOnDemandRequestDataModeSettings(CInt((requestDataRateEd.Value)))
            Else
                sett = AsynchronousCollectionSettingsFactory.CreateInBackgroundThreadRequestDataModeSettings(CInt((requestDataRateEd.Value)))
            End If
            DataSourceManager.Uninitialize()
            DataSourceManager.Settings = sett
            DataSourceManager.Initialize()
        End Sub
    End Class
End Namespace
