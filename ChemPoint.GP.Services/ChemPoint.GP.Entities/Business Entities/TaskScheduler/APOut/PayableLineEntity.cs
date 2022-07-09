using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut
{
    public class PayableLineEntity : IModelBase
    {
        public string CreditAccountNumber { get; set; }
        public int CurrencyDecimal { get; set; }
        public string UserId { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime WeekendingDate { get; set; }
        public string VendorId { get; set; }
        public decimal TotalApprovedDocumentAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public string VendorName { get; set; }
        public string DocumentNumber { get; set; }
        public string CurrencyId { get; set; }
        public string GLAccountNumber { get; set; }
        public string GLAccountDescription { get; set; }
        public string DocumentType { get; set; }
        public string ErrorDescription { get; set; }
        public string VoucherNumber { get; set; }
        public string ValidDocumentNumber { get; set; }


        //CTSI Re Upload properties
        public string GLAccount { get; set; }
        public decimal FreightAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal MiscellaneousAmount { get; set; }
        public decimal TradeDiscounts { get; set; }
        public decimal PurchaseAmount { get; set; }
        public string CPTReference { get; set; }
        public string AirwayInvoiceNumber { get; set; }
        public string CtsiId { get; set; }
        public string GPVoucherNumber { get; set; }
        public decimal BaseLocalCharge { get; set; }
        public decimal BaseZeroRatedCharge { get; set; }
        public decimal BaseReverseCharge { get; set; }
        public string BaseChargeType { get; set; }

        //CTSI Tax 
        public string BaseCharge { get; set; }
        public string TaxDetailId { get; set; }
        public decimal TaxPercentage { get; set; }
        public string TaxDetailIdDescription { get; set; }

        public int OriginalCTSIInvoiceId { get; set; }
        public string CurrencyCode { get; set; }
        public int CtsiFileId { get; set; }
        public int StatusId { get; set; }
        public decimal OverCharge { get; set; }
        public string Notes { get; set; }
        public string OtherDuplicates { get; set; }
        public int DebitDistributionType { get; set; }
        public decimal CreditAmount { get; set; }
        public string CtsiStatus { get; set; }

        //API EMEA
        public string DocumentRowId { get; set; }
        public decimal DocumentAmount { get; set; }
        public DateTime ReceiptDate { get; set; }

        // API EMEA Upload properties
        // For Non-PO
        public int OriginalApiInvoiceId { get; set; }
        public string DocumentId { get; set; }
        public string DocumentTypeName { get; set; }
        public string LocationName { get; set; }
        public string ApiText1 { get; set; }
        public string TaxScheduleId { get; set; }
        //public string CurrencyId { get; set; }
        public string DocumentIdStatus { get; set; }
        // For PO
        public string PurchaseOrderNumber { get; set; }
        public int POLineNumber { get; set; }
        public string ItemNumber { get; set; }
        public string ReceiptNumber { get; set; }
        public int ReceiptLineNumber { get; set; }
        public decimal QuantityShipped { get; set; }
        public decimal AdjustedItemUnitQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal ExtendedCost { get; set; }
        public decimal AdjustedItemUnitPrice { get; set; }
        public int GLIndex { get; set; }
        public int APIInvoiceId { get; set; }
        public decimal POAmount { get; set; }
        public DateTime PODateOpened { get; set; }
        public decimal ItemUnitPrice { get; set; }
        public string ShipToState { get; set; }
        public DateTime ShippedDate { get; set; }
        public int FormTypeCode { get; set; }
        public int IsDuplicated { get; set; }
        public int RequiredDistribution { get; set; }
        public decimal ItemUnitQty { get; set; }

       
    }   
}
