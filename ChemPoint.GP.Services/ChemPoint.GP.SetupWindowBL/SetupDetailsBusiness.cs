using Chempoint.GP.Model.Interactions.Setup;
using ChemPoint.GP.BusinessContracts.Sales;
using ChemPoint.GP.DataContracts.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Model.Interactions.Sales;
using Chempoint.GP.Infrastructure.Logging;
using Chempoint.GP.Infrastructure.Constants;
using Chempoint.GP.Infrastructure.Exceptions;
using ChemPoint.GP.BusinessContracts.Setup;

namespace ChemPoint.GP.SetupWindowBL
{
    public class SetupDetailsBusiness : ISetupDetailUpdate
    {
        public SetupDetailsBusiness()
        {

        }


        #region TaxDetailUpdate
        /// <summary>
        /// Save
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>

        public TaxDetailSetupResponse SaveTaxExtDetails(TaxDetailSetupRequest taxDetailSetupRequest)
        {
            TaxDetailSetupResponse taxDetailSetupResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            taxDetailSetupRequest.ThrowIfNull("Tax Detail Maintenance");
            taxDetailSetupRequest.setupDetailEntity.ThrowIfNull("TaxDetailSetupRequest.setupDetailEntity");
            try
            {
                taxDetailSetupResponse = new TaxDetailSetupResponse();
                iSetupDetailUpdateRepository = new ChemPoint.GP.SetupWindowDetailDL.SetupWindowDetailDL(taxDetailSetupRequest.ConnectionString);
                iSetupDetailUpdateRepository.SaveTaxRefDetails(taxDetailSetupRequest);
                taxDetailSetupResponse.Status = SetupExtResponseStatus.Success;
                return taxDetailSetupResponse;
            }
            catch (Exception ex)
            {

                taxDetailSetupResponse.Status = SetupExtResponseStatus.Error;
                ExceptionLogger.LogException(ex, ApplicationConstants.ExceptionPolicy);
                throw new SetupExtException("Exception while GetSalesItemDetail");
            }


        }

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>

        public TaxDetailSetupResponse DeleteTaxExtDetails(TaxDetailSetupRequest taxDetailSetupRequest)
        {
            TaxDetailSetupResponse taxDetailSetupResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            taxDetailSetupRequest.ThrowIfNull("Tax Detail Maintenance");
            taxDetailSetupRequest.setupDetailEntity.ThrowIfNull("TaxDetailSetupRequest.setupDetailEntity");
            try
            {
                taxDetailSetupResponse = new TaxDetailSetupResponse();
                iSetupDetailUpdateRepository = new ChemPoint.GP.SetupWindowDetailDL.SetupWindowDetailDL(taxDetailSetupRequest.ConnectionString);
                iSetupDetailUpdateRepository.DeleteTaxRefDetail(taxDetailSetupRequest);
                taxDetailSetupResponse.Status = SetupExtResponseStatus.Success;
                return taxDetailSetupResponse;
            }
            catch (Exception ex)
            {

                taxDetailSetupResponse.Status = SetupExtResponseStatus.Error;
                ExceptionLogger.LogException(ex, ApplicationConstants.ExceptionPolicy);
                throw new SetupExtException("Exception while GetSalesItemDetail");
            }


        }

        /// <summary>
        /// Fetch
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>

        public TaxDetailSetupResponse GetTaxExtDetails(TaxDetailSetupRequest taxDetailSetupRequest)
        {
            TaxDetailSetupResponse taxDetailSetupResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            taxDetailSetupRequest.ThrowIfNull("Tax Detail Maintenance");
            taxDetailSetupRequest.setupDetailEntity.ThrowIfNull("TaxDetailSetupRequest.setupDetailEntity");
            try
            {
                taxDetailSetupResponse = new TaxDetailSetupResponse();
                iSetupDetailUpdateRepository = new ChemPoint.GP.SetupWindowDetailDL.SetupWindowDetailDL(taxDetailSetupRequest.ConnectionString);
                var taxDetailSetupFetch = iSetupDetailUpdateRepository.GetTaxRefDetail(taxDetailSetupRequest);
                taxDetailSetupResponse.setupTaxDetail = taxDetailSetupFetch.ToList().FirstOrDefault(); 
                taxDetailSetupResponse.Status = SetupExtResponseStatus.Success;
                return taxDetailSetupResponse;
            }
            catch (Exception ex)
            {

                taxDetailSetupResponse.Status = SetupExtResponseStatus.Error;
                ExceptionLogger.LogException(ex, ApplicationConstants.ExceptionPolicy);
                throw new SetupExtException("Exception while GetSalesItemDetail");
            }


        }

