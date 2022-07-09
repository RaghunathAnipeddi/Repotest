using Chempoint.GP.Infrastructure.Maps.Base;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class ReceivablesItemDetailMap : BaseDataTableMap<ReceivableDetails>
    {
        public override ReceivableDetails Map(DataRow dr)
        {
            ReceivableDetails receivableSKU = new ReceivableDetails();

            decimal decimalValue;
            receivableSKU.ApplyToDocumentNumber = dr["InvoiceNumber"].ToString();
            receivableSKU.SupportId = dr["SupportId"].ToString();
            receivableSKU.IncidentId = dr["IncidentId"].ToString();
            receivableSKU.OrderLineId = Convert.ToInt32(dr["LineItemSequence"]);
            receivableSKU.IsSelected = Convert.ToBoolean(dr["IsSelected"]);
            receivableSKU.ItemNumber = dr["ItemNumber"].ToString().Trim();
            receivableSKU.ItemDescription = dr["ItemDescription"].ToString().Trim();
            decimal.TryParse(dr["ExtendedPrice"].ToString(), out decimalValue);
            receivableSKU.LineTotalAmount = decimalValue;
            decimal.TryParse(dr["AppliedAmount"].ToString(), out decimalValue);
            receivableSKU.UnitPriceAmount = decimalValue;
            receivableSKU.NetWeight = Convert.ToDecimal(dr["NetWeight"]);
            receivableSKU.ShipWeight = Convert.ToDecimal(dr["Volume"]);
            receivableSKU.OrderedQuantity = Convert.ToDecimal(dr["Quantity"]);
            receivableSKU.TypeId = Convert.ToInt16(dr["TypeId"]);
            receivableSKU.CommentText = dr["Details"].ToString().Trim();

            return receivableSKU;
        }
    }
}
