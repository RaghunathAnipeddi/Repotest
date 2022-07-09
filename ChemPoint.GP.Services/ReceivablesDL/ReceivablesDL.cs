using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ChemPoint.GP.DataContracts.Sales;
using ChemPoint.GP.Entities.Business_Entities;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;
using Chempoint.GP.Infrastructure.Config;
using System.Data.SqlClient;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.Entities.Business_Entities.Sales;
using ReceivablesDL.Util;
using System.IO;
using ChemPoint.GP.Entities.BaseEntities;
using Chempoint.GP.Infrastructure.Maps.Sales;
namespace ChemPoint.GP.ReceivablesDL
{
    public class ReceivablesDL : RepositoryBase, IReceivablesRepository
    {

        public ReceivablesDL(string connectionString)
            : base(new GpAddInDbContext(connectionString))
        {

        }

        #region Bank_summary_and_CTX_import

        /// <summary>
        /// Saves the bank summary details to DB.
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <returns></returns>
        public object SaveImportedEFTBankSummaryDetail(ReceivablesRequest receivablesRequest)
        {
            string result = string.Empty;

            DataTable dtOriginalEFT = DataTableMapper.ImportOriginalEFTSummaryReport(receivablesRequest,
               DataColumnMapping.ImportOriginalEFTSummary);

            DataTable dtFilteredEFT = DataTableMapper.ImportFilteredEFTSummaryReport(receivablesRequest,
               DataColumnMapping.ImportFilteredEFT);

            var cmd = CreateStoredProcCommand(receivablesRequest.CompanyId == 1 ? Configuration.SPSaveimportedeftdetailsNA : Configuration.SPSaveimportedeftdetailsEU);

            cmd.Parameters.AddInputParams(Configuration.SaveimportedeftdetailsUserNameParam, SqlDbType.VarChar, receivablesRequest.UserName);
            cmd.Parameters.AddInputParams(Configuration.SaveimportedeftdetailsFileParam, SqlDbType.VarChar, receivablesRequest.EFTPaymentFileName);
            cmd.Parameters.AddInputParams(Configuration.SPSaveimportedeftdetailsOriginal, SqlDbType.Structured, dtOriginalEFT);
            cmd.Parameters.AddInputParams(Configuration.SPSaveimportedeftdetailsActual, SqlDbType.Structured, dtFilteredEFT);
            cmd.Parameters.AddInputParams("BatchNumber", SqlDbType.VarChar, receivablesRequest.BatchId);


            DataSet ds = base.GetDataSet(cmd);

            if (ds != null)
            {
                DataTable dt = ds.Tables[0];

                foreach (DataRow row in dt.Rows)
                {
                    result = row[0].ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Saves the CTX remittance details to DB.
        /// </summary>
        /// <param name="dtBankRemittanceOriginalEFTTransaction"></param>
        /// <param name="dtBankRemittanceOriginalEFTTransactionMap"></param>
        /// <param name="strUserName"></param>
        /// <param name="strFileName"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public object SaveRemittanceFileDetails(DataTable dtBankRemittanceOriginalEFTTransaction, DataTable dtBankRemittanceOriginalEFTTransactionMap,
                                                string strUserName, string strFileName, int companyId)
        {
            string result = string.Empty;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPSaveOriginalRemittanceFileDetailsNA : Configuration.SPSaveOriginalRemittanceFileDetailsEU);

            cmd.Parameters.AddInputParams(Configuration.UserNameParam, SqlDbType.VarChar, strUserName);
            cmd.Parameters.AddInputParams(Configuration.SaveimportedeftdetailsFileParam, SqlDbType.VarChar, strFileName);
            cmd.Parameters.AddInputParams(Configuration.BankRemittanceOrigType, SqlDbType.Structured, dtBankRemittanceOriginalEFTTransaction);
            cmd.Parameters.AddInputParams(Configuration.BankRemittanceOrigMapType, SqlDbType.Structured, dtBankRemittanceOriginalEFTTransactionMap);
            return base.Insert(cmd);
        }

        public string IsEftFileAlreadyProcessed(string strFileName, string BatchNumber, int companyId)
        {
            string status = string.Empty;
            ReceivablesResponse objReceivablesResponse = new ReceivablesResponse();
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPFilevalidations : null);
            cmd.Parameters.AddInputParams("FileName", SqlDbType.VarChar, strFileName);
            cmd.Parameters.AddInputParams("BatchNumber", SqlDbType.VarChar, BatchNumber);
            cmd.Parameters.AddOutputParams("Status", SqlDbType.VarChar, 1);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            status = Convert.ToString(commandResult.Parameters["@Status"].Value);
            return status;
        }

