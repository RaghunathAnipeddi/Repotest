using Chempoint.GP.Model.Interactions.Setup;
using ChemPoint.GP.DataContracts.Setup;
using System;
using System.Collections.Generic;
using Chempoint.GP.Infrastructure.Utils;
using System.Data;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;
using Chempoint.GP.Infrastructure.Config;
using Chempoint.GP.Infrastructure.Maps.Setup;
using ChemPoint.GP.Entities.Business_Entities.Setup;
using ChemPoint.GP.SetupDetailDL.Util;
using System.Data.SqlClient;
using ChemPoint.GP.Entities.BaseEntities;

namespace ChemPoint.GP.SetupDetailDL
{
    public class SetupDetailDL : RepositoryBase, ISetupDetailUpdateRepository
    {
        public SetupDetailDL(string connectionString) : base(new GpAddInDbContext(connectionString))
        {
        }

        #region TaxDetailMaintenance

        /// <summary>
        /// SaveTaxRefDetails To Save the Tax Ref Details 
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        public void SaveTaxDetailFeildValues(SetupRequest taxDetailSetupRequest)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPSaveTaxExtDetails);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxExtDetailsParam1, SqlDbType.VarChar, taxDetailSetupRequest.SetupEntity.SetupDetails.TaxDetailId, 15);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxExtDetailsParam2, SqlDbType.VarChar, taxDetailSetupRequest.SetupEntity.SetupDetails.TaxDetailReference, 100);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxExtDetailsParam3, SqlDbType.VarChar, taxDetailSetupRequest.SetupEntity.SetupDetails.TaxDetailUnivarNvTaxCode, 100);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxExtDetailsParam4, SqlDbType.VarChar, taxDetailSetupRequest.SetupEntity.SetupDetails.TaxDetailUnivarNvTaxCodeDescription, 100);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxExtDetailsParam5, SqlDbType.VarChar, taxDetailSetupRequest.UserId, 30);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxExtDetailsParam6, SqlDbType.Int, taxDetailSetupRequest.CompanyID);
            base.Insert(cmd);
        }

        /// <summary>
        /// DeleteTaxRefDetail To Delete the Tax Ref Details
        /// </summary>
        /// <param name="taxDetailID"></param>
        public void DeleteTaxDetailRecord(SetupRequest deleteTaxDetailRecord)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPDeleteTaxExtDetails);
            cmd.Parameters.AddInputParams(Configuration.SPDeleteTaxExtDetailsParam1, SqlDbType.VarChar, deleteTaxDetailRecord.SetupEntity.SetupDetails.TaxDetailId, 15);
            base.Delete(cmd);
        }

        /// <summary>
        /// GetTaxRefDetail 
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>
        public IEnumerable<SetupEntity> FetchTaxDetailInfo(SetupRequest taxDetailSetupRequest)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPGetTaxExtDetails);
            cmd.Parameters.AddInputParams(Configuration.SPGetTaxExtDetailsParam1, SqlDbType.VarChar, taxDetailSetupRequest.SetupEntity.SetupDetails.TaxDetailId, 15);
            return base.FindAll<SetupEntity, TaxDetailMaintenanceMap>(cmd);
        }

        #endregion TaxDetailMaintenance

        #region TaxScheduleMaintenance

        /// <summary>
        /// SaveTaxRefDetails To Save the Tax Ref Details 
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        public void SaveTaxSchduleDetails(SetupRequest taxScheduledMaintenanceRequest)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPSaveTaxScheduleVatDetails);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxScheduleVatDetailsParam1, SqlDbType.VarChar, taxScheduledMaintenanceRequest.SetupEntity.SetupDetails.TaxScheduleId, 15);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxScheduleVatDetailsParam2, SqlDbType.VarChar, taxScheduledMaintenanceRequest.SetupEntity.SetupDetails.TaxScheduleChempointVatNumber, 100);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxScheduleVatDetailsParam3, SqlDbType.Bit, taxScheduledMaintenanceRequest.SetupEntity.SetupDetails.TaxScheduleIsActive);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxScheduleVatDetailsParam4, SqlDbType.VarChar, taxScheduledMaintenanceRequest.UserId, 30);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxScheduleVatDetailsParam5, SqlDbType.Int, taxScheduledMaintenanceRequest.CompanyID);
            base.Insert(cmd);
        }

        /// <summary>
        /// DeleteTaxRefDetail To Delete the Tax Ref Details
        /// </summary>
        /// <param name="taxDetailID"></param>
        public void DeleteTaxScheduleRecord(SetupRequest taxScheduledMaintenanceRequest)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPDeleteTaxSchduleDetails);
            cmd.Parameters.AddInputParams(Configuration.SPDeleteTaxSchduleDetailsParam1, SqlDbType.VarChar, taxScheduledMaintenanceRequest.SetupEntity.SetupDetails.TaxScheduleId, 15);
            base.Delete(cmd);
        }

        /// <summary>
        /// GetTaxRefDetail 
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>
        public IEnumerable<SetupEntity> FetchTaxScheduleInfo(SetupRequest taxScheduledMaintenanceRequest)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPGetTaxScheduleDetails);
            cmd.Parameters.AddInputParams(Configuration.SPGetTaxScheduleDetailsParam1, SqlDbType.VarChar, taxScheduledMaintenanceRequest.SetupEntity.SetupDetails.TaxScheduleId, 20);
            return base.FindAll<SetupEntity, TaxScheduledMaintenanceMap>(cmd);
        }

        #endregion TaxScheduleMaintenance

        #region PaymentTermsSetup

        public IEnumerable<SetupEntity> GetPaymentTermCustomRecord(SetupRequest paymentTermsRequest, int companyID)
        {
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPFetchPaymentTermSetupDetailsNA : Configuration.SPFetchPaymentTermSetupDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPFetchPaymentTermSetupDetailsParam1, SqlDbType.VarChar, paymentTermsRequest.SetupEntity.PaymentTermsDetails.PaymentTermsID);
            return base.FindAll<SetupEntity, PaymentTermsSetupMap>(cmd);
        }

        public void SavePaymentTermCustomRecord(SetupRequest paymentTermsRequest, int companyID)
        {
            DataTable paymentTermDT = DataTableMapper.SavePaymentTermsDataTable(paymentTermsRequest,
                DataColumnMappings.SavePaymentTermsDetails);

            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPSavePaymentTermsSetupNA : Configuration.SPSavePaymentTermsSetupEU);
            cmd.Parameters.AddInputParams(Configuration.SPSavePaymentTermsSetupParam1, SqlDbType.VarChar, paymentTermsRequest.SetupEntity.AuditInformation.ModifiedBy, 50);
            cmd.Parameters.AddInputParams(Configuration.SPSavePaymentTermsSetupParam2, SqlDbType.Int, paymentTermsRequest.CompanyID, 2);
            cmd.Parameters.AddInputParams(Configuration.SPSavePaymentTermsSetupParam3, SqlDbType.Structured, paymentTermDT);
            base.Insert(cmd);
        }

        public void DeletePaymentTermCustomRecord(SetupRequest paymentTermsRequest, int companyID)
        {
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPDeletePaymentTermsDetailsNA : Configuration.SPDeletePaymentTermsDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPDeletePaymentTermsDetailsParam1, SqlDbType.VarChar, paymentTermsRequest.SetupEntity.PaymentTermsDetails.PaymentTermsID, 21);
            base.Delete(cmd);
        }

        public SetupResponse GetCalulatedDueDateByPaymentTerm(SetupRequest duedateRequest, int companyID)
        {
            SetupResponse response = new SetupResponse();
            SetupEntity setupEntity = new SetupEntity();
            PaymentTermsInformation ptInformation = new PaymentTermsInformation();

            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SpnaGetPaymentTermsDueDate : Configuration.SpeuGetPaymentTermsDueDate);
            cmd.Parameters.AddInputParams(Configuration.SPGetPaymentTermsDueDateParam1, SqlDbType.DateTime, duedateRequest.SetupEntity.PaymentTermsDetails.DocDate);
            cmd.Parameters.AddInputParams(Configuration.SPGetPaymentTermsDueDateParam2, SqlDbType.VarChar, duedateRequest.SetupEntity.PaymentTermsDetails.PaymentTermsID, 15);
            cmd.Parameters.AddOutputParams(Configuration.SPGetPaymentTermsDueDateParam3, SqlDbType.DateTime);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            ptInformation.Duedate = commandResult.Parameters["@" + Configuration.SPGetPaymentTermsDueDateParam3].Value == DBNull.Value ? duedateRequest.SetupEntity.PaymentTermsDetails.DocDate : Convert.ToDateTime(commandResult.Parameters["@" + Configuration.SPGetPaymentTermsDueDateParam3].Value);
            setupEntity.PaymentTermsDetails = ptInformation;
            response.SetupDetailsEntity = setupEntity;
            return response;
        }
        
        #endregion PaymentTermsSetup
    }
}
