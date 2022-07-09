using Chempoint.GP.Infrastructure.Config;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Model.Interactions.Account;
using ChemPoint.GP.AccountDL.Util;
using ChemPoint.GP.DataContracts.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace ChemPoint.GP.Account.AccountDL
{
    public class AccountDL : RepositoryBase, IAccountRepository
    {
        public AccountDL(string connectionString)
            : base(new GpAddInDbContext(connectionString))
        {
        }

        public AccountDL(SqlConnection connectionString)
            : base(new GpAddInDbContext(connectionString))
        {
        }

        public object FetchAccountDetails(string customerId, int companyId)
        {
            string spName = string.Empty;

            if (companyId == 1)
                spName = Configuration.SpnaFetchAccountDetails;
            else
                spName = Configuration.SpeuFetchAccountDetails;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPFetchAccountDetailsParam1, SqlDbType.VarChar, customerId, 15);

            return base.GetDataSet(cmd);
        }

        public string GetCustomerOpenTransactionStatus(string customerId, int companyId)
        {
            string spName = string.Empty;
            string hasOpenTrans = string.Empty;
            if (companyId == 1)
                spName = Configuration.SpnaGetAccountHasOpenTransDetails;
            else
                spName = Configuration.SpeuGetAccountHasOpenTransDetails;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPGetAccountHasOpenTransDetailsParam1, SqlDbType.VarChar, customerId, 15);
            cmd.Parameters.AddOutputParams(Configuration.SPGetAccountHasOpenTransDetailsParam2, SqlDbType.VarChar, 255);

            base.GetDataSet(cmd);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            hasOpenTrans = commandResult.Parameters["@" + Configuration.SPGetAccountHasOpenTransDetailsParam2].Value == DBNull.Value ? "" : Convert.ToString(commandResult.Parameters["@" + Configuration.SPGetAccountHasOpenTransDetailsParam2].Value);

            return hasOpenTrans;
        }

        public int GetWarehouseDeactivationStatus(AccountRequest aRequest)
        {
            int warehouseStatus;

            var cmd = CreateStoredProcCommand(Configuration.SPGetWarehouseDeactivationDetails);
            cmd.Parameters.AddInputParams(Configuration.SPGetWarehouseDeactivationDetailsParam1, SqlDbType.VarChar, aRequest.GPWarehouseInformation.WarehouseId, 15);
            cmd.Parameters.AddInputParams(Configuration.SPGetWarehouseDeactivationDetailsParam2, SqlDbType.VarChar, aRequest.CurrencyId, 15);
            cmd.Parameters.AddOutputParams(Configuration.SPGetWarehouseDeactivationDetailsParam3, SqlDbType.SmallInt);

            base.GetDataSet(cmd);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            warehouseStatus = commandResult.Parameters["@" + Configuration.SPGetWarehouseDeactivationDetailsParam3].Value == DBNull.Value ? 5 : Convert.ToUInt16(commandResult.Parameters["@" + Configuration.SPGetWarehouseDeactivationDetailsParam3].Value);

            return warehouseStatus;
        }

        public void DeactivateCustomerinGP(string customerId, int companyId)
        {
            string spName = string.Empty;

            if (companyId == 1)
                spName = Configuration.SPNaDeActivateCustomerInGP;
            else
                spName = Configuration.SPEuDeActivateCustomerInGP;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPDeActivateCustomerInGPParam1, SqlDbType.VarChar, customerId, 15);

            base.GetDataSet(cmd);
        }

        public bool IsCustomerExistsInGP(string customerId, int companyId)
        {
            string spName = string.Empty;
            bool isCustomerExists = false;

            if (companyId == 1)
                spName = Configuration.SPNaIsCustomerExistsInGP;
            else
                spName = Configuration.SPEuIsCustomerExistsInGP;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPIsCustomerExistsInGPParam1, SqlDbType.VarChar, customerId, 15);
            cmd.Parameters.AddOutputParams(Configuration.SPIsCustomerExistsInGPParam2, SqlDbType.Bit);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            isCustomerExists = Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPIsCustomerExistsInGPParam2].Value);
            return isCustomerExists;
        }

        public object FetchAvalaraRequestDetails(string customerId, int companyId)
        {
            string spName = string.Empty;

            if (companyId == 1)
                spName = Configuration.SPNaGetAvalaraRequestDetails;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPNaGetAvalaraRequestDetailsParam1, SqlDbType.VarChar, customerId, 15);

            return base.GetDataSet(cmd);
        }

        public object FetchCustomerDetailsToPushToAvalara(string source, string customerId, int companyId)
        {
            string spName = string.Empty;

            if (companyId == 1)
                spName = Configuration.SPNaGetCustomerDetailsToPushToAvalara;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPNaGetCustomerDetailsToPushToAvalaraParam1, SqlDbType.VarChar, source, 21);
            cmd.Parameters.AddInputParams(Configuration.SPNaGetCustomerDetailsToPushToAvalaraParam2, SqlDbType.VarChar, customerId, 15);

            return base.GetDataSet(cmd);
        }

        public void DeactivateQuoteinGP(string quoteId, string statusId, string statusReason, int companyId)
        {
            string spName = string.Empty;

            if (companyId == 1)
                spName = Configuration.SPNaDeActivateQuoteInGP;
            else
                spName = Configuration.SPEuDeActivateQuoteInGP;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPDeActivateQuoteInGPParam1, SqlDbType.VarChar, quoteId, 50);
            cmd.Parameters.AddInputParams(Configuration.SPDeActivateQuoteInGPParam2, SqlDbType.Int, statusId);
            cmd.Parameters.AddInputParams(Configuration.SPDeActivateQuoteInGPParam3, SqlDbType.VarChar, statusReason, 20);

            base.Update(cmd);
        }

        /// <summary>
        /// IsService SKU Eligible for Customer
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public bool GetCustomerIsServiceSKUEligible(Chempoint.GP.Model.Interactions.Sales.SalesOrderRequest aRequest)
        {
            string spName = string.Empty;

            if (aRequest.CompanyID == 1)
                spName = Configuration.SPNAIsServiceSKUEligible;
            else
                spName = Configuration.SPEUIsServiceSKUEligible;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPIsServiceSKUEligibleParam1, SqlDbType.VarChar, aRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId, 50);
            cmd.Parameters.AddOutputParams(Configuration.SPIsServiceSKUEligibleParam2, SqlDbType.Bit);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPIsServiceSKUEligibleParam2].Value);
        }

    }
}