        /// <summary>
        /// To Update EFT open Eft transactions
        /// </summary>
        /// <param name="dtUpdateEFTTransactions"></param>
        /// <param name="dtInsertEFTTransactions"></param>
        /// <param name="dtBankRemittanceTrans"></param>
        /// <param name="dtBankRemittanceTransMap"></param>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public object UpdateEFTTransactionsApplyDetails(DataTable dtUpdateEFTTransactions, DataTable dtInsertEFTTransactions, DataTable dtBankRemittanceTrans, DataTable dtBankRemittanceTransMap, int companyId, string userId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPInsertEFTTransactionDetailsNA : null);
            cmd.Parameters.AddInputParams(Configuration.UpdateEFTTransactionDetailsType, SqlDbType.Structured, dtUpdateEFTTransactions);
            cmd.Parameters.AddInputParams(Configuration.InsertEFTTransactionDetailsType, SqlDbType.Structured, dtInsertEFTTransactions);
            cmd.Parameters.AddInputParams("BankRemittanceOriginalEFTTransactionType", SqlDbType.Structured, dtBankRemittanceTrans);
            cmd.Parameters.AddInputParams("BankRemittanceOriginalEFTTransactionMapType", SqlDbType.Structured, dtBankRemittanceTransMap);
            cmd.Parameters.AddInputParams(Configuration.UserNameParam, SqlDbType.VarChar, userId);
            return base.Insert(cmd);
        }



