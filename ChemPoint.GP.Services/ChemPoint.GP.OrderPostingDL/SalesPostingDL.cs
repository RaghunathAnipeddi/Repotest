using Chempoint.GP.Infrastructure.Config;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;
using Chempoint.GP.Infrastructure.Maps.Sales;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.DataContracts.Sales;
using ChemPoint.GP.Entities.Business_Entities.Sales;
using ChemPoint.GP.OrderPostingDL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ChemPoint.GP.OrderPostingDL
{
    public class SalesPostingDL : RepositoryBase, ISalesOrderPostRepository
    {
        public SalesPostingDL(string connectionString)
            : base(new GpAddInDbContext(connectionString))
        {
        }

        /// <summary>
        /// Method to remove the users for the given batch number
        /// </summary>
        /// <param name="reqObj">Request Object</param>
        /// <returns>Status the execution</returns>
        public OrderBatchPostResponse RemoveBatchLockedUsers(OrderBatchPostRequest lockUserRequest, int companyId)
        {
            OrderBatchPostResponse response = new OrderBatchPostResponse();
            DataTable lockedUserDT = DataTableMapper.LockedUserDataTable(lockUserRequest,
                DataColumnMappings.LockedUserColumns);
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPRemoveBatchLockedUsers : Configuration.SPRemoveBatchLockedUsers);
            cmd.Parameters.AddInputParams(Configuration.SPRemoveBatchLockedUsersBatchNumber, SqlDbType.VarChar, lockUserRequest.BatchNumber, 50);
            cmd.Parameters.AddInputParams(Configuration.SPRemoveBatchLockedUsersCompanyId, SqlDbType.Int, lockUserRequest.CompanyID);
            cmd.Parameters.AddInputParams(Configuration.SPRemoveBatchLockedUsersRemoveUsers, SqlDbType.Structured, lockedUserDT);
            cmd.Parameters.AddInputParams(Configuration.SPRemoveBatchLockedUsersUserId, SqlDbType.VarChar, lockUserRequest.UserId, 15);
            cmd.Parameters.AddOutputParams(Configuration.SPRemoveBatchLockedUsersIsBatchLockCleared, SqlDbType.Bit);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            response.IsBatchLockCleared = Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPRemoveBatchLockedUsersIsBatchLockCleared].Value);
            return response;
        }

        public object LockBatchforPosting(OrderBatchPostRequest lockBatchRequest, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPLockBatchForPostingNA : Configuration.SPLockBatchForPostingEU);
            cmd.Parameters.AddInputParams(Configuration.SPLockBatchForPostingBatchNumber, SqlDbType.VarChar, lockBatchRequest.BatchNumber, 50);
            cmd.Parameters.AddInputParams(Configuration.SPLockBatchForPostingUserId, SqlDbType.VarChar, lockBatchRequest.UserId, 15);
            return base.Insert(cmd);
        }

        public object ValidateCadExemptionRules(OrderBatchPostRequest lockBatchRequest, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidateCadExemptionRules : Configuration.SPValidateCadExemptionRules);
            cmd.Parameters.AddInputParams(Configuration.SPValidateCadExemptionRulesBatchNumber, SqlDbType.VarChar, lockBatchRequest.BatchNumber, 50);
            return base.Insert(cmd);
        }

        public object UnLockBatchforPosting(OrderBatchPostRequest lockBatchRequest, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPUnLockBatchforPostingNA : Configuration.SPUnLockBatchforPostingEU);
            cmd.Parameters.AddInputParams(Configuration.SPUnLockBatchforPostingBatchNumber, SqlDbType.VarChar, lockBatchRequest.BatchNumber, 15);
            cmd.Parameters.AddInputParams(Configuration.SPUnLockBatchforPostingUserId, SqlDbType.VarChar, lockBatchRequest.UserId, 15);
            return base.Insert(cmd);
        }

        public OrderBatchPostResponse FetchNumTrxandBatchTotal(OrderBatchPostRequest lockBatchRequest, int companyId)
        {
            OrderBatchPostResponse response = new OrderBatchPostResponse();
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPFetchNumTrxandBatchTotalNA : Configuration.SPFetchNumTrxandBatchTotalEU);
            cmd.Parameters.AddInputParams(Configuration.SPFetchNumTrxandBatchTotalBatchNumber, SqlDbType.VarChar, lockBatchRequest.BatchNumber, 25);
            cmd.Parameters.AddOutputParams(Configuration.SPFetchNumTrxandBatchTotalBatchTotal, SqlDbType.Decimal);
            cmd.Parameters.AddOutputParams(Configuration.SPFetchNumTrxandBatchTotalTransTotal, SqlDbType.Int);
            cmd.Parameters.AddInputParams(Configuration.SPFetchNumTrxandBatchTotalUserId, SqlDbType.VarChar, lockBatchRequest.UserId, 15);
            base.Insert(cmd);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            response.BatchDetails = new ChemPoint.Entities.Business_Entities.Sales.SalesOrderPostEntity();
            response.BatchDetails.TotalBatchAmount = Convert.ToDecimal(commandResult.Parameters["@" + Configuration.SPFetchNumTrxandBatchTotalBatchTotal].Value);
            response.BatchDetails.TotalTransInBatch = Convert.ToInt32(commandResult.Parameters["@" + Configuration.SPFetchNumTrxandBatchTotalTransTotal].Value);
            return response;
        }

        public OrderBatchPostResponse ValidateBatchAndFetchData(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidateBatchAndFetchData : Configuration.SPValidateBatchAndFetchData);
            cmd.Parameters.AddInputParams(Configuration.SPValidateBatchAndFetchDataBatchNumber, SqlDbType.VarChar, batchRequest.BatchNumber, 50);
            cmd.Parameters.AddInputParams(Configuration.SPValidateBatchAndFetchDataCompanyId, SqlDbType.Int, batchRequest.CompanyID);
            cmd.Parameters.AddInputParams(Configuration.SPValidateBatchAndFetchDataUserId, SqlDbType.VarChar, batchRequest.UserId);

            DataSet batchDS = base.GetDataSet(cmd);
            if (batchDS.Tables.Count > 0)
            {
                if (batchDS.Tables[1].Rows.Count > 0 && (Convert.ToInt16(batchDS.Tables[1].Rows[0]["NumOfTrx"]) > 0))
                {
                    if (Convert.ToBoolean(batchDS.Tables[1].Rows[0]["MKDTOPST"]))
                    {
                        responseBatch.Status = ResponseStatus.Error;
                        responseBatch.ErrorMessage = "Batch is set for posting. Please select a valid batch.";
                    }
                    else
                    {
                        if (batchDS.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in batchDS.Tables[0].Rows)
                            {
                                responseBatch.AddUserToHoldingBatch(row["UserId"].ToString(), row["UserName"].ToString(), Convert.ToInt16(row["CompanyId"]));
                            }
                        }
                        responseBatch.BatchDetails = new ChemPoint.Entities.Business_Entities.Sales.SalesOrderPostEntity();
                        responseBatch.BatchDetails.TotalTransInBatch = Convert.ToInt16(batchDS.Tables[1].Rows[0]["NumOfTrx"]);
                        responseBatch.BatchDetails.TotalBatchAmount = Convert.ToDecimal(batchDS.Tables[1].Rows[0]["BchTotal"]);
                        responseBatch.Status = ResponseStatus.Success;
                    }
                }
                else
                {
                    responseBatch.Status = ResponseStatus.Error;
                }
            }
            return responseBatch;
        }

        public OrderBatchPostResponse GetAllCanadianErrorTransactions(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();
            string newBatchNumb = Configuration.CanadianTaxIssueBatch;
            int processedTransacations = 0,
            totalTranscationIdentified = 0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetAllCanadianErrorTransactions : Configuration.SPGetAllCanadianErrorTransactions);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllCanadianErrorTransactionsBatchNumber, SqlDbType.VarChar, batchRequest.BatchNumber, 50);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllCanadianErrorTransactionssUserId, SqlDbType.VarChar, batchRequest.UserId, 15);
            
            DataSet canadianIssueFixDS = base.GetDataSet(cmd);
            if (canadianIssueFixDS.Tables.Count > 0)
            {
                if (canadianIssueFixDS.Tables[0].Rows.Count > 0)
                {
                    totalTranscationIdentified = canadianIssueFixDS.Tables[0].Rows.Count;
                    foreach (DataRow row in canadianIssueFixDS.Tables[0].Rows)
                    {
                        UpdateSalesBatchTotals(batchRequest, row["SopNumbe"].ToString(), newBatchNumb, companyId);
                        processedTransacations += 1;
                    }
                }
            }
            responseBatch = FetchNumTrxandBatchTotal(batchRequest, companyId);
            responseBatch.BatchDetails.NumTranIdentified = totalTranscationIdentified;
            responseBatch.BatchDetails.NumTranProcessed = processedTransacations;
            return responseBatch;
        }

        public void UpdateSalesBatchTotals(OrderBatchPostRequest batchRequest, string orderNumber, string newBatch, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPUpdateSalesBatchTotalsNA : Configuration.SPUpdateSalesBatchTotalsEU);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateSalesBatchTotalsCompanyId, SqlDbType.VarChar, batchRequest.CompanyID, 50);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateSalesBatchTotalsCompanyName, SqlDbType.VarChar, batchRequest.CompanyName, 65);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateSalesBatchTotalsNewBatch, SqlDbType.VarChar, newBatch, 15);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateSalesBatchTotalsOldBatch, SqlDbType.VarChar, batchRequest.BatchNumber, 15);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateSalesBatchTotalsSopNumber, SqlDbType.VarChar, orderNumber, 31);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateSalesBatchTotalsUserId, SqlDbType.VarChar, batchRequest.UserId, 15);
            base.Insert(cmd);
        }

        public OrderBatchPostResponse GetAllDropshipTransactions(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();

            int processedTransacations = 0,
            totalTranscationIdentified = 0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetAllDropshipTransactionsNA : Configuration.SPGetAllDropshipTransactionsEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllDropshipTransactionsBatchNumber, SqlDbType.VarChar, batchRequest.BatchNumber, 50);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllDropshipTransactionsUserId, SqlDbType.VarChar, batchRequest.UserId, 15);

            DataSet correctDsDS = base.GetDataSet(cmd);
            if (correctDsDS.Tables.Count > 0 && correctDsDS.Tables[0].Rows.Count > 0)
            {
                totalTranscationIdentified = correctDsDS.Tables[0].Rows.Count;
                foreach (DataRow row in correctDsDS.Tables[0].Rows)
                {
                    DataView specificOrderDV = new DataView(correctDsDS.Tables[0]);
                    specificOrderDV.RowFilter = "SopNumbe='" + row["SopNumbe"].ToString().Trim() + "'";
                    foreach (DataRow ordersRow in specificOrderDV.ToTable().Rows)
                    {
                        UpdateShipviatoDropshipTrans(batchRequest, ordersRow["SopNumbe"].ToString(), Convert.ToInt32(ordersRow["LnItmSeq"]), companyId);
                    }
                    processedTransacations += 1;
                }
            }

            responseBatch = FetchNumTrxandBatchTotal(batchRequest, companyId);
            responseBatch.BatchDetails.NumTranIdentified = totalTranscationIdentified;
            responseBatch.BatchDetails.NumTranProcessed = processedTransacations;
            return responseBatch;
        }

        public void UpdateShipviatoDropshipTrans(OrderBatchPostRequest batchRequest, string orderNumber, int lineNumber, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPUpdateShipviatoDropshipTransNA : Configuration.SPUpdateShipviatoDropshipTransEU);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateShipviatoDropshipTransLineNumber, SqlDbType.Int, lineNumber);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateShipviatoDropshipTransSopNumber, SqlDbType.VarChar, orderNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateShipviatoDropshipTransUserId, SqlDbType.VarChar, batchRequest.UserId, 15);
            base.Insert(cmd);
        }

        public OrderBatchPostResponse GetAllDropShipErrorTransactions(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();
            string newDropshipBatchNumb = Configuration.DropshipIssueBatch;
            string newDocumentAmountBatchNumb = Configuration.DocumentAmountIssueBatch;

            int processedUnitCostTransactions = 0,
                processedAccountTransactions = 0,
                processedDocumentAmountTransactions = 0,
            totalTranscationIdentified = 0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetAllDropshipErrorTransactionsNA : Configuration.SPGetAllDropshipErrorTransactionsEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllDropshipErrorTransactionsBatchNumber, SqlDbType.VarChar, batchRequest.BatchNumber, 25);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllDropshipErrorTransactionsUserId, SqlDbType.VarChar, batchRequest.UserId, 15);

            DataSet serviceSkuDS = base.GetDataSet(cmd);
            if (serviceSkuDS.Tables.Count > 0)
            {
                if (serviceSkuDS.Tables[0].Rows.Count > 0)
                {
                    totalTranscationIdentified = serviceSkuDS.Tables[0].Rows.Count;
                    foreach (DataRow row in serviceSkuDS.Tables[0].Rows)
                    {
                        switch (row["OperationType"].ToString().ToLower().Trim())
                        {
                            case "unitcostissue":
                                UpdateSalesBatchTotals(batchRequest, row["SopNumbe"].ToString(), newDropshipBatchNumb, companyId);
                                processedUnitCostTransactions += 1;
                                break;
                            case "accountissue":
                                UpdateSalesBatchTotals(batchRequest, row["SopNumbe"].ToString(), newDropshipBatchNumb, companyId);
                                processedAccountTransactions += 1;
                                break;
                            case "docamountissue":
                                UpdateSalesBatchTotals(batchRequest, row["SopNumbe"].ToString(), newDocumentAmountBatchNumb, companyId);
                                processedDocumentAmountTransactions += 1;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            responseBatch = FetchNumTrxandBatchTotal(batchRequest, companyId);
            responseBatch.BatchDetails.NumTranIdentified = totalTranscationIdentified;
            responseBatch.BatchDetails.NumTranProcessed = processedUnitCostTransactions + processedAccountTransactions + processedDocumentAmountTransactions;
            responseBatch.BatchDetails.TotalOrdersWithDropshipIssue = processedUnitCostTransactions;
            responseBatch.BatchDetails.TotalOrdersWithAccountNumberIssue = processedAccountTransactions;
            responseBatch.BatchDetails.TotalOrdersWithDocumentAmountIssue = processedDocumentAmountTransactions;

            return responseBatch;
        }

        public OrderBatchPostResponse GetAllCreditCardTransactions(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();
            bool isUsdccTransactionsAvailable = false;
            string newBatchNumb = Configuration.CreditCardBatch;
            int processedTransacations = 0,
            totalTranscationIdentified = 0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetAllCreditCardTransactions : Configuration.SPGetAllCreditCardTransactions);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllCreditCardTransactionsBathNumber, SqlDbType.VarChar, batchRequest.BatchNumber, 50);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllCreditCardTransactionsUserId, SqlDbType.VarChar, batchRequest.UserId, 15);

            DataSet creditCardsDS = base.GetDataSet(cmd);
            if (creditCardsDS.Tables.Count > 0)
            {
                if (creditCardsDS.Tables[1].Rows.Count > 0)
                {
                    totalTranscationIdentified = creditCardsDS.Tables[1].Rows.Count;
                    foreach (DataRow row in creditCardsDS.Tables[1].Rows)
                    {
                        UpdateSalesBatchTotals(batchRequest, row["SopNumbe"].ToString(), newBatchNumb, companyId);
                        processedTransacations += 1;
                    }
                }
                if (creditCardsDS.Tables[0].Rows.Count > 0)
                    isUsdccTransactionsAvailable = true;
            }
            else
            {
                isUsdccTransactionsAvailable = false;
            }

            responseBatch = FetchNumTrxandBatchTotal(batchRequest, companyId);
            responseBatch.BatchDetails.NumTranIdentified = totalTranscationIdentified;
            responseBatch.BatchDetails.NumTranProcessed = processedTransacations;
            responseBatch.IsUsdccTransactionsAvailable = isUsdccTransactionsAvailable;
            return responseBatch;
        }

        public OrderBatchPostResponse GetAllInvalidExchangeRateTransactions(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();
            string newBatchNumb = Configuration.CurrencyBatch;
            int processedTransacations = 0,
            totalTranscationIdentified = 0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetAllInvalidExchangeRateTransactions : Configuration.SPGetAllInvalidExchangeRateTransactions);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllInvalidExchangeRateTransactionsBathNumber, SqlDbType.VarChar, batchRequest.BatchNumber, 25);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllInvalidExchangeRateTransactionsUserId, SqlDbType.VarChar, batchRequest.UserId, 15);

            DataSet currencyIssueDS = base.GetDataSet(cmd);
            if (currencyIssueDS.Tables.Count > 0)
            {
                if (currencyIssueDS.Tables[0].Rows.Count > 0)
                {
                    totalTranscationIdentified = currencyIssueDS.Tables[0].Rows.Count;
                    foreach (DataRow row in currencyIssueDS.Tables[0].Rows)
                    {
                        UpdateSalesBatchTotals(batchRequest, row["SopNumbe"].ToString(), newBatchNumb, companyId);
                        processedTransacations += 1;
                    }
                }
            }

            responseBatch = FetchNumTrxandBatchTotal(batchRequest, companyId);
            responseBatch.BatchDetails.NumTranIdentified = totalTranscationIdentified;
            responseBatch.BatchDetails.NumTranProcessed = processedTransacations;
            return responseBatch;
        }

        public OrderBatchPostResponse GetAllDistributionErrorTransactions(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();
            string newBatchNumb = Configuration.DistributionIssueBatch;
            int processedTransacations = 0,
            totalTranscationIdentified = 0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetAllDistributionErrorTransactionsNA : Configuration.SPGetAllDistributionErrorTransactionsEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllDistributionErrorTransactionsBathNumber, SqlDbType.VarChar, batchRequest.BatchNumber, 25);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllDistributionErrorTransactionsUserId, SqlDbType.VarChar, batchRequest.UserId, 15);

            DataSet distributionDS = base.GetDataSet(cmd);
            if (distributionDS.Tables.Count > 0)
            {
                if (distributionDS.Tables[0].Rows.Count > 0)
                {
                    totalTranscationIdentified = distributionDS.Tables[0].Rows.Count;
                    foreach (DataRow row in distributionDS.Tables[0].Rows)
                    {
                        UpdateSalesBatchTotals(batchRequest, row["SopNumbe"].ToString(), newBatchNumb, companyId);
                        processedTransacations += 1;
                    }
                }
            }

            responseBatch = FetchNumTrxandBatchTotal(batchRequest, companyId);
            responseBatch.BatchDetails.NumTranIdentified = totalTranscationIdentified;
            responseBatch.BatchDetails.NumTranProcessed = processedTransacations;
            return responseBatch;
        }

        public OrderBatchPostResponse GetAllServiceSkuErrorTransactions(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();
            string newBatchNumb = Configuration.ServiceSkuIssueBatch;
            int processedTransacations = 0,
            totalTranscationIdentified = 0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetAllServiceSkuErrorTransactionsNA : Configuration.SPGetAllServiceSkuErrorTransactionsEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllServiceSkuErrorTransactionsBathNumber, SqlDbType.VarChar, batchRequest.BatchNumber, 25);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllServiceSkuErrorTransactionsUserId, SqlDbType.VarChar, batchRequest.UserId, 15);

            DataSet serviceSkuDS = base.GetDataSet(cmd);
            if (serviceSkuDS.Tables.Count > 0)
            {
                if (serviceSkuDS.Tables[0].Rows.Count > 0)
                {
                    totalTranscationIdentified = serviceSkuDS.Tables[0].Rows.Count;
                    foreach (DataRow row in serviceSkuDS.Tables[0].Rows)
                    {
                        UpdateSalesBatchTotals(batchRequest, row["SopNumbe"].ToString(), newBatchNumb, companyId);
                        processedTransacations += 1;
                    }
                }
            }

            responseBatch = FetchNumTrxandBatchTotal(batchRequest, companyId);
            responseBatch.BatchDetails.NumTranIdentified = totalTranscationIdentified;
            responseBatch.BatchDetails.NumTranProcessed = processedTransacations;
            return responseBatch;
        }

        public OrderBatchPostResponse GetAllInterCompanyTransactions(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();
            string newBatchNumb = Configuration.InterCompanyBatch;
            int processedTransacations = 0,
            totalTranscationIdentified = 0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetAllInterCompanyTransactionsNA : Configuration.SPGetAllInterCompanyTransactionsEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllInterCompanyTransactionsBathNumber, SqlDbType.VarChar, batchRequest.BatchNumber, 25);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllInterCompanyTransactionsUserId, SqlDbType.VarChar, batchRequest.UserId, 15);

            DataSet interCompDS = base.GetDataSet(cmd);
            if (interCompDS.Tables.Count > 0)
            {
                if (interCompDS.Tables[0].Rows.Count > 0)
                {
                    totalTranscationIdentified = interCompDS.Tables[0].Rows.Count;
                    foreach (DataRow row in interCompDS.Tables[0].Rows)
                    {
                        UpdateSalesBatchTotals(batchRequest, row["SopNumbe"].ToString(), newBatchNumb, companyId);
                        processedTransacations += 1;
                    }
                }
            }

            responseBatch = FetchNumTrxandBatchTotal(batchRequest, companyId);
            responseBatch.BatchDetails.NumTranIdentified = totalTranscationIdentified;
            responseBatch.BatchDetails.NumTranProcessed = processedTransacations;
            return responseBatch;
        }

        public OrderBatchPostResponse GetAllMissingShipViaTransactions(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();
            string newBatchNumb = Configuration.MissingShipViaBatch;
            int processedTransacations = 0,
            totalTranscationIdentified = 0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SpnaGetAllMissingShipViaTransactions : Configuration.SpeuGetAllMissingShipViaTransactions);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllMissingShipViaTransactionsBathNumber, SqlDbType.VarChar, batchRequest.BatchNumber, 25);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllMissingShipViaTransactionsUserId, SqlDbType.VarChar, batchRequest.UserId, 15);

            DataSet missingShipViaDS = base.GetDataSet(cmd);
            if (missingShipViaDS.Tables.Count > 0)
            {
                if (missingShipViaDS.Tables[0].Rows.Count > 0)
                {
                    totalTranscationIdentified = missingShipViaDS.Tables[0].Rows.Count;
                    foreach (DataRow row in missingShipViaDS.Tables[0].Rows)
                    {
                        UpdateSalesBatchTotals(batchRequest, row["SopNumbe"].ToString(), newBatchNumb, companyId);
                        processedTransacations += 1;
                    }
                }
            }

            responseBatch = FetchNumTrxandBatchTotal(batchRequest, companyId);
            responseBatch.BatchDetails.NumTranIdentified = totalTranscationIdentified;
            responseBatch.BatchDetails.NumTranProcessed = processedTransacations;
            return responseBatch;
        }

        public OrderBatchPostResponse GetAllIncorrectTaxScheduleIdTranscations(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();
            string newBatchNumb = Configuration.EuvatBatch;
            int processedTransacations = 0,
            totalTranscationIdentified = 0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetAllIncorrectTaxScheduleIdTranscations : Configuration.SPGetAllIncorrectTaxScheduleIdTranscations);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllIncorrectTaxScheduleIdTranscationsBathNumber, SqlDbType.VarChar, batchRequest.BatchNumber, 25);
            cmd.Parameters.AddInputParams(Configuration.SPGetAllIncorrectTaxScheduleIdTranscationsUserId, SqlDbType.VarChar, batchRequest.UserId, 15);

            DataSet vatCompDS = base.GetDataSet(cmd);
            if (vatCompDS.Tables.Count > 0)
            {
                if (vatCompDS.Tables[0].Rows.Count > 0)
                {
                    totalTranscationIdentified = vatCompDS.Tables[0].Rows.Count;
                    foreach (DataRow row in vatCompDS.Tables[0].Rows)
                    {
                        UpdateSalesBatchTotals(batchRequest, row["SopNumbe"].ToString(), newBatchNumb, companyId);
                        processedTransacations += 1;
                    }
                }
            }

            responseBatch = FetchNumTrxandBatchTotal(batchRequest, companyId);
            responseBatch.BatchDetails.NumTranIdentified = totalTranscationIdentified;
            responseBatch.BatchDetails.NumTranProcessed = processedTransacations;
            return responseBatch;
        }

        public object MovePostedInvoiceTiHistory(int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.NaspMovepostedinvoice : Configuration.EuspMovepostedinvoice);
            return base.Insert(cmd);
        }

        /// <summary>
        /// GetAllLinkedPaymentIssuesTransactions
        /// </summary>
        /// <param name="batchRequest"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public OrderBatchPostResponse GetAllLinkedPaymentIssuesTransactions(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();
            List<InvoiceLinkedPaymentEntity> InvoiceLinkedPaymentList = new List<InvoiceLinkedPaymentEntity>();
            string newBatchNumb = Configuration.LinkedPaymentBatch;
            int processedTransacations = 0,
            totalTranscationIdentified = 0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetNALinkedPaymentIssuesTransactions : Configuration.SPGetEULinkedPaymentIssuesTransactions);
            cmd.Parameters.AddInputParams(Configuration.GetLinkedPaymentIssuesTransactionsBatchId, SqlDbType.VarChar, batchRequest.BatchNumber, 25);
            cmd.Parameters.AddInputParams(Configuration.GetLinkedPaymentIssuesTransactionsUserId, SqlDbType.VarChar, batchRequest.UserId, 15);

            var vatCompDS = base.GetDataSet(cmd);
            
            if (vatCompDS.Tables.Count > 0)
            {
                if (vatCompDS.Tables[0].Rows.Count > 0)
                {
                    totalTranscationIdentified = vatCompDS.Tables[0].Rows.Count;
                    foreach (DataRow row in vatCompDS.Tables[0].Rows)
                    {
                        UpdateSalesBatchTotals(batchRequest, row["Invoice Number"].ToString(), newBatchNumb, companyId);
                        processedTransacations += 1;
                    }
                }
            }

            responseBatch = FetchNumTrxandBatchTotal(batchRequest, companyId);
            if (vatCompDS.Tables[0].Rows.Count > 0)
                responseBatch.EmailContent = ConvertDataTableToString(vatCompDS.Tables[0]);
            else
                responseBatch.EmailContent = string.Empty;
            responseBatch.BatchDetails.NumTranIdentified = totalTranscationIdentified;
            responseBatch.BatchDetails.NumTranProcessed = processedTransacations;
            return responseBatch;
        }



        /// <summary>
        /// GetAllFailedPrePaymentTransactions
        /// </summary>
        /// <param name="batchRequest"></param>
        /// <param name="companyId"></param>
        /// <returns>Failed Transactions</returns>
        public OrderBatchPostResponse GetAllFailedPrePaymentTransactions(OrderBatchPostRequest batchRequest, int companyId)
        {
            OrderBatchPostResponse responseBatch = new OrderBatchPostResponse();
            string newBatchNumb = Configuration.FailedPrePaymentBatch;
            int processedTransacations = 0,
            totalTranscationIdentified = 0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetNAFailedPrePaymentTransactions : Configuration.SPGetEUFailedPrePaymentTransactions);
            cmd.Parameters.AddInputParams(Configuration.GetFailedPrePaymentBatchId, SqlDbType.VarChar, batchRequest.BatchNumber, 25);
            cmd.Parameters.AddInputParams(Configuration.GetFailedPrePaymentUserId, SqlDbType.VarChar, batchRequest.UserId, 15);
            var vatCompDS = base.GetDataSet(cmd);
            responseBatch.EmailContent = string.Empty;

            if (vatCompDS.Tables.Count > 0)
            {
                if (vatCompDS.Tables[0].Rows.Count > 0)
                {
                    totalTranscationIdentified = vatCompDS.Tables[0].Rows.Count;
                    responseBatch.EmailContent = ConvertDataTableToString(vatCompDS.Tables[0]);
                    foreach (DataRow row in vatCompDS.Tables[0].Rows)
                    {
                        UpdateSalesBatchTotals(batchRequest, row["Invoice Number"].ToString(), newBatchNumb, companyId);
                        processedTransacations += 1;
                    }
                }
            }

            responseBatch = FetchNumTrxandBatchTotal(batchRequest, companyId);
            if (vatCompDS.Tables[0].Rows.Count > 0)
                responseBatch.EmailContent = ConvertDataTableToString(vatCompDS.Tables[0]);
            else
                responseBatch.EmailContent = string.Empty;
            responseBatch.BatchDetails.NumTranIdentified = totalTranscationIdentified;
            responseBatch.BatchDetails.NumTranProcessed = processedTransacations;
            return responseBatch;
        }


        /// <summary>
        /// Convert DataTable to String...
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static string ConvertDataTableToString(DataTable dataTable)
        {
            string data = string.Empty;
            int rowsCount = dataTable.Rows.Count;
            for (int rowValue = 0; rowValue < rowsCount; rowValue++)
            {
                DataRow row = dataTable.Rows[rowValue];
                int columnsCount = dataTable.Columns.Count;
                for (int colValue = 0; colValue < columnsCount; colValue++)
                {
                    data += dataTable.Columns[colValue].ColumnName + "~" + row[colValue];
                    if (colValue == columnsCount - 1)
                    {
                        if (rowValue != (rowsCount - 1))
                            data += "#";
                    }
                    else
                        data += "|";
                }
            }
            return data;
        }
    }
}
