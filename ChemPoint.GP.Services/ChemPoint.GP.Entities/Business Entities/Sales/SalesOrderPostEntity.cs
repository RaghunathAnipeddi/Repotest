using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities;
using System;

namespace ChemPoint.Entities.Business_Entities.Sales
{
    public class SalesOrderPostEntity : IModelBase
    {
        public string BatchNumber { get; set; }

        public int Remove { get; set; }

        public int TotalInvoicesInBatch { get; set; }

        public Decimal TotalBatchAmount { get; set; }

        public int TotalTransInBatch { get; set; }

        public int NumTranIdentified { get; set; }

        public int NumTranProcessed { get; set; }

        public bool IsUsdccTransactionsAvailable { get; set; }

        public int TotalOrdersWithInterCompanySalesIssue { get; set; }

        public int TotalOrdersWithCurrencyIssue { get; set; }

        public int TotalOrdersWithDropshipIssue { get; set; }

        public int TotalOrdersWithAccountNumberIssue { get; set; }

        public int TotalOrdersWithDocumentAmountIssue { get; set; }

        public int TotalOrdersWithTaxIssue { get; set; }

        public int TotalOrdersWithShipViaIssue { get; set; }

        public int TotalOrdersWithCreditCard { get; set; }

        public int TotalOrdersWithDistributionIssue { get; set; }

        public int TotalTransactionsPosted { get; set; }

        public string OpenOrderReportPath { get; set; }

        public SalesOrderEntity[] SalesOrders { get; set; }
    }
}
