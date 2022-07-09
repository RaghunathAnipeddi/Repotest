using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class CustomerMappingDetails : IModelBase
    {
        public string EftCTXCustomerReference { get; set; }
        public string CustomerId { get; set; }
        public string Type { get; set; }
        public int EftCustomerMappingId { get; set; }

    }
}
