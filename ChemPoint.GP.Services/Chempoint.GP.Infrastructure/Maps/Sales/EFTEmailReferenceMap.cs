using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class EFTEmailReferenceMap : BaseDataTableMap<EFTPayment>
    {
        public override EFTPayment Map(DataRow dr)
        {
            EFTPayment eftPayment = new EFTPayment();
            DateTime dateValue;
            Decimal decimalValue;
            int value;
            int.TryParse(dr["EFTRowId"].ToString(), out value);
            eftPayment.EftId = value;
            int.TryParse(dr["EFTApplyId"].ToString(), out value);
            eftPayment.EftAppId = value;
            eftPayment.ReferenceNumber = dr["EFTReference"].ToString().Trim();
            DateTime.TryParse(dr["DateReceived"].ToString(), out dateValue);
            eftPayment.DateReceived = dateValue;
            eftPayment.CustomerID = dr["Customer"].ToString().Trim();
            eftPayment.CurrencyID = dr["Currency"].ToString().Trim();
            Decimal.TryParse(dr["PaymentAmount"].ToString().Trim(), out decimalValue);
            eftPayment.PaymentAmount = decimalValue;
            eftPayment.ItemReference = dr["ItemReferenceNumber"].ToString().Trim();
            Decimal.TryParse(dr["ItemAmount"].ToString().Trim(), out decimalValue);
            eftPayment.ItemAmount = decimalValue;

            return eftPayment;
        }
    }
}
