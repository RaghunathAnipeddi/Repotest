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
    public class EFTCustomerSummaryRemittanceMap : BaseDataTableMap<EFTCustomerPayment>
    {
        public override EFTCustomerPayment Map(DataRow dr)
        {
            EFTCustomerPayment eftPayment = new EFTCustomerPayment();
            DateTime dateValue;
            Decimal decimalValue; int value;
            int.TryParse(dr["EftId"].ToString().Trim(), out value);
            eftPayment.EftId = value;
            int.TryParse(dr["EftAppId"].ToString().Trim(), out value);
            eftPayment.EftAppId = value;
            eftPayment.PaymentNumber = dr["PaymentNumber"].ToString().Trim();
            eftPayment.ReferenceNumber = dr["ReferenceNumber"].ToString().Trim();
            DateTime.TryParse(dr["DateReceived"].ToString(), out dateValue);
            eftPayment.DateReceived = dateValue;
            Decimal.TryParse(dr["PaymentAmount"].ToString().Trim(), out decimalValue);
            eftPayment.PaymentAmount = decimalValue;
            eftPayment.CustomerID = dr["CustomerId"].ToString().Trim();
            eftPayment.CurrencyId = dr["CurrencyId"].ToString().Trim();
            int.TryParse(dr["IsFullyApplied"].ToString(), out value);
            eftPayment.IsFullyApplied = Convert.ToBoolean(dr["IsFullyApplied"].ToString().Trim());
            eftPayment.Source = dr["Source"].ToString().Trim();
            eftPayment.BankOriginating = dr["BankOriginating"].ToString().Trim();
            eftPayment.ItemReferenceNumber = dr["ItemReferenceNumber"].ToString().Trim();
            Decimal.TryParse(dr["ItemAmount"].ToString().Trim(), out decimalValue);
            eftPayment.ItemAmount = decimalValue;
            int.TryParse(dr["IsUpdated"].ToString().Trim(), out value);
            eftPayment.IsUpdated = value;
            int.TryParse(dr["Status"].ToString(), out value);
            eftPayment.Status = value;
            eftPayment.StatusReason = dr["StatusReason"].ToString().Trim();
            eftPayment.AccountName = dr["AccountName"].ToString().Trim();
            return eftPayment;
        }
    }
}
