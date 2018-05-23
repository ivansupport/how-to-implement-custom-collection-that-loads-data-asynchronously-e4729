using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfService {
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "EmployeeService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select EmployeeService.svc or EmployeeService.svc.cs at the Solution Explorer and start debugging.
    public class OrderService : IOrderService {
        NorthwindEntities1 NorthwindEntities = new NorthwindEntities1();

        public IList<Order> GetRecords(int skipCount, int takeCount) {
            var res = from em in NorthwindEntities.Orders.OrderBy((or) => or.OrderID).Skip(skipCount).Take(takeCount)
                            select em;
            return res.ToList();
        }

        public int GetRecordsCount() {
            return NorthwindEntities.Orders.Count();
        }
    }
}
