Imports System
Imports System.Collections.Generic
Imports System.Data.SqlClient
Imports System.Linq
Imports System.Runtime.Serialization
Imports System.ServiceModel
Imports System.Text

Namespace WcfService
    ' NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "EmployeeService" in code, svc and config file together.
    ' NOTE: In order to launch WCF Test Client for testing this service, please select EmployeeService.svc or EmployeeService.svc.cs at the Solution Explorer and start debugging.
    Public Class OrderService
        Implements IOrderService

        Private NorthwindEntities As New NorthwindEntities1()

        Public Function GetRecords(ByVal skipCount As Integer, ByVal takeCount As Integer) As IList(Of Order) Implements IOrderService.GetRecords
            Dim res = From em In NorthwindEntities.Orders.OrderBy(Function([or]) [or].OrderID).Skip(skipCount).Take(takeCount) _
                Select em
            Return res.ToList()
        End Function

        Public Function GetRecordsCount() As Integer Implements IOrderService.GetRecordsCount
            Return NorthwindEntities.Orders.Count()
        End Function
    End Class
End Namespace
