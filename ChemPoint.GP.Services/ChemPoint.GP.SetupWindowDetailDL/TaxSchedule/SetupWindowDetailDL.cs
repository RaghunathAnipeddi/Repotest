using Chempoint.GP.Model.Interactions.Setup;
using ChemPoint.GP.DataContracts.Setup;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chempoint.GP.Infrastructure.Utils;
using System.Data;
using ChemPoint.GP.Entities.Business_Entities;
using Chempoint.GP.Infrastructure.Config;
using Chempoint.GP.Infrastructure.Maps.Setup;

//using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;

namespace ChemPoint.GP.SetupWindowDetailDL
{
    public class SetupWindowDetailDL : RepositoryBase, ISetupDetailUpdateRepository
    {
        public SetupWindowDetailDL(string connectionString)
            : base(new GpAddInDbContext(connectionString))
        {

        }
        #region TaxDetailMaintenance
        /// <summary>
        /// SaveTaxRefDetails To Save the Tax Ref Details 
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        public void SaveTaxRefDetails(TaxDetailSetupRequest taxDetailSetupRequest)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPSaveTaxExtDetails);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxExtDetails_Param1, SqlDbType.VarChar, taxDetailSetupRequest.setupDetailEntity.TaxDetailId, 15);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxExtDetails_Param2, SqlDbType.VarChar, taxDetailSetupRequest.setupDetailEntity.TaxDetailReference,100);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxExtDetails_Param3, SqlDbType.VarChar, taxDetailSetupRequest.setupDetailEntity.UnivarNvTaxCode, 100);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxExtDetails_Param4, SqlDbType.VarChar, taxDetailSetupRequest.setupDetailEntity.UnivarNvTaxCodeDescription,100);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxExtDetails_Param5, SqlDbType.VarChar, taxDetailSetupRequest.setupDetailEntity.UserId,30);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxExtDetails_Param6, SqlDbType.VarChar, taxDetailSetupRequest.CompanyID, 30);
            base.Insert(cmd);

        }

        /// <summary>
        /// DeleteTaxRefDetail To Delete the Tax Ref Details
        /// </summary>
        /// <param name="taxDetailID"></param>
        public void DeleteTaxRefDetail(TaxDetailSetupRequest taxDetailID)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPDeleteTaxExtDetails);
            cmd.Parameters.AddInputParams(Configuration.SPDeleteTaxExtDetails_Param1, SqlDbType.VarChar, taxDetailID.setupDetailEntity.TaxDetailId, 15);
            base.Delete(cmd);
        }

        /// <summary>
        /// GetTaxRefDetail 
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>
        public IEnumerable<SetupDetailEntity> GetTaxRefDetail(TaxDetailSetupRequest taxDetailSetupRequest)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPGetTaxExtDetails);
            cmd.Parameters.AddInputParams(Configuration.SPGetTaxExtDetails_Param1, SqlDbType.VarChar, taxDetailSetupRequest.setupDetailEntity.TaxDetailId, 15);
            return base.FindAll<SetupDetailEntity, TaxDetailMaintenanceMap>(cmd);
        }
        #endregion TaxDetailMaintenance
        #region TaxScheduleMaintenance
        /// <summary>
        /// SaveTaxRefDetails To Save the Tax Ref Details 
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        public void SaveTaxSchduleDetails(TaxScheduledMaintenanceRequest taxScheduledMaintenanceRequest)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPSaveTaxScheduleVATDetails);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxScheduleVATDetails_Param1, SqlDbType.VarChar, taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity.TaxScheduleId, 15);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxScheduleVATDetails_Param2, SqlDbType.VarChar, taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity.ChempointVatNumber, 100);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxScheduleVATDetails_Param3, SqlDbType.VarChar, taxScheduledMaintenanceRequest.UserId, 30);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTaxScheduleVATDetails_Param4, SqlDbType.VarChar, taxScheduledMaintenanceRequest.CompanyID, 10);
            base.Insert(cmd);

        }

        /// <summary>
        /// DeleteTaxRefDetail To Delete the Tax Ref Details
        /// </summary>
        /// <param name="taxDetailID"></param>
        public void DeleteTaxScheduleDetail(TaxScheduledMaintenanceRequest taxScheduledMaintenanceRequest)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPDeleteTaxSchduleDetails);
            cmd.Parameters.AddInputParams(Configuration.SPDeleteTaxSchduleDetails_Param1, SqlDbType.VarChar, taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity.TaxScheduleId, 15);
            base.Delete(cmd);
        }

        /// <summary>
        /// GetTaxRefDetail 
        /// </summary>
        /// <param name="taxDetailSetupRequest"></param>
        /// <returns></returns>
        public IEnumerable<TaxScheduledMaintenanceEntity> GetTaxScheduleDetail(TaxScheduledMaintenanceRequest taxScheduledMaintenanceRequest)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPGetTaxScheduleDetails);
            cmd.Parameters.AddInputParams(Configuration.SPGetTaxScheduleDetails_Param1, SqlDbType.VarChar, taxScheduledMaintenanceRequest.taxScheduledMaintenanceEntity.TaxScheduleId, 15);
            return base.FindAll<TaxScheduledMaintenanceEntity, TaxScheduledMaintenanceMap>(cmd);
        }


        #endregion TaxScheduleMaintenance
    }
}
