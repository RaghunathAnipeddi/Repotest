using ChemPoint.GP.DataContracts.Setup;
using System;
using System.Linq;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Infrastructure.Logging;
using Chempoint.GP.Infrastructure.Constants;
using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Model.Interactions.Setup;
using ChemPoint.GP.BusinessContracts.Setup;

namespace ChemPoint.GP.SetupBL
{
    public class SetupDetailsBusiness : ISetupDetailUpdate
    {
        public SetupDetailsBusiness()
        {
        }

        #region TaxDetailUpdate

        /// <summary>
        /// Saves the tax details to DB
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>
        public SetupResponse SaveTaxDetailsToDB(SetupRequest taxDetailSetupRequest)
        {
            SetupResponse taxDetailSetupResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            taxDetailSetupRequest.ThrowIfNull("Tax Detail Maintenance");
            taxDetailSetupRequest.SetupEntity.ThrowIfNull("TaxDetailSetupRequest.setupDetailEntity");
            try
            {
                taxDetailSetupResponse = new SetupResponse();
                iSetupDetailUpdateRepository = new SetupDetailDL.SetupDetailDL(taxDetailSetupRequest.ConnectionString);
                iSetupDetailUpdateRepository.SaveTaxDetailFeildValues(taxDetailSetupRequest);
                taxDetailSetupResponse.Status = ResponseStatus.Success;
               
            }
            catch (Exception ex)
            {
                taxDetailSetupResponse.Status = ResponseStatus.Error;
                taxDetailSetupResponse.ErrorMessage = ex.Message.ToString().Trim();
               
            }
            return taxDetailSetupResponse;
        }

        /// <summary>
        /// Deletes the tax details from DB
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>
        public SetupResponse DeleteTaxDetailsFromDB(SetupRequest taxDetailSetupRequest)
        {
            SetupResponse taxDetailSetupResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            taxDetailSetupRequest.ThrowIfNull("Tax Detail Maintenance");
            taxDetailSetupRequest.SetupEntity.ThrowIfNull("TaxDetailSetupRequest.setupDetailEntity");
            try
            {
                taxDetailSetupResponse = new SetupResponse();
                iSetupDetailUpdateRepository = new SetupDetailDL.SetupDetailDL(taxDetailSetupRequest.ConnectionString);
                iSetupDetailUpdateRepository.DeleteTaxDetailRecord(taxDetailSetupRequest);
                taxDetailSetupResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                taxDetailSetupResponse.Status = ResponseStatus.Error;
                taxDetailSetupResponse.ErrorMessage = ex.Message.ToString().Trim();
               
            }
            return taxDetailSetupResponse;
        }

        /// <summary>
        /// Fetch the tax detail object from DB
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>
        public SetupResponse FetchTaxDetailCustomRecord(SetupRequest taxDetailSetupRequest)
        {
            SetupResponse taxDetailSetupResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            taxDetailSetupRequest.ThrowIfNull("Tax Detail Maintenance");
            taxDetailSetupRequest.SetupEntity.ThrowIfNull("TaxDetailSetupRequest.setupDetailEntity");
            try
            {
                taxDetailSetupResponse = new SetupResponse();
                iSetupDetailUpdateRepository = new SetupDetailDL.SetupDetailDL(taxDetailSetupRequest.ConnectionString);
                var taxDetailSetupFetch = iSetupDetailUpdateRepository.FetchTaxDetailInfo(taxDetailSetupRequest);
                taxDetailSetupResponse.SetupDetailsEntity = taxDetailSetupFetch.ToList().FirstOrDefault();
                taxDetailSetupResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                taxDetailSetupResponse.Status = ResponseStatus.Error;
                taxDetailSetupResponse.ErrorMessage = ex.Message.ToString().Trim();
              
            }
            return taxDetailSetupResponse;
        }

        #endregion TaxDetailUpdate

        #region TaxScheduleMaintenance

        /// <summary>
        /// Save the tax schedule to DB
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>
        public SetupResponse SaveTaxScheduleToDB(SetupRequest taxScheduledMaintenanceRequest)
        {
            SetupResponse taxScheduledMaintenanceResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            taxScheduledMaintenanceRequest.ThrowIfNull("Tax Schedule Detail Maintenance");
            taxScheduledMaintenanceRequest.SetupEntity.ThrowIfNull("taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity");
            try
            {
                taxScheduledMaintenanceResponse = new SetupResponse();
                iSetupDetailUpdateRepository = new SetupDetailDL.SetupDetailDL(taxScheduledMaintenanceRequest.ConnectionString);
                iSetupDetailUpdateRepository.SaveTaxSchduleDetails(taxScheduledMaintenanceRequest);
                taxScheduledMaintenanceResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                taxScheduledMaintenanceResponse.Status = ResponseStatus.Error;
                taxScheduledMaintenanceResponse.ErrorMessage = ex.Message.ToString().Trim();
                
            }
            return taxScheduledMaintenanceResponse;
        }

        /// <summary>
        /// Deletes the tax schedule from DB
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>
        public SetupResponse DeleteTaxScheduleFromDB(SetupRequest taxScheduledMaintenanceRequest)
        {
            SetupResponse taxScheduledMaintenanceResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            taxScheduledMaintenanceRequest.ThrowIfNull("Tax Schedule Detail Maintenance");
            taxScheduledMaintenanceRequest.SetupEntity.ThrowIfNull("taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity");
            try
            {
                taxScheduledMaintenanceResponse = new SetupResponse();
                iSetupDetailUpdateRepository = new SetupDetailDL.SetupDetailDL(taxScheduledMaintenanceRequest.ConnectionString);
                iSetupDetailUpdateRepository.DeleteTaxScheduleRecord(taxScheduledMaintenanceRequest);
                taxScheduledMaintenanceResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                taxScheduledMaintenanceResponse.Status = ResponseStatus.Error;
                taxScheduledMaintenanceResponse.ErrorMessage = ex.Message.ToString().Trim();
               
            }
            return taxScheduledMaintenanceResponse;
        }

