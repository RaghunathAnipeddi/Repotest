using System.Collections.Generic;
using ChemPoint.GP.Entities.Business_Entities;

namespace Chempoint.GP.Model.Interactions.Sales
{
    public enum ResponseStatus
    {
        Success = 0,
        Error = 1,
        Unknown = 2,
        Custom = 3,
        NoRecord = 4,
        Partial = 5
    }

    public class SalesOrderResponse
    {
        public List<int> AddressCount { get; set; }

        public ResponseStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public SalesOrderEntity SalesOrderDetails { get; set; }

        public int ValidateStatus { get; set; }

        public ResponseStatus TermHoldInventoryStatus { get; set; }

        public string LogginMessage { get; set; }

    }

}
