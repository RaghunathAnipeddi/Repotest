using System;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Model.Interactions.HoldEngine;
using ChemPoint.GP.BusinessContracts.TaskScheduler.HoldEngine;
using ChemPoint.GP.DataContracts.TaskScheduler.HoldEngine;
using System.Data;
using System.Collections.Generic;
using Chempoint.GP.Model.Interactions.Email;
using ChemPoint.GP.Email;
using ChemPoint.GP.Entities.BaseEntities;
using System.Reflection;

namespace ChemPoint.GP.HoldEngineBL
{
    public class HoldEngineBusiness : IHoldEngineBusiness
    {
        public HoldEngineBusiness()
        {
        }

        #region Credit Hold

        /// <summary>
        /// Process records for credit hold
        /// </summary>
        /// <param name="requestObj">It contains data table required to process for credit hold</param>
        /// <returns>return success if data has been saved successfully else error</returns>
        public HoldEngineResponse ProcessCreditHold(HoldEngineRequest creditholdEngineRequest)
        {
            HoldEngineResponse creditHoldresponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            creditholdEngineRequest.ThrowIfNull("CreditholdEngineRequest");
            creditholdEngineRequest.HoldEngineEntity.CustomerInformation.ThrowIfNull("creditholdEngineRequest.HoldEngineEntity");
            try
            {
                creditHoldresponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(creditholdEngineRequest.ConnectionString);
                holdDataAccess.ProcessCreditHold(creditholdEngineRequest.HoldEngineEntity, creditholdEngineRequest.CompanyId);
                creditHoldresponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                creditHoldresponseObj.Status = HoldEngineResponseStatus.Error;
                creditHoldresponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return creditHoldresponseObj;
        }

        #endregion Credit Hold

        #region Update Cache Details

        /// <summary>
        /// Update records for cache details
        /// </summary>
        /// <param name="requestObj">It contains data table required to update for cache details</param>
        /// <returns>return success if data has been saved successfully else error</returns>
        public HoldEngineResponse ProcessCustCreditCache(HoldEngineRequest custCreditCacheRequestObj)
        {
            HoldEngineResponse custCreditCacheResponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            custCreditCacheRequestObj.ThrowIfNull("CreditholdEngineRequest");
            custCreditCacheRequestObj.HoldEngineEntity.CustomerInformation.ThrowIfNull("creditholdEngineRequest.HoldEngineEntity");
            try
            {
                custCreditCacheResponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(custCreditCacheRequestObj.ConnectionString);
                holdDataAccess.ProcessCustCreditCache(custCreditCacheRequestObj.HoldEngineEntity, custCreditCacheRequestObj.CompanyId);
                custCreditCacheResponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                custCreditCacheResponseObj.Status = HoldEngineResponseStatus.Error;
                custCreditCacheResponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return custCreditCacheResponseObj;
        }

        #endregion Update Cache Details

        #region Customer Hold

        /// <summary>
        /// Process records for customer hold
        /// </summary>
        /// <param name="requestObj">It contains data table required to process for customer hold</param>
        /// <returns>return success if data has been saved successfully else error</returns>
        public HoldEngineResponse ProcessCustomerHold(HoldEngineRequest holdEngineRequest)
        {
            HoldEngineResponse customerHoldResponseObj = null;
            IHoldEngineRepository holdDataAccess = null;
            try
            {
                customerHoldResponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(holdEngineRequest.ConnectionString);
                holdDataAccess.ProcessCustomerHold(holdEngineRequest.HoldEngineEntity, holdEngineRequest.CompanyId);
                customerHoldResponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                customerHoldResponseObj.Status = HoldEngineResponseStatus.Error;
                customerHoldResponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return customerHoldResponseObj;
        }

        #endregion Customer Hold

        #region Document Customer Hold

        /// <summary>
        /// Process records for document customer hold
        /// </summary>
        /// <param name="requestObj">It contains data table required to process for document customer hold</param>
        /// <returns>return success if data has been saved successfully else error</returns>
        public HoldEngineResponse ProcessCustomerDocumentHold(HoldEngineRequest customerDocumentHoldEngineRequest)
        {
            HoldEngineResponse customerDocumentHoldresponseObj = null;
            IHoldEngineRepository holdDataAccess = null;
            customerDocumentHoldEngineRequest.ThrowIfNull("customerDocumentHoldEngineRequest");
            customerDocumentHoldEngineRequest.HoldEngineEntity.CustomerInformation.ThrowIfNull("holdEngineRequest.CustomerInformation");
            try
            {
                customerDocumentHoldresponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(customerDocumentHoldEngineRequest.ConnectionString);
                holdDataAccess.ProcessCustomerDocumentHold(customerDocumentHoldEngineRequest.HoldEngineEntity, customerDocumentHoldEngineRequest.CompanyId);
                customerDocumentHoldresponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                customerDocumentHoldresponseObj.Status = HoldEngineResponseStatus.Error;
                customerDocumentHoldresponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return customerDocumentHoldresponseObj;
        }

        #endregion Document Customer Hold

        #region Document First Order Hold

        /// <summary>
        /// Process records for document first order hold
        /// </summary>
        /// <param name="requestObj">It contains data table required to process for document first order hold</param>
        /// <returns>return success if data has been saved successfully else error</returns>
        public HoldEngineResponse ProcessFirstOrderHold(HoldEngineRequest firstOrderholdEngineRequest)
        {
            HoldEngineResponse firstOrderHoldresponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            firstOrderholdEngineRequest.ThrowIfNull("FirstOrderholdEngineRequest");
            firstOrderholdEngineRequest.HoldEngineEntity.CustomerInformation.ThrowIfNull("holdEngineRequest.CustomerInformation");
            try
            {
                firstOrderHoldresponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(firstOrderholdEngineRequest.ConnectionString);
                holdDataAccess.ProcessFirstOrderHold(firstOrderholdEngineRequest.HoldEngineEntity, firstOrderholdEngineRequest.CompanyId);
                firstOrderHoldresponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                firstOrderHoldresponseObj.Status = HoldEngineResponseStatus.Error;
                firstOrderHoldresponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return firstOrderHoldresponseObj;
        }

        #endregion Document First Order Hold

        #region Document Manual Hold

        /// <summary>
        /// Process records for document manual hold
        /// </summary>
        /// <param name="requestObj">It contains data table required to process for document manual hold</param>
        /// <returns>return success if data has been saved successfully else error</returns>
        public HoldEngineResponse ProcessManualHold(HoldEngineRequest manualHoldRequestObj)
        {
            HoldEngineResponse manualHoldresponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            manualHoldRequestObj.ThrowIfNull("manualHoldRequestObj");
            manualHoldRequestObj.HoldEngineEntity.CustomerInformation.ThrowIfNull("holdEngineRequest.CustomerInformation");
            try
            {
                manualHoldresponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(manualHoldRequestObj.ConnectionString);
                holdDataAccess.ProcessManualHold(manualHoldRequestObj.HoldEngineEntity, manualHoldRequestObj.CompanyId);
                manualHoldresponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                manualHoldresponseObj.Status = HoldEngineResponseStatus.Error;
                manualHoldresponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return manualHoldresponseObj;
        }

        #endregion Document Manual Hold

        #region Document Sales Alert Hold

        /// <summary>
        /// Process records for document sales alert hold
        /// </summary>
        /// <param name="requestObj">It contains data table required to process for document sales alert hold</param>
        /// <returns>return success if data has been saved successfully else error</returns>
        public HoldEngineResponse ProcessSalesAlertHold(HoldEngineRequest salesAlertholdEngineRequest)
        {
            HoldEngineResponse salesAlertHoldresponseObj = null;
            IHoldEngineRepository holdDataAccess = null;
            salesAlertholdEngineRequest.ThrowIfNull("salesAlertholdEngineRequest");
            salesAlertholdEngineRequest.HoldEngineEntity.CustomerInformation.ThrowIfNull("salesAlertholdEngineRequest.CustomerInformation");
            try
            {
                salesAlertHoldresponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(salesAlertholdEngineRequest.ConnectionString);
                holdDataAccess.ProcessSalesAlertHold(salesAlertholdEngineRequest.HoldEngineEntity, salesAlertholdEngineRequest.CompanyId);
                salesAlertHoldresponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesAlertHoldresponseObj.Status = HoldEngineResponseStatus.Error;
                salesAlertHoldresponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesAlertHoldresponseObj;
        }

        #endregion Document Sales Alert Hold

        #region Document Terms Hold

        /// <summary>
        /// Process records for document term hold
        /// </summary>
        /// <param name="requestObj">It contains data table required to process for document term hold</param>
        /// <returns>return success if data has been saved successfully else error</returns>
        public HoldEngineResponse ProcessTermHold(HoldEngineRequest termHoldRequestObj)
        {
            HoldEngineResponse termHoldresponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            termHoldRequestObj.ThrowIfNull("termHoldRequestObj");
            termHoldRequestObj.HoldEngineEntity.CustomerInformation.ThrowIfNull(" termHoldRequestObj.HoldEngineEntity.CustomerInformation");
            try
            {
                termHoldresponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(termHoldRequestObj.ConnectionString);
                holdDataAccess.ProcessTermHold(termHoldRequestObj.HoldEngineEntity, termHoldRequestObj.CompanyId);
                termHoldresponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                termHoldresponseObj.Status = HoldEngineResponseStatus.Error;
                termHoldresponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return termHoldresponseObj;
        }

        #endregion Document Terms Hold

        #region Tax Hold

        /// <summary>
        /// Process records for tax hold
        /// </summary>
        /// <param name="requestObj">It contains data table required to process for tax hold</param>
        /// <returns>return success if data has been saved successfully else error</returns>
        public HoldEngineResponse ProcessTaxHold(HoldEngineRequest taxHoldRequestObj)
        {
            HoldEngineResponse taxHoldResponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            taxHoldRequestObj.ThrowIfNull("taxHoldRequestObj");
            taxHoldRequestObj.HoldEngineEntity.CustomerInformation.ThrowIfNull("taxHoldRequestObj.HoldEngineEntity");
            try
            {
                taxHoldResponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(taxHoldRequestObj.ConnectionString);
                holdDataAccess.ProcessTaxHold(taxHoldRequestObj.HoldEngineEntity, taxHoldRequestObj.CompanyId);
                taxHoldResponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                taxHoldResponseObj.Status = HoldEngineResponseStatus.Error;
                taxHoldResponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return taxHoldResponseObj;
        }

        #endregion Tax Hold

        #region Open Orders Payment Terms update

        /// <summary>
        /// Process records for payment terms hold
        /// </summary>
        /// <param name="requestObj">It contains data table required to process for payment terms hold</param>
        /// <returns>return success if data has been saved successfully else error</returns>
        public HoldEngineResponse ProcessUpdateOpenOrdersPaymentTerms(HoldEngineRequest updateOpenOrdersRequestObj)
        {
            HoldEngineResponse updateOpenOrderResponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            updateOpenOrdersRequestObj.ThrowIfNull("CreditholdEngineRequest");
            updateOpenOrdersRequestObj.HoldEngineEntity.CustomerInformation.ThrowIfNull("holdEngineRequest.CustomerInformation");
            try
            {
                updateOpenOrderResponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(updateOpenOrdersRequestObj.ConnectionString);
                holdDataAccess.ProcessUpdateOpenOrdersPaymentTerms(updateOpenOrdersRequestObj.HoldEngineEntity, updateOpenOrdersRequestObj.CompanyId);
                updateOpenOrderResponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                updateOpenOrderResponseObj.Status = HoldEngineResponseStatus.Error;
                updateOpenOrderResponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return updateOpenOrderResponseObj;
        }

        #endregion Open Orders Payment Terms update

        #region VAT Hold

        /// <summary>
        /// Process records for tax hold
        /// </summary>
        /// <param name="requestObj">It contains data table required to process for tax hold</param>
        /// <returns>return success if data has been saved successfully else error</returns>
        public HoldEngineResponse ProcessVatHold(HoldEngineRequest vatRequestObj)
        {
            HoldEngineResponse vatHoldresponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            vatRequestObj.ThrowIfNull("CreditholdEngineRequest");
            vatRequestObj.HoldEngineEntity.OrderHeader.ThrowIfNull("vatRequestObj.HoldEngineEntity");
            try
            {
                vatHoldresponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(vatRequestObj.ConnectionString);
                holdDataAccess.ProcessVatHold(vatRequestObj.HoldEngineEntity);
                vatHoldresponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                vatHoldresponseObj.Status = HoldEngineResponseStatus.Error;
                vatHoldresponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return vatHoldresponseObj;
        }

        #endregion VAT Hold

        #region CreditHoldEngine

        public HoldEngineResponse ProcessCreditHoldEngine(HoldEngineRequest creditHoldEngineRequestObj)
        {
            HoldEngineResponse creditHoldEngineResponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            creditHoldEngineRequestObj.ThrowIfNull("creditHoldEngineRequestObj");
            creditHoldEngineRequestObj.HoldEngineEntity.ThrowIfNull("creditHoldEngineRequestObj.HoldEngineEntity.CustomerInformation");
            try
            {
                creditHoldEngineResponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(creditHoldEngineRequestObj.ConnectionString);
                holdDataAccess.ProcessCreditHoldEngine(creditHoldEngineRequestObj.HoldEngineEntity, 1);
                holdDataAccess.ProcessCreditHoldEngine(creditHoldEngineRequestObj.HoldEngineEntity, 2);
                creditHoldEngineResponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                creditHoldEngineResponseObj.Status = HoldEngineResponseStatus.Error;
                creditHoldEngineResponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return creditHoldEngineResponseObj;
        }

        #endregion CreditHoldEngine

        #region ProcessHoldEngine

        public HoldEngineResponse ProcessHoldEngine(HoldEngineRequest processHoldEngineRequestObj)
        {
            HoldEngineResponse processHoldEngineResponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            processHoldEngineRequestObj.ThrowIfNull("processHoldEngineRequestObj");
            processHoldEngineRequestObj.HoldEngineEntity.ThrowIfNull("processHoldEngineRequestObj.HoldEngineEntity.CustomerInformation");
            try
            {
                processHoldEngineResponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(processHoldEngineRequestObj.ConnectionString);
                holdDataAccess.ProcessHoldEngine();
                processHoldEngineResponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                processHoldEngineResponseObj.Status = HoldEngineResponseStatus.Error;
                processHoldEngineResponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return processHoldEngineResponseObj;
        }

        #endregion processHoldEngine

        public HoldEngineResponse ProcessHoldForOrder(HoldEngineRequest processHoldEngineRequestObj)
        {
            HoldEngineResponse processHoldEngineResponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            processHoldEngineRequestObj.ThrowIfNull("processHoldEngineRequestObj");
            processHoldEngineRequestObj.HoldEngineEntity.ThrowIfNull("processHoldEngineRequestObj.HoldEngineEntity.CustomerInformation");
            try
            {
                processHoldEngineResponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(processHoldEngineRequestObj.ConnectionString);
                holdDataAccess.ProcessHoldForOrder(processHoldEngineRequestObj.HoldEngineEntity, processHoldEngineRequestObj.CompanyId);
                processHoldEngineResponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                processHoldEngineResponseObj.Status = HoldEngineResponseStatus.Error;
                processHoldEngineResponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return processHoldEngineResponseObj;
        }
        #region  freight holds
        /// <summary>
        /// Process frieght hold
        /// </summary>
        /// <param name="freightHoldRequestObj"></param>
        /// <returns></returns>
        public HoldEngineResponse ProcessFreightHold(HoldEngineRequest freightHoldRequestObj)
        {
            HoldEngineResponse freightHoldresponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            freightHoldRequestObj.ThrowIfNull("freightHoldRequestObj");
            freightHoldRequestObj.HoldEngineEntity.OrderHeader.ThrowIfNull(" freightHoldRequestObj.HoldEngineEntity.CustomerInformation");
            try
            {
                freightHoldresponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(freightHoldRequestObj.ConnectionString);
                holdDataAccess.ProcessFreightHold(freightHoldRequestObj.HoldEngineEntity, freightHoldRequestObj.CompanyId);
                freightHoldresponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                freightHoldresponseObj.Status = HoldEngineResponseStatus.Error;
                freightHoldresponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return freightHoldresponseObj;
        }

        #endregion  freight holds

        #region  Export holds
        /// <summary>
        /// Process frieght hold
        /// </summary>
        /// <param name="freightHoldRequestObj"></param>
        /// <returns></returns>
        public HoldEngineResponse ProcessExportHold(HoldEngineRequest exportHoldRequestObj)
        {
            HoldEngineResponse exportHoldresponseObj = null;
            IHoldEngineRepository holdDataAccess = null;

            exportHoldRequestObj.ThrowIfNull("exportHoldRequestObj");
            exportHoldRequestObj.HoldEngineEntity.OrderHeader.ThrowIfNull(" exportHoldRequestObj.HoldEngineEntity.CustomerInformation");
            try
            {
                exportHoldresponseObj = new HoldEngineResponse();
                holdDataAccess = new ChemPoint.GP.HoldEngineDL.HoldEngineDL(exportHoldRequestObj.ConnectionString);
                exportHoldresponseObj=holdDataAccess.ProcessExportHold(exportHoldRequestObj.HoldEngineEntity, exportHoldRequestObj.CompanyId);
                exportHoldresponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                exportHoldresponseObj.Status = HoldEngineResponseStatus.Error;
                exportHoldresponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return exportHoldresponseObj;
        }


        public HoldEngineResponse ExportHoldMailingDetails(HoldEngineRequest exportHoldRequestObj)
        {
            HoldEngineResponse exportHoldresponseObj = null;
            SendEmailRequest emailRequest = new SendEmailRequest();
            EMailInformation emailInformation = new EMailInformation();
            EmailBusiness emailBusiness = new EmailBusiness();

            exportHoldRequestObj.ThrowIfNull("exportHoldRequestObj");
            exportHoldRequestObj.HoldEngineEntity.OrderHeader.ThrowIfNull(" exportHoldRequestObj.HoldEngineEntity.CustomerInformation");
            try
            {
                exportHoldresponseObj = new HoldEngineResponse();
                DataTable exportHoldDT = ConvertListToDataTable(exportHoldRequestObj.CountryDetails);
                
                emailRequest.EmailConfigID = exportHoldRequestObj.AppConfigID;
                emailRequest.ConnectionString = exportHoldRequestObj.ConnectionString;
                emailInformation.Subject = exportHoldRequestObj.EmailRequest.EmailInformation.Subject;
                emailInformation.EmailFrom = exportHoldRequestObj.EmailRequest.EmailInformation.EmailFrom;
                emailInformation.Body = exportHoldRequestObj.EmailRequest.EmailInformation.Body;
                emailInformation.SmtpAddress = exportHoldRequestObj.EmailRequest.EmailInformation.SmtpAddress;
                emailInformation.Signature = exportHoldRequestObj.EmailRequest.EmailInformation.Signature;
                emailInformation.IsDataTableBodyRequired = true;
                emailRequest.EmailInformation = emailInformation;
                emailRequest.Report = ConvertDataTableToString(exportHoldDT);
                emailBusiness.SendEmail(emailRequest);


                exportHoldresponseObj.Status = HoldEngineResponseStatus.Success;
            }
            catch (Exception ex)
            {
                exportHoldresponseObj.Status = HoldEngineResponseStatus.Error;
                exportHoldresponseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return exportHoldresponseObj;
        }

        static DataTable ConvertListToDataTable(List<string[]> list)
        {
            // New table.
            DataTable table = new DataTable();            

            foreach (var array in list[0])
            {
                table.Columns.Add(array);
            }

            // Add rows.
            for (int x = 1; x <= list.Count - 1; x++)
            {
                table.Rows.Add(list[x]);
            }

            return table;
        }        


        private static string ConvertDataTableToString(DataTable dataTable)
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

        #endregion  Export holds






    }
}