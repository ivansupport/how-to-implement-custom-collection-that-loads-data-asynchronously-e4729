'------------------------------------------------------------------------------
' <auto-generated>
'    This code was generated from a template.
'
'    Manual changes to this file may cause unexpected behavior in your application.
'    Manual changes to this file will be overwritten if the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic

Partial Public Class Order
    Public Property OrderID As Integer
    Public Property CustomerID As String
    Public Property EmployeeID As Nullable(Of Integer)
    Public Property OrderDate As Nullable(Of Date)
    Public Property RequiredDate As Nullable(Of Date)
    Public Property ShippedDate As Nullable(Of Date)
    Public Property ShipVia As Nullable(Of Integer)
    Public Property Freight As Nullable(Of Decimal)
    Public Property ShipName As String
    Public Property ShipAddress As String
    Public Property ShipCity As String
    Public Property ShipRegion As String
    Public Property ShipPostalCode As String
    Public Property ShipCountry As String

End Class
