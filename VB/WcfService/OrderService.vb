Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.Serialization
Imports System.ServiceModel
Imports System.Text

Namespace WcfService
    ' NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEmployeeService" in both code and config file together.
    <ServiceContract> _
    Public Interface IOrderService
        <OperationContract> _
        Function GetRecords(ByVal skipCount As Integer, ByVal takeCount As Integer) As IList(Of Order)
        <OperationContract> _
        Function GetRecordsCount() As Integer
    End Interface
End Namespace