        /// <summary>           
        /// To get open EFT transactions
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public object GetEftOpenTransactions(int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetEftOpenTransactionDetailNA : Configuration.SPGetEftOpenTransactionDetailEU);
            var ds = base.GetDataSet(cmd);
            return ds;
        }

        #endregion Bank_summary_and_CTX_import

        /// <summary>
        /// Validate EFT payment Remittance window Details.
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <returns></returns>
        public DataSet ValidatePaymentRemittance(ReceivablesRequest receivablesRequest)
        {
            DataTable paymentRemitDT = DataTableMapper.PushToGPPayments(receivablesRequest,
               DataColumnMapping.PushToGPTable);

            var cmd = CreateStoredProcCommand(receivablesRequest.AuditInformation.CompanyId == 1 ? Configuration.SPValidatePaymentRemittanceNA : Configuration.SPValidatePaymentRemittanceEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidatePaymentRemittanceParam1, SqlDbType.Structured, paymentRemitDT);
            cmd.Parameters.AddInputParams(Configuration.SPValidatePaymentRemittanceParam2, SqlDbType.Int, receivablesRequest.Actiontype);

            return base.GetDataSet(cmd);
        }

        /// <summary>
        /// Get Push To GP details...
        /// </summary>
        /// <param name="salesRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetPushToGP(ReceivablesRequest eftRequest, int actionType)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            DataTable dtXmlDetails = DataTableMapper.PushToGPPayments(eftRequest,
              DataColumnMapping.PushToGPTable);

            string spName = string.Empty;
            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAEFTPaymentApplyRemittanceDetails;
            else
                spName = Configuration.SPEUEFTPaymentApplyRemittanceDetails;
            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.SPEFTPaymentApplyRemittanceDetailsParam1, SqlDbType.Structured, dtXmlDetails);
            cmd.Parameters.AddInputParams(Configuration.SPEFTPaymentApplyRemittanceDetailsParam2, SqlDbType.Int, actionType);
            eftResponse.ApplyPaymentDetail = base.GetDataSet(cmd);

            return eftResponse;
        }

        /// <summary>
        /// Update Payment status to GP tables.
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse UpdatePaymentStatus(string createdBy, string nextAvailablePaymentNumber, int CompanyId, string referenceNumber)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            string spName = string.Empty;
            if (CompanyId == 1)
                spName = Configuration.SPNAUpdateEFTPaymentTransactionsDetails;
            else
                spName = Configuration.SPEUUpdateEFTPaymentTransactionsDetails;
            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateEFTPaymentTransactionsDetailsParam1, SqlDbType.VarChar, nextAvailablePaymentNumber);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateEFTPaymentTransactionsDetailsParam2, SqlDbType.VarChar, referenceNumber);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateEFTPaymentTransactionsDetailsParam3, SqlDbType.VarChar, createdBy);
            base.GetDataSet(cmd);
            eftResponse.Status = ResponseStatus.Success;

            return eftResponse;
        }

        /// <summary>
        /// Update Apply Status
        /// </summary>
        /// <param name="EftId"></param>
        /// <param name="applyType"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public ReceivablesResponse UpdateApplyStatus(int EftId, DataTable applyType, int companyId)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            string spName = string.Empty;
            if (companyId == 1)
                spName = Configuration.SPNAUpdateEFTApplyTransactionsDetails;
            else
                spName = Configuration.SPNAUpdateEFTApplyTransactionsDetails;
            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateEFTApplyTransactionsDetailsParam1, SqlDbType.Int, EftId);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateEFTApplyTransactionsDetailsParam2, SqlDbType.Structured, applyType);
            base.GetDataSet(cmd);
            eftResponse.Status = ResponseStatus.Success;

            return eftResponse;
        }

        /// <summary>
        /// Get Next Payment Number
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="documentId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public string GetNextPaymentNumber(int orderType, int companyId)
        {
            string spName = string.Empty;
            if (companyId == 1)
                spName = Configuration.SPGetNextAvailablePaymentNumberNA;
            else
                spName = Configuration.SPGetNextAvailablePaymentNumberEU;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPGetNextAvailablePaymentNumberParam1, SqlDbType.TinyInt, orderType);
            cmd.Parameters.AddOutputParams(Configuration.SPGetNextAvailablePaymentNumberParam2, SqlDbType.VarChar, 21);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return commandResult.Parameters["@" + Configuration.SPGetNextAvailablePaymentNumberParam2].Value.ToString().Trim();
        }
        /// <summary>
        /// Get documents details for apply to open orders window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public object SaveDocumentForApplyToOpenOrdersForEngine(ReceivablesResponse cashEntity, int companyId)
        {
            string spName = string.Empty;
            if (companyId == 1)
                spName = Configuration.SPSaveDocumentsForCashEngineNA;
            else
                spName = Configuration.SPSaveDocumentsForCashEngineEU;

            DataTable documentCashDT = cashEntity.ApplyPaymentDetail.Tables[0].DefaultView.ToTable(false, "ApplyOpenOrdersId", "ErrorId");

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPSaveDocumentsForCashEngineParam1, SqlDbType.Structured, documentCashDT);
            return base.Update(cmd);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public object GetEftTransactionDetails(int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetEftTransactionDetailNA : null);
            var ds = base.GetDataSet(cmd);
            return ds;
        }

        public ReceivablesResponse ValidateEFTLine(DataTable dtSummaryLineRef, DataTable dtSummaryLineItemRef, int companyId)
        {
            ReceivablesResponse objReceivablesResponse = new ReceivablesResponse();
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidateEFTLineNA : null);
            cmd.Parameters.AddInputParams(Configuration.EFTSummaryLineReferenceType, SqlDbType.Structured, dtSummaryLineRef);
            cmd.Parameters.AddInputParams(Configuration.EFTSummaryLineItemReferenceType, SqlDbType.Structured, dtSummaryLineItemRef);
            cmd.Parameters.AddOutputParams("IsValid", SqlDbType.Bit);
            cmd.Parameters.AddOutputParams("ErrorMessage", SqlDbType.VarChar);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            objReceivablesResponse.ValidationStatus = Convert.ToBoolean(commandResult.Parameters["IsValid"].Value);
            objReceivablesResponse.ErrorMessage = Convert.ToString(commandResult.Parameters["ErrorMessage"].Value);
            return objReceivablesResponse;
        }



        /// <summary>
        /// Fetch Email Reference lookup Details...
        /// </summary>
        /// <param name="ReceivableRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchEmailReferenceLookup(ReceivablesRequest ReceivableRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();

            List<EFTPayment> eftPaymentList = new List<EFTPayment>();
            string spName = string.Empty;

            if (ReceivableRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAFetchEmailReference;
            else
                spName = Configuration.SPEUFetchEmailReference;

            var cmd = CreateStoredProcCommand(spName);

            var ds = base.GetDataSet(cmd);

            eftPaymentList = base.GetAllEntities<EFTPayment, EFTEmailReferenceLookupMap>(ds.Tables[0]).ToList();  // Customre detail Bill Id
            eftResponse.EFTCustomerRemittancesList = eftPaymentList;

            return eftResponse;
        }


        /// <summary>
        /// Fetch Email Reference Line Details...
        /// </summary>
        /// <param name="ReceivableRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchEmailReferenceScroll(ReceivablesRequest ReceivableRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            List<EFTPayment> eftPaymentList = new List<EFTPayment>();
            EFTPayment eftPayment = new EFTPayment();
            string spName = string.Empty;

            if (ReceivableRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAFetchEmailReferenceScroll;
            else
                spName = Configuration.SPEUFetchEmailReferenceScroll;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.FetchEmailReferenceScrollParam1, SqlDbType.Int, ReceivableRequest.EFTPayment.EFTRowId);

            var ds = base.GetDataSet(cmd);

            eftPayment = base.GetEntity<EFTPayment, EFTEmailReferenceLookupMap>(ds.Tables[0]);  // Customre detail Bill Id

            eftPaymentList = base.GetAllEntities<EFTPayment, EFTEmailReferenceMap>(ds.Tables[1]).ToList();  // Customre detail Bill Id
            eftResponse.EFTCustomerRemittancesList = eftPaymentList;
            eftResponse.EFTPayment = eftPayment;

            return eftResponse;
        }

        /// <summary>
        /// Fetch Email Reference lookup Details...
        /// </summary>
        /// <param name="ReceivableRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse DeleteEFTEmailRemittance(ReceivablesRequest ReceivableRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();

            List<EFTPayment> eftPaymentList = new List<EFTPayment>();
            string spName = string.Empty;

            if (ReceivableRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNADeleteEFTEmailRemittance;
            else
                spName = Configuration.SPEUDeleteEFTEmailRemittance;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.DeleteEFTEmailRemittanceParam1, SqlDbType.Int, ReceivableRequest.EFTPayment.EFTRowId);
            base.Update(cmd);

            eftResponse.Status = ResponseStatus.Success;
            return eftResponse;
        }

        /// <summary>
        /// Fetch Email Reference Line Details...
        /// </summary>
        /// <param name="ReceivableRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse SaveEmailReferences(ReceivablesRequest ReceivableRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            List<EFTPayment> eftPaymentList = new List<EFTPayment>();
            string spName = string.Empty;

            DataTable eftEmailLineDT = DataTableMapper.EFTEmailLineDetails(ReceivableRequest,
              DataColumnMapping.EFTEmailLineDetails);

            if (ReceivableRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNASaveEmailReferences;
            else
                spName = Configuration.SPEUSaveEmailReferences;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SaveEmailReferencesParam1, SqlDbType.Int, ReceivableRequest.EFTPayment.EFTRowId);
            cmd.Parameters.AddInputParams(Configuration.SaveEmailReferencesParam2, SqlDbType.VarChar, ReceivableRequest.EFTPayment.ReferenceNumber);
            cmd.Parameters.AddInputParams(Configuration.SaveEmailReferencesParam3, SqlDbType.Decimal, ReceivableRequest.EFTPayment.PaymentAmount);
            cmd.Parameters.AddInputParams(Configuration.SaveEmailReferencesParam4, SqlDbType.VarChar, ReceivableRequest.EFTPayment.CustomerID);
            cmd.Parameters.AddInputParams(Configuration.SaveEmailReferencesParam5, SqlDbType.VarChar, ReceivableRequest.EFTPayment.CurrencyID);
            cmd.Parameters.AddInputParams(Configuration.SaveEmailReferencesParam6, SqlDbType.DateTime, ReceivableRequest.EFTPayment.DateReceived);
            cmd.Parameters.AddInputParams(Configuration.SaveEmailReferencesParam7, SqlDbType.Structured, eftEmailLineDT);

            var ds = base.GetDataSet(cmd);

            eftResponse.Status = ResponseStatus.Success;

            return eftResponse;
        }
        /// <summary>
        /// Get EFT Customer Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public int DeleteBankEntryEFTTransaction(ReceivablesRequest eftRequest)
        {
            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNADeleteEFTTransactionCustomerRemittance;
            else
                spName = Configuration.SPEUDeleteEFTTransactionCustomerRemittance;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.SPNADeleteEFTTransactionCustomerRemittanceParam1, SqlDbType.Int, eftRequest.EFTPayment.EftId);

            return base.Delete(cmd);

        }

        /// <summary>
        /// Get EFT Customer Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse MapBankEntryToEmailRemittance(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAUpdateEftEmailRemittancesToPayments;
            else
                spName = Configuration.SPEUUpdateEftEmailRemittancesToPayments;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.SPNAUpdateEftEmailRemittancesToPaymentsParam1, SqlDbType.Int, eftRequest.EFTPayment.EftFileId);
            base.Update(cmd);

            eftResponse.Status = ResponseStatus.Success;
            return eftResponse;

        }
    }
}
