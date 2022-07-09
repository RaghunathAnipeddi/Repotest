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
    public class EFTEmailReferenceLookupMap : BaseDataTableMap<EFTPayment>
    {
        public override EFTPayment Map(DataRow dr)
        {
            EFTPayment eftPayment = new EFTPayment();
            DateTime dateValue;
            Decimal decimalValue; int value;
            int.TryParse(dr["EftRowId"].ToString(), out value);
            eftPayment.EFTRowId = value;
            eftPayment.ReferenceNumber = dr["EmailReferenceNumber"].ToString().Trim();
            DateTime.TryParse(dr["DateReceived"].ToString(), out dateValue);
            eftPayment.DateReceived = dateValue;
            Decimal.TryParse(dr["PaymentAmount"].ToString().Trim(), out decimalValue);
            eftPayment.PaymentAmount = decimalValue;
            eftPayment.CustomerID = dr["Customer"].ToString().Trim();
            eftPayment.CurrencyID = dr["Currency"].ToString().Trim();
            return eftPayment;
        }
    }
}
