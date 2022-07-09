using Chempoint.GP.Infrastructure.Maps.Base;
using Chempoint.GP.Model.Interactions.Account;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class CashApplicationMap : BaseDataTableMap<ReceivableDetails>
    {
        public override ReceivableDetails Map(DataRow dr)
        {
            ReceivableDetails receivableSKU = new ReceivableDetails();
            Amount amountinfo = new Amount();
            int value; decimal amount;
            DateTime date;
            receivableSKU.ApplyFromDocumentNumber = dr["DocumentNumber"].ToString();
            receivableSKU.ApplyToDocumentNumber = dr["ApplyToDocumentNumber"].ToString();
            int.TryParse(dr["DocumentType"].ToString(), out value);
            receivableSKU.ApplyFromDocumentTypeId = value;
            int.TryParse(dr["ApplyToDocumentType"].ToString(), out value);
            receivableSKU.ApplyToDocumentType = value;
            receivableSKU.ApplyToCustomerId = dr["ApplyToCustomerNumber"].ToString();
            decimal.TryParse(dr["DocumentAmount"].ToString(), out amount);
            amountinfo.DocumentAmount = amount;
            decimal.TryParse(dr["ApplyAmount"].ToString(), out amount);
            amountinfo.ApplyAmount = amount;
            decimal.TryParse(dr["AmountRemaining"].ToString(), out amount);
            amountinfo.AmountRemaining = amount;
            amountinfo.Currency = dr["CurrencyID"].ToString();
            decimal.TryParse(dr["DocumentAmountinOrignCurrency"].ToString(), out amount);
            amountinfo.OriginatingCurrencyDocumentAmount = amount;
            decimal.TryParse(dr["ApplyAmountInOrignCurrency"].ToString(), out amount);
            amountinfo.ApplyAmountInOrignCurrency = amount;
            decimal.TryParse(dr["ExchangeRate"].ToString(), out amount);
            amountinfo.ExchangeRate = amount;
            
            receivableSKU.Amount = amountinfo;
           
            DateTime.TryParse(dr["ApplyDate"].ToString(), out date);
            receivableSKU.ApplyDate = date;
            receivableSKU.IsSelected = Convert.ToBoolean(dr["IsSelected"]);
            receivableSKU.StatusId = Convert.ToInt16(dr["StatusId"]);
            receivableSKU.SopTypeDatabase = Convert.ToInt16(dr["SopType"]);
            return receivableSKU;
        }
    }
}