        /// <summary>
        /// Fetch
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>
        public SetupResponse FetchTaxScheduleCustomRecord(SetupRequest taxScheduledMaintenanceRequest)
        {
            SetupResponse taxScheduledMaintenanceResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            taxScheduledMaintenanceRequest.ThrowIfNull("Tax Schedule Detail Maintenance");
            taxScheduledMaintenanceRequest.SetupEntity.ThrowIfNull("taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity");
            try
            {
                taxScheduledMaintenanceResponse = new SetupResponse();
                iSetupDetailUpdateRepository = new SetupDetailDL.SetupDetailDL(taxScheduledMaintenanceRequest.ConnectionString);
                var taxDetailSetupFetch = iSetupDetailUpdateRepository.FetchTaxScheduleInfo(taxScheduledMaintenanceRequest);
                taxScheduledMaintenanceResponse.SetupDetailsEntity = taxDetailSetupFetch.ToList().FirstOrDefault();
                taxScheduledMaintenanceResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                taxScheduledMaintenanceResponse.Status = ResponseStatus.Error;
                taxScheduledMaintenanceResponse.ErrorMessage = ex.Message.ToString().Trim();
                
            }
            return taxScheduledMaintenanceResponse;
        }

        #endregion #region TaxScheduleMaintenance

        #region PaymentTermsSetup
        
        public SetupResponse FetchPaymentTermDetail(SetupRequest paymentTermRequest)
        {
            SetupResponse paymentTermResponse = null;
            ISetupDetailUpdateRepository paymentTermDetailRepository = null;
            paymentTermRequest.ThrowIfNull("salesEntryRequest.SalesOrderBase.GetSalesEntryDetail");
            paymentTermRequest.SetupEntity.ThrowIfNull("Payment Terms Setup Maintenance");
            try
            {
                paymentTermResponse = new SetupResponse();
                paymentTermDetailRepository = new SetupDetailDL.SetupDetailDL(paymentTermRequest.ConnectionString);
                var paymentTermDetailList = paymentTermDetailRepository.GetPaymentTermCustomRecord(paymentTermRequest, paymentTermRequest.CompanyID);
                paymentTermResponse.SetupDetailsEntity = paymentTermDetailList.ToList().FirstOrDefault();
                paymentTermResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                paymentTermResponse.Status = ResponseStatus.Error;
                paymentTermResponse.ErrorMessage = ex.Message.ToString().Trim();
               
            }
            return paymentTermResponse;
        }
        
        public SetupResponse SavePaymentTermDetail(SetupRequest paymentTermRequest)
        {
            SetupResponse setupResponse = null;
            ISetupDetailUpdateRepository paymentTermRepository = null;
            
            paymentTermRequest.ThrowIfNull("Sales Entry");
            paymentTermRequest.SetupEntity.ThrowIfNull("salesEntryRequest.SalesOrderEntity.SaveSalesEntryDetail");
            
            try
            {
                setupResponse = new SetupResponse();
                paymentTermRepository = new SetupDetailDL.SetupDetailDL(paymentTermRequest.ConnectionString);
                paymentTermRepository.SavePaymentTermCustomRecord(paymentTermRequest, paymentTermRequest.CompanyID);
                setupResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                setupResponse.Status = ResponseStatus.Error;
                setupResponse.ErrorMessage = ex.Message.ToString().Trim();
                
            }
            return setupResponse;
        }
        
        public SetupResponse DeletePaymentTermDetail(SetupRequest paymentTermRequest)
        {
            SetupResponse taxScheduledMaintenanceResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            paymentTermRequest.ThrowIfNull("Tax Schedule Detail Maintenance");
            paymentTermRequest.SetupEntity.ThrowIfNull("taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity");
            try
            {
                taxScheduledMaintenanceResponse = new SetupResponse();
                iSetupDetailUpdateRepository = new SetupDetailDL.SetupDetailDL(paymentTermRequest.ConnectionString);
                iSetupDetailUpdateRepository.DeletePaymentTermCustomRecord(paymentTermRequest, paymentTermRequest.CompanyID);
                taxScheduledMaintenanceResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                taxScheduledMaintenanceResponse.Status = ResponseStatus.Error;
                taxScheduledMaintenanceResponse.ErrorMessage = ex.Message.ToString().Trim();
               
            }
            return taxScheduledMaintenanceResponse;
        }
        
        public SetupResponse GetCalulatedDueDateByPaymentTerm(SetupRequest duedateRequest, int companyID)
        {
            SetupResponse duedateResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            duedateRequest.ThrowIfNull("Sales Entry");
            duedateRequest.ThrowIfNull("salesEntryRequest.SalesOrderBase.GetSalesEntryDetail");
            try
            {
                duedateResponse = new SetupResponse();
                iSetupDetailUpdateRepository = new SetupDetailDL.SetupDetailDL(duedateRequest.ConnectionString);
                duedateResponse = iSetupDetailUpdateRepository.GetCalulatedDueDateByPaymentTerm(duedateRequest, duedateRequest.CompanyID);
                duedateResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                duedateResponse.Status = ResponseStatus.Error;
                duedateResponse.ErrorMessage = ex.Message.ToString().Trim();
                
            }
            return duedateResponse;
        }
    
        #endregion PaymentTermsSetup
    }
}