        #endregion TaxDetailUpdate

        #region #region TaxScheduleMaintenance

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>

        public TaxScheduledMaintenanceResponse SaveTaxScheduleDetails(TaxScheduledMaintenanceRequest taxScheduledMaintenanceRequest)
        {
            TaxScheduledMaintenanceResponse taxScheduledMaintenanceResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            taxScheduledMaintenanceRequest.ThrowIfNull("Tax Schedule Detail Maintenance");
            taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity.ThrowIfNull("taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity");
            try
            {
                taxScheduledMaintenanceResponse = new TaxScheduledMaintenanceResponse();
                iSetupDetailUpdateRepository = new ChemPoint.GP.SetupWindowDetailDL.SetupWindowDetailDL(taxScheduledMaintenanceRequest.ConnectionString);
                iSetupDetailUpdateRepository.SaveTaxSchduleDetails(taxScheduledMaintenanceRequest);
                taxScheduledMaintenanceResponse.Status = TaxSchdResponseStatus.Success;
                return taxScheduledMaintenanceResponse;
            }
            catch (Exception ex)
            {

                taxScheduledMaintenanceResponse.Status = TaxSchdResponseStatus.Error;
                ExceptionLogger.LogException(ex, ApplicationConstants.ExceptionPolicy);
                throw new SetupExtException("Exception while SaveTaxScheduleDetails");
            }


        }

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>

        public TaxScheduledMaintenanceResponse DeleteTaxScheduleDetails(TaxScheduledMaintenanceRequest taxScheduledMaintenanceRequest)
        {
            TaxScheduledMaintenanceResponse taxScheduledMaintenanceResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            taxScheduledMaintenanceRequest.ThrowIfNull("Tax Schedule Detail Maintenance");
            taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity.ThrowIfNull("taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity");
            try
            {
                taxScheduledMaintenanceResponse = new TaxScheduledMaintenanceResponse();
                iSetupDetailUpdateRepository = new ChemPoint.GP.SetupWindowDetailDL.SetupWindowDetailDL(taxScheduledMaintenanceRequest.ConnectionString);
                iSetupDetailUpdateRepository.DeleteTaxScheduleDetail(taxScheduledMaintenanceRequest);
                taxScheduledMaintenanceResponse.Status =TaxSchdResponseStatus.Success;
                return taxScheduledMaintenanceResponse;
            }
            catch (Exception ex)
            {

                taxScheduledMaintenanceResponse.Status =TaxSchdResponseStatus.Error;
                ExceptionLogger.LogException(ex, ApplicationConstants.ExceptionPolicy);
                throw new SetupExtException("Exception while DeleteTaxScheduleDetails");
            }


        }

        /// <summary>
        /// Fetch
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>

        public TaxScheduledMaintenanceResponse GetTaxScheduleDetails(TaxScheduledMaintenanceRequest taxScheduledMaintenanceRequest)
        {
            TaxScheduledMaintenanceResponse taxScheduledMaintenanceResponse = null;
            ISetupDetailUpdateRepository iSetupDetailUpdateRepository = null;
            taxScheduledMaintenanceRequest.ThrowIfNull("Tax Schedule Detail Maintenance");
            taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity.ThrowIfNull("taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity");
            try
            {
                taxScheduledMaintenanceResponse = new TaxScheduledMaintenanceResponse();
                iSetupDetailUpdateRepository = new ChemPoint.GP.SetupWindowDetailDL.SetupWindowDetailDL(taxScheduledMaintenanceRequest.ConnectionString);
                var taxDetailSetupFetch = iSetupDetailUpdateRepository.GetTaxScheduleDetail(taxScheduledMaintenanceRequest);
                taxScheduledMaintenanceResponse.setupDetailEntity = taxDetailSetupFetch.ToList();
                taxScheduledMaintenanceResponse.Status =TaxSchdResponseStatus.Success;
                return taxScheduledMaintenanceResponse;
            }
            catch (Exception ex)
            {

                taxScheduledMaintenanceResponse.Status =TaxSchdResponseStatus.Error;
                ExceptionLogger.LogException(ex, ApplicationConstants.ExceptionPolicy);
                throw new SetupExtException("Exception while GetTaxScheduleDetails");
            }
        }



        #endregion #region TaxScheduleMaintenance

    }
}
