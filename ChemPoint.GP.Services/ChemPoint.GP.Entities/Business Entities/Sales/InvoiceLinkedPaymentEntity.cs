using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.Business_Entities.Sales
{
    public class InvoiceLinkedPaymentEntity : IModelBase
    {
        public CustomerInformation CustomerInfo { get; set; }

        public OrderHeader OrderInfo { get; set; }

        public string PaymentNumber { get; set; }

        public string ReturnNumber { get; set; }

        public string CreditNumber { get; set; }

        public string ErrorDescription { get; set; }

    }
}
