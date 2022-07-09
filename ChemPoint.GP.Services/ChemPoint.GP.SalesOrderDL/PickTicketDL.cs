using Chempoint.GP.Infrastructure.Config;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;
using Chempoint.GP.Infrastructure.Utils;
using ChemPoint.GP.DataContracts.Sales;
using System.Data;
using System.Data.SqlClient;

namespace ChemPoint.GP.SalesOrderDL
{
    public class PickTicketDL : RepositoryBase, ISalesOrderPickTicketRepository
    {
        public PickTicketDL(string connectionString) : base(new GpAddInDbContext(connectionString))
        {
        }

        public PickTicketDL(SqlConnection connectionString) : base(new GpAddInDbContext(connectionString))
        {
        }

        #region PickTicket
        
        public object GetSalesOrderDetailsForPT(string invoiceNumber, int invoiceType)
        {
            DataSet orderDS = null;
            var cmd = CreateStoredProcCommand(Configuration.SPGetOrderPTDetails);
            cmd.Parameters.AddInputParams(Configuration.SPGetOrderPTDetailsParam1, SqlDbType.VarChar, invoiceNumber, 31);
            cmd.Parameters.AddInputParams(Configuration.SPGetOrderPTDetailsParam2, SqlDbType.Int, invoiceType);
            
            orderDS = base.GetDataSet(cmd);
            return orderDS;
        }
        
        public object SendPTToWHandChr(string invoiceNumber)
        {
            DataSet ptResult = new DataSet();
            
            if (!string.IsNullOrEmpty(invoiceNumber))
            {
                var cmd = CreateStoredProcCommand(Configuration.SPSubmitPTtoWHandChr);
                cmd.Parameters.AddInputParams(Configuration.SPSubmitPTtoWHandChrParam1, SqlDbType.VarChar, invoiceNumber, 31);
                
                ptResult = base.GetDataSet(cmd);
            }
            
            return ptResult;
        }
        
        /// <summary>
        /// Update Sop Status
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public object UpdateSalesDocumentStatus(string invoiceNumber, int companyID)
        {
            string spName = "";
            if (companyID == 1)
                spName = Configuration.SPUpdatePTStatusToGpna;
            else
                spName = Configuration.SPUpdatePTStatusToGpeu;
            
            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPUpdatePTStatusToGPParam1, SqlDbType.VarChar, invoiceNumber, 21);
            
            return base.Update(cmd);
        }
    
        #endregion PickTicket
    }
}
