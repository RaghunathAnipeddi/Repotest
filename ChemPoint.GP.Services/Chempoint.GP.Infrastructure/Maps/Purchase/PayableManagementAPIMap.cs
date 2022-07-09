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
    /// <PayableManagement>
    /// Project Name        :   payable mangement migration
    /// Affected Module     :   Purchase
    /// Affected Windows    :  Mapping API Validation SP details to Object.
    /// Developed on        :   April2017
    /// Developed by        :   Muthu
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **Apr2017       Mthangaraj      Initial Creation.
    /// </PayableManagement>
    public class PayableManagementAPIMap : BaseDataTableMap<PayableLineEntity>
    {
        public override PayableLineEntity Map(DataRow dr)
        {
            PayableLineEntity payableEntity = new PayableLineEntity();
            int result;
            DateTime dateResult;
            decimal decimalResult;
            int.TryParse(dr["APIInvoiceId"].ToString(), out result);
            payableEntity.APIInvoiceId = result;
            int.TryParse(dr["OriginalApiInvoiceId"].ToString(), out result);
            payableEntity.OriginalApiInvoiceId = result;
            payableEntity.DocumentTypeName = dr["DocumentType"].ToString();
            payableEntity.DocumentRowId = dr["DocumentRowID"].ToString();
            payableEntity.DocumentNumber = dr["DocumentNumber"].ToString();
            DateTime.TryParse(dr["DocumentDate"].ToString(), out dateResult);
            payableEntity.DocumentDate = dateResult;
            payableEntity.VendorName = dr["VendorNumber"].ToString();
            decimal.TryParse(dr["DocumentAmount"].ToString(), out decimalResult);
            payableEntity.DocumentAmount = decimalResult;
            decimal.TryParse(dr["ApprovedAmount"].ToString(), out decimalResult);
            payableEntity.ApprovedAmount = decimalResult;
            decimal.TryParse(dr["FreightAmount"].ToString(), out decimalResult);
            payableEntity.FreightAmount = decimalResult;
            decimal.TryParse(dr["SalesTaxAmount"].ToString(), out decimalResult);
            payableEntity.TaxAmount = decimalResult;
            decimal.TryParse(dr["TradeDiscountAmount"].ToString(), out decimalResult);
            payableEntity.TradeDiscounts = decimalResult;
            decimal.TryParse(dr["MiscellaneousAmount"].ToString(), out decimalResult);
            payableEntity.MiscellaneousAmount = decimalResult;
            decimal.TryParse(dr["PurchasingAmount"].ToString(), out decimalResult);
            payableEntity.PurchaseAmount = decimalResult;
            decimal.TryParse(dr["POAmount"].ToString(), out decimalResult);
            payableEntity.POAmount = decimalResult;
            payableEntity.PurchaseOrderNumber = dr["PONumber"].ToString();
            DateTime.TryParse(dr["DocumentDate"].ToString(), out dateResult);
            payableEntity.PODateOpened = dateResult;
            int.TryParse(dr["POLineNumber"].ToString(), out result);
            payableEntity.POLineNumber = result;
            payableEntity.ReceiptNumber = dr["ReceiptNumber"].ToString();
            int.TryParse(dr["ReceiptLineNumber"].ToString(), out result);
            payableEntity.ReceiptLineNumber = result;
            payableEntity.ItemNumber = dr["ItemNumber"].ToString();
            decimal.TryParse(dr["ItemUnitQty"].ToString(), out decimalResult);
            payableEntity.ItemUnitQty = decimalResult;
            decimal.TryParse(dr["ItemUnitPrice"].ToString(), out decimalResult);
            payableEntity.ItemUnitPrice = decimalResult; ;
            decimal.TryParse(dr["ItemExtendedAmount"].ToString(), out decimalResult);
            payableEntity.ExtendedCost = decimalResult;
            decimal.TryParse(dr["AdjustedItemUnitQty"].ToString(), out decimalResult);
            payableEntity.AdjustedItemUnitQuantity = decimalResult;
            decimal.TryParse(dr["AdjustedItemUnitPrice"].ToString(), out decimalResult);
            payableEntity.AdjustedItemUnitPrice = decimalResult;
            payableEntity.ShipToState = dr["ShipToState"].ToString();
            DateTime.TryParse(dr["ShippedDate"].ToString(), out dateResult);
            payableEntity.ShippedDate = dateResult;
            payableEntity.LocationName = dr["LocationKey"].ToString();
            int.TryParse(dr["GLIndex"].ToString(), out result);
            payableEntity.GLIndex = result;
            payableEntity.TaxScheduleId = dr["TaxScheduleId"].ToString();
            int.TryParse(dr["FormTypeCode"].ToString(), out result);
            payableEntity.FormTypeCode = result;
            int.TryParse(dr["FormTypeCode"].ToString(), out result);
            payableEntity.IsDuplicated = result;
            int.TryParse(dr["RequiredDistribution"].ToString(), out result);
            payableEntity.RequiredDistribution = result;
            int.TryParse(dr["StatusId"].ToString(), out result);
            payableEntity.StatusId = result;
            payableEntity.ErrorDescription = dr["ErrorDescription"].ToString();
            payableEntity.CurrencyId = dr["CurrencyId"].ToString();
            payableEntity.UserId = dr["UserId"].ToString();
            payableEntity.Notes = dr["Notes"].ToString();

            return payableEntity;
        }
    }
}

