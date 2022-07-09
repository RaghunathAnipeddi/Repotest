using System.Collections.Generic;
using System.Data;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.HoldEngine;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;
using ChemPoint.GP.HoldEngineDL.Utils;
using Chempoint.GP.Infrastructure.Config;
using ChemPoint.GP.DataContracts.TaskScheduler.HoldEngine;
using ChemPoint.GP.Entities.BaseEntities;
using System.Data.SqlClient;
using Chempoint.GP.Model.Interactions.HoldEngine;
using System;

namespace ChemPoint.GP.HoldEngineDL
{
    public class HoldEngineDL : RepositoryBase, IHoldEngineRepository
    {
        public HoldEngineDL(string connectionString) : base(new GpAddInDbContext(connectionString))
        {
        }

        public object ProcessCreditHold(HoldEngineEntity holdEngineEntity, int companyID)
        {
            DataTable customerListDT = DataTableMapper.GetCustomerDataTable(new List<CustomerInformation>() { holdEngineEntity.CustomerInformation },
                DataColumnMappings.GetCustomerList);
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPProcessCreditHoldNA : Configuration.SPProcessCreditHoldEU);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopNumber, SqlDbType.VarChar, holdEngineEntity.OrderHeader.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopType, SqlDbType.Int, holdEngineEntity.OrderHeader.SopType);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamBatchUserId, SqlDbType.VarChar, holdEngineEntity.BatchUserID);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamCustomerList, SqlDbType.Structured, customerListDT);
            return base.Insert(cmd);
        }

        public object ProcessCustomerHold(HoldEngineEntity holdEngineEntity, int companyID)
        {
            DataTable customerListDT = DataTableMapper.GetCustomerDataTable(new List<CustomerInformation>() { holdEngineEntity.CustomerInformation },
                DataColumnMappings.GetCustomerList);
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPProcessCustomerHoldNA : Configuration.SPProcessCustomerHoldEU);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamCustomerList, SqlDbType.Structured, customerListDT);
            return base.Insert(cmd);
        }

        public object ProcessCustomerDocumentHold(HoldEngineEntity holdEngineEntity, int companyID)
        {
            DataTable customerListDT = DataTableMapper.GetCustomerDataTable(new List<CustomerInformation>() { holdEngineEntity.CustomerInformation },
                DataColumnMappings.GetCustomerList);
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPProcessDocumentCustomerHoldNA : Configuration.SPProcessDocumentCustomerHoldEU);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopNumber, SqlDbType.VarChar, holdEngineEntity.OrderHeader.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopType, SqlDbType.Int, holdEngineEntity.OrderHeader.SopType);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamBatchUserId, SqlDbType.VarChar, holdEngineEntity.BatchUserID);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamCustomerList, SqlDbType.Structured, customerListDT);
            return base.Insert(cmd);
        }

        public object ProcessFirstOrderHold(HoldEngineEntity holdEngineEntity, int companyID)
        {
            DataTable customerListDT = DataTableMapper.GetCustomerDataTable(new List<CustomerInformation>() { holdEngineEntity.CustomerInformation },
                DataColumnMappings.GetCustomerList);
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPProcessDocumentCustomerHoldNA : Configuration.SPProcessDocumentCustomerHoldEU);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopNumber, SqlDbType.VarChar, holdEngineEntity.OrderHeader.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopType, SqlDbType.Int, holdEngineEntity.OrderHeader.SopType);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamBatchUserId, SqlDbType.VarChar, holdEngineEntity.BatchUserID);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamCustomerList, SqlDbType.Structured, customerListDT);
            return base.Insert(cmd);
        }

        public object ProcessSalesAlertHold(HoldEngineEntity holdEngineEntity, int companyID)
        {
            DataTable customerListDT = DataTableMapper.GetCustomerDataTable(new List<CustomerInformation>() { holdEngineEntity.CustomerInformation },
                DataColumnMappings.GetCustomerList);
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPProcessDocumentSalesAlertHoldNA : Configuration.SPProcessDocumentSalesAlertHoldEU);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopNumber, SqlDbType.VarChar, holdEngineEntity.OrderHeader.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopType, SqlDbType.Int, holdEngineEntity.OrderHeader.SopType);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamBatchUserId, SqlDbType.VarChar, holdEngineEntity.BatchUserID);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamCustomerList, SqlDbType.Structured, customerListDT);
            return base.Insert(cmd);
        }

        public object ProcessManualHold(HoldEngineEntity holdEngineEntity, int companyID)
        {
            DataTable customerListDT = DataTableMapper.GetCustomerDataTable(new List<CustomerInformation>() { holdEngineEntity.CustomerInformation },
                DataColumnMappings.GetCustomerList);
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPProcessDocumentManualHoldNA : Configuration.SPProcessDocumentManualHoldEU);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopNumber, SqlDbType.VarChar, holdEngineEntity.OrderHeader.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopType, SqlDbType.Int, holdEngineEntity.OrderHeader.SopType);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamBatchUserId, SqlDbType.VarChar, holdEngineEntity.BatchUserID);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamCustomerList, SqlDbType.Structured, customerListDT);
            return base.Insert(cmd);
        }

        public object ProcessTermHold(HoldEngineEntity holdEngineEntity, int companyID)
        {
            DataTable customerListDT = DataTableMapper.GetCustomerDataTable(new List<CustomerInformation>() { holdEngineEntity.CustomerInformation },
                DataColumnMappings.GetCustomerList);
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPProcessDocumentTermsHoldNA : Configuration.SPProcessDocumentTermsHoldEU);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopNumber, SqlDbType.VarChar, holdEngineEntity.OrderHeader.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopType, SqlDbType.Int, holdEngineEntity.OrderHeader.SopType);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamBatchUserId, SqlDbType.VarChar, holdEngineEntity.BatchUserID);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamCustomerList, SqlDbType.Structured, customerListDT);
            return base.Insert(cmd);
        }

        public object ProcessUpdateOpenOrdersPaymentTerms(HoldEngineEntity holdEngineEntity, int companyID)
        {
            DataTable customerListDT = DataTableMapper.GetCustomerDataTable(new List<CustomerInformation>() { holdEngineEntity.CustomerInformation },
                DataColumnMappings.GetCustomerList);
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPProcessOpenOrdersPaymentTermsNA : Configuration.SPProcessOpenOrdersPaymentTermsEU);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamCustomerList, SqlDbType.Structured, customerListDT);
            return base.Insert(cmd);
        }

        public object ProcessCustCreditCache(HoldEngineEntity holdEngineEntity, int companyID)
        {
            DataTable customerListDT = DataTableMapper.GetCustomerDataTable(new List<CustomerInformation>() { holdEngineEntity.CustomerInformation },
                DataColumnMappings.GetCustomerList);
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPProcessCustCreditStandCacheNA : Configuration.SPProcessCustCreditStandCacheEU);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamCustomerList, SqlDbType.Structured, customerListDT);
            return base.Insert(cmd);
        }

        public object ProcessTaxHold(HoldEngineEntity holdEngineEntity, int companyID)
        {
            DataTable customerListDT = DataTableMapper.GetCustomerDataTable(new List<CustomerInformation>() { holdEngineEntity.CustomerInformation },
                DataColumnMappings.GetCustomerList);
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPProcessDocumentTermsHoldNA : Configuration.SPProcessDocumentTermsHoldEU);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopNumber, SqlDbType.VarChar, holdEngineEntity.OrderHeader.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopType, SqlDbType.Int, holdEngineEntity.OrderHeader.SopType);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamBatchUserId, SqlDbType.VarChar, holdEngineEntity.BatchUserID);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamCustomerList, SqlDbType.Structured, customerListDT);
            return base.Insert(cmd);
        }

        public object ProcessCreditHoldEngine(HoldEngineEntity holdEngineEntity, int companyID)
        {
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPProcessCreditHoldEngineNA : Configuration.SPProcessCreditHoldEngineEU);
            return base.Insert(cmd);
        }

        public object ProcessHoldEngine()
        {
            var cmd = CreateStoredProcCommand(Configuration.SPProcessHoldEngine);
            return base.Insert(cmd);
        }

        public object ProcessVatHold(HoldEngineEntity holdEngineEntity)
        {
            var cmd = CreateStoredProcCommand(Configuration.VatHoldUpdateSP);
            cmd.Parameters.AddInputParams(Configuration.SopNumberParam, SqlDbType.VarChar, holdEngineEntity.OrderHeader.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SopTypeParam, SqlDbType.Int, holdEngineEntity.OrderHeader.SopType);
            cmd.Parameters.AddInputParams(Configuration.VatLocationCode, SqlDbType.VarChar, holdEngineEntity.OrderHeader.LocationCode, 50);
            cmd.Parameters.AddInputParams(Configuration.VatSubTotal, SqlDbType.Decimal, holdEngineEntity.OrderHeader.SubTotalAmount);
            cmd.Parameters.AddInputParams(Configuration.VatTaxScheduleId, SqlDbType.VarChar, holdEngineEntity.OrderHeader.TaxScheduleId, 50);
            cmd.Parameters.AddInputParams(Configuration.VatTaxRegNumebr, SqlDbType.VarChar, holdEngineEntity.OrderHeader.TaxRegNumber, 50);
            cmd.Parameters.AddInputParams(Configuration.VatShiptoAddress, SqlDbType.VarChar, holdEngineEntity.OrderHeader.ShipToAddress.AddressCode, 50);
            cmd.Parameters.AddInputParams(Configuration.VatUserId, SqlDbType.VarChar, holdEngineEntity.BatchUserID);
            return base.Insert(cmd);
        }

        public object ProcessHoldForOrder(HoldEngineEntity holdEngineEntity, int companyID)
        {
            var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPProcessCreditHoldEngineForOrderNA : Configuration.SPProcessCreditHoldEngineForOrderEU);
            cmd.Parameters.AddInputParams(Configuration.SopNumberParam, SqlDbType.VarChar, holdEngineEntity.OrderHeader.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SopTypeParam, SqlDbType.Int, holdEngineEntity.OrderHeader.SopType);
            return base.Insert(cmd);
        }

        public object ProcessFreightHold(HoldEngineEntity holdEngineEntity, int companyID)
        {
            DataTable customerListDT = DataTableMapper.GetCustomerDataTable(new List<CustomerInformation>() { holdEngineEntity.CustomerInformation },
                DataColumnMappings.GetCustomerList);
            var cmd = CreateStoredProcCommand(Configuration.SPProcessDocumentFreightHoldNA);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopNumber, SqlDbType.VarChar, holdEngineEntity.OrderHeader.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopType, SqlDbType.Int, holdEngineEntity.OrderHeader.SopType);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamBatchUserId, SqlDbType.VarChar, holdEngineEntity.BatchUserID);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamCustomerList, SqlDbType.Structured, customerListDT);
            return base.Insert(cmd);
        }

        public HoldEngineResponse ProcessExportHold(HoldEngineEntity holdEngineEntity, int companyID)
        {
            HoldEngineResponse holdEngineResponse = new HoldEngineResponse();
            var cmd = CreateStoredProcCommand(Configuration.SPProcessDocumentExportHoldNA);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopNumber, SqlDbType.VarChar, holdEngineEntity.OrderHeader.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamSopType, SqlDbType.Int, holdEngineEntity.OrderHeader.SopType);
            cmd.Parameters.AddInputParams(Configuration.ProcessHoldParamBatchUserId, SqlDbType.VarChar, holdEngineEntity.BatchUserID);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldShipFromCountry, SqlDbType.VarChar, holdEngineEntity.OrderHeader.ShipFromCountry);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldShipFromCC, SqlDbType.VarChar, holdEngineEntity.OrderHeader.ShipFromCountryCode);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldShipToCountry, SqlDbType.VarChar, holdEngineEntity.OrderHeader.ShipToCountry);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldShipToCC, SqlDbType.VarChar, holdEngineEntity.OrderHeader.ShipToCountryCode);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldFinalCountry, SqlDbType.VarChar, holdEngineEntity.OrderHeader.FinalDestinationCountry);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldFinalCC, SqlDbType.VarChar, holdEngineEntity.OrderHeader.FinalDestinationCountryCode);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldNewShipFromCountry, SqlDbType.VarChar, holdEngineEntity.OrderHeader.NewShipFromCountry);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldNewShipFromCC, SqlDbType.VarChar, holdEngineEntity.OrderHeader.NewShipFromCountryCode);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldNewShipToCountry, SqlDbType.VarChar, holdEngineEntity.OrderHeader.NewShipToCountry);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldNewShipToCC, SqlDbType.VarChar, holdEngineEntity.OrderHeader.NewShipToCountryCode);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldNewFinalCountry, SqlDbType.VarChar, holdEngineEntity.OrderHeader.NewFinalDestinationCountry);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldNewFinalCC, SqlDbType.VarChar, holdEngineEntity.OrderHeader.NewFinalDestinationCountryCode);
            cmd.Parameters.AddInputParams(Configuration.SPProcessDocumentExportHoldIsLineAdd, SqlDbType.Bit, holdEngineEntity.OrderHeader.IsLineAdded);
            cmd.Parameters.AddOutputParams(Configuration.SPProcessDocumentExportHoldResult, SqlDbType.VarChar, 61);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            holdEngineResponse.Result = Convert.ToInt16(commandResult.Parameters["@" + Configuration.SPProcessDocumentExportHoldResult].Value);
            return holdEngineResponse;
            //return base.Insert(cmd);
        }
    }
}
