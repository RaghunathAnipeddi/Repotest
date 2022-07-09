using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Infrastructure.Maps.Purchase
{
        public class PayableManagementEUCTSIMap : BaseDataTableMap<PayableLineEntity>
        { 
        public override PayableLineEntity Map(DataRow dr)
        {
            PayableLineEntity payableEntity = new PayableLineEntity();
            int result;
            DateTime dateResult;
            decimal decimalResult;
            int.TryParse(dr["OriginialCTSIInvoiceId"].ToString(), out result);
            payableEntity.OriginalCTSIInvoiceId = result;
            int.TryParse(dr["CTSIFileId"].ToString(), out result);
            payableEntity.CtsiFileId = result;
            payableEntity.CtsiId = dr["CTSIId"].ToString();
            payableEntity.DocumentTypeName = dr["DocumentType"].ToString();
            payableEntity.DocumentNumber = dr["DocumentNumber"].ToString();
            DateTime.TryParse(dr["DocumentDate"].ToString(), out dateResult);
            payableEntity.DocumentDate = dateResult;
            payableEntity.VendorName = dr["VendorId"].ToString();
            decimal.TryParse(dr["TotalApprovedDocumentAmount"].ToString(), out decimalResult);
            payableEntity.TotalApprovedDocumentAmount = decimalResult;
            decimal.TryParse(dr["ApprovedAmount"].ToString(), out decimalResult);
            payableEntity.ApprovedAmount = decimalResult;
            decimal.TryParse(dr["OverCharge"].ToString(), out decimalResult);
            payableEntity.OverCharge = decimalResult;
            decimal.TryParse(dr["FreightAmount"].ToString(), out decimalResult);
            payableEntity.FreightAmount = decimalResult;
            decimal.TryParse(dr["TaxAmount"].ToString(), out decimalResult);
            payableEntity.TaxAmount = decimalResult;
            decimal.TryParse(dr["MiscellaneousAmount"].ToString(), out decimalResult);
            payableEntity.MiscellaneousAmount = decimalResult;
            decimal.TryParse(dr["TradeDiscounts"].ToString(), out decimalResult);
            payableEntity.TradeDiscounts = decimalResult;
            decimal.TryParse(dr["PurchaseAmount"].ToString(), out decimalResult);
            payableEntity.PurchaseAmount = decimalResult;
            decimal.TryParse(dr["BaseLocalCharge"].ToString(), out decimalResult);
            payableEntity.BaseLocalCharge = decimalResult;
            decimal.TryParse(dr["BaseZeroRatedCharge"].ToString(), out decimalResult);
            payableEntity.BaseZeroRatedCharge = decimalResult;
            decimal.TryParse(dr["BaseReverseCharge"].ToString(), out decimalResult);
            payableEntity.BaseReverseCharge = decimalResult;
            payableEntity.BaseChargeType = dr["BaseChargeType"].ToString();
            payableEntity.TaxScheduleId = dr["TaxScheduleId"].ToString();
            payableEntity.CurrencyCode = dr["CurrencyCode"].ToString();
            payableEntity.GLAccount = dr["GlAccount"].ToString();
            payableEntity.GLAccountDescription = dr["GLAccountDescription"].ToString();
            payableEntity.CPTReference = dr["CptReference"].ToString();
            payableEntity.AirwayInvoiceNumber = dr["AirwayInvoiceNumber"].ToString();
            payableEntity.Notes = dr["Notes"].ToString();
            payableEntity.OtherDuplicates = dr["OtherDuplicates"].ToString();
            int.TryParse(dr["CurrencyDecimalPlaces"].ToString(), out result);
            payableEntity.CurrencyDecimal = result;
            int.TryParse(dr["DebitDistributionType"].ToString(), out result);
            payableEntity.DebitDistributionType = result;
            decimal.TryParse(dr["CreditAmount"].ToString(), out decimalResult);
            payableEntity.CreditAmount = decimalResult;
            payableEntity.CreditAccountNumber = dr["CreditAccountNumber"].ToString();
            int.TryParse(dr["StatusId"].ToString(), out result);
            payableEntity.StatusId = result;
            payableEntity.ValidDocumentNumber = dr["ValidDocumentNumber"].ToString();
            payableEntity.CtsiStatus = dr["CtsiStatus"].ToString();
            payableEntity.ErrorDescription = dr["ErrorDescription"].ToString();

            return payableEntity;
        }
    }
}
