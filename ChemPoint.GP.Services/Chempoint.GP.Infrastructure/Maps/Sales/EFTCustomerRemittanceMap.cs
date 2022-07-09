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
    public class EFTCustomerRemittanceMap : BaseDataTableMap<EFTPayment>
    {
        public override EFTPayment Map(DataRow dr)
        {
            EFTPayment eftPayment = new EFTPayment();
            DateTime dateValue;
            Decimal decimalValue; int value;
            bool boolValue;
            int.TryParse(dr["EftId"].ToString(), out value);
            eftPayment.EftId = value;
            eftPayment.PaymentNumber = dr["PaymentNumber"].ToString().Trim();
            eftPayment.ReferenceNumber = dr["ReferenceNumber"].ToString().Trim();
            DateTime.TryParse(dr["DateReceived"].ToString(), out dateValue);
            eftPayment.DateReceived = dateValue;
            Decimal.TryParse(dr["PaymentAmount"].ToString().Trim(), out decimalValue);
            eftPayment.PaymentAmount = decimalValue;
            eftPayment.CustomerID = dr["CustomerId"].ToString().Trim();
            bool.TryParse(dr["IsFullyApplied"].ToString(), out boolValue);
            if (boolValue == true)
                eftPayment.IsFullyApplied = 1;
            else
                eftPayment.IsFullyApplied = 0;
            eftPayment.Source = dr["Source"].ToString().Trim();
            eftPayment.BankOriginatingID = dr["BankOriginating"].ToString().Trim();
            eftPayment.Notes = dr["Notes"].ToString().Trim();
            eftPayment.ItemReference = dr["ItemReferenceNumber"].ToString().Trim();
            Decimal.TryParse(dr["ItemAmount"].ToString().Trim(), out decimalValue);
            eftPayment.ItemAmount = decimalValue;
            int.TryParse(dr["EftAppId"].ToString(), out value);
            eftPayment.EftAppId = value;
            eftPayment.CurrencyID = dr["CurrencyID"].ToString().Trim();
            int.TryParse(dr["EftStatusId"].ToString(), out value);
            eftPayment.EftStatusId = value;
            //eftPayment.ReceivedOnCTS = dr["ReceivedOnCTX"].ToString().Trim();
            //eftPayment.Source = dr["Source"].ToString().Trim();
            //eftPayment.BankOriginatingID = dr["BankOriginating"].ToString().Trim();
            Decimal.TryParse(dr["TotalPymtAmount"].ToString().Trim(), out decimalValue);
            eftPayment.TotalPaymentAmount = decimalValue;
            eftPayment.Description = dr["EftStatusDescription"].ToString().Trim();
            int.TryParse(dr["FileId"].ToString().Trim(), out value);
            eftPayment.EftFileId = value;
            eftPayment.AccountName = dr["AccountName"].ToString().Trim();
            return eftPayment;
        }
    }
}
