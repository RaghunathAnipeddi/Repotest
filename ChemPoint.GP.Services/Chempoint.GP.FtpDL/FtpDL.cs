using Chempoint.GP.Infrastructure.Logging;
using Chempoint.GP.Model.Interactions.FTP;
using ChemPoint.GP.DataContracts.FTP;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Chempoint.GP.FtpDL
{
    public class FtpDAL : IFtpDL
    {

        string strConnectionString = string.Empty, strFtpLogPath = string.Empty, strFtpLogFileName = string.Empty;

        public FtpDAL(string strConnString)
        {
            strConnectionString = strConnString;
            strFtpLogPath = ConfigurationManager.AppSettings["FtpLogPath"];
            strFtpLogFileName = ConfigurationManager.AppSettings["FtpLogFileName"];

        }

        public FTPResponse SaveFtpLog(FTPRequest objFTPRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            FTPResponse objFTPResponse = new FTPResponse();
            try
            {
                logMessage = objFTPRequest.logMessage;
                logMessage.AppendLine(DateTime.Now.ToString() + " - SaveFtpLog method in Chempoint.GP.FtpDL.FtpDAL is started");

                using (SqlConnection con = new SqlConnection(strConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(Chempoint.GP.Infrastructure.Config.Configuration.SPSaveFtpLog, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CompanyId", objFTPRequest.CompanyId);
                        cmd.Parameters.AddWithValue("@AppName", objFTPRequest.objFTPEntity.AppName);
                        cmd.Parameters.AddWithValue("@BatchFileName", objFTPRequest.objFTPEntity.BatchFileName);
                        cmd.Parameters.AddWithValue("@IsFileUpload", objFTPRequest.objFTPEntity.IsFileUpload);
                        cmd.Parameters.AddWithValue("@Status", objFTPRequest.objFTPEntity.Status);
                        
                        cmd.Parameters.AddWithValue("@FtpUploadPath", objFTPRequest.objFTPEntity.FtpUploadPath);
                        cmd.Parameters.AddWithValue("@FtpDownloadPath", objFTPRequest.objFTPEntity.FtpDownloadPath);
                        cmd.Parameters.AddWithValue("@ClientUploadPath", objFTPRequest.objFTPEntity.ClientUploadPath);
                        cmd.Parameters.AddWithValue("@ClientDownloadPath", objFTPRequest.objFTPEntity.ClientDownloadPath);
                        
                        cmd.Parameters.AddWithValue("@UserName", objFTPRequest.objFTPEntity.UserName);

                        cmd.ExecuteNonQuery();
                        objFTPResponse.ErrorStatus = "Success";
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Ftp log is successfully saved in GpTrace.common.FTPLog table");

                    }
                }
            }

            catch (Exception ex)
            {
                objFTPResponse.ErrorStatus = "Failure";
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - SaveFtpLog method in Chempoint.GP.FtpDL.FtpDAL ended.");
              
                objFTPResponse.logMessage = logMessage;
            }

            return objFTPResponse;
        }

        public FTPResponse FetchFtpDetails(FTPRequest objFTPRequest)
        {
            SqlConnection sqlConnection = null;
            SqlDataReader sqlDataReader = null;
            FTPEntity objFTPEntity = null;
            StringBuilder logMessage = new StringBuilder();
            FTPResponse objFTPResponse = new FTPResponse();
            bool bIsDataFetch = false;
            try
            {
                logMessage = objFTPRequest.logMessage;
                logMessage.AppendLine(DateTime.Now.ToString() + " - FetchFtpDetails method in Chempoint.GP.FtpDL.FtpDAL is started");

                using (sqlConnection = new SqlConnection(strConnectionString))
                {

                    using (SqlCommand sqlCommand = new SqlCommand(Chempoint.GP.Infrastructure.Config.Configuration.SPFetchftpconfigdetails, sqlConnection))
                    {
                        sqlConnection.Open();

                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.AddWithValue("@AppName", objFTPRequest.objFTPEntity.AppName);

                        sqlDataReader = sqlCommand.ExecuteReader();

                        while (sqlDataReader.Read())
                        {
                            objFTPEntity = new FTPEntity();
                            if (sqlDataReader["FtpAddress"] != null)
                                objFTPEntity.FtpAddress = Convert.ToString(sqlDataReader["FtpAddress"]);

                            if (sqlDataReader["FtpUserName"] != null)
                                objFTPEntity.FtpUserName = Convert.ToString(sqlDataReader["FtpUserName"]);

                            if (sqlDataReader["FtpPassword"] != null)
                                objFTPEntity.FtpPassword = Convert.ToString(sqlDataReader["FtpPassword"]);

                            if (sqlDataReader["ClientDownloadPath"] != null)
                                objFTPEntity.ClientDownloadPath = Convert.ToString(sqlDataReader["ClientDownloadPath"]);

                            if (sqlDataReader["ClientUploadPath"] != null)
                                objFTPEntity.ClientUploadPath = Convert.ToString(sqlDataReader["ClientUploadPath"]);

                            if (sqlDataReader["FtpDownloadPath"] != null)
                                objFTPEntity.FtpDownloadPath = Convert.ToString(sqlDataReader["FtpDownloadPath"]);

                            if (sqlDataReader["FtpUploadPath"] != null)
                                objFTPEntity.FtpUploadPath = Convert.ToString(sqlDataReader["FtpUploadPath"]);

                            objFTPResponse.objFTPEntity = objFTPEntity;
                            bIsDataFetch = true;
                        }
                        sqlDataReader.Close();
                        sqlConnection.Close();

                        if (bIsDataFetch)
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Successfully Fetched the data from stored procedure GpCustomizations.Common.Fetchftpconfigdetails");
                        else
                            logMessage.AppendLine(DateTime.Now.ToString() + " -  Fetching the data from stored procedure GpCustomizations.Common.Fetchftpconfigdetails is failed");

                    }
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - SaveFtpLog method in Chempoint.GP.FtpDL.FtpDAL ended.");
                
                objFTPResponse.logMessage = logMessage;
                }
            return objFTPResponse;
        }

        public FTPResponse GetUploadReportConfigDetails(FTPRequest objFTPRequest)
        {
            SqlConnection sqlConnection = null;
            SqlDataReader sqlDataReader = null;
            FTPEntity objFTPEntity = null;
            List<FTPEntity> objFTPEntityList = new List<FTPEntity>();
            StringBuilder logMessage = new StringBuilder();
            FTPResponse objFTPResponse = new FTPResponse();
            bool bIsDataFetch = false;
            try
            {
                logMessage = objFTPRequest.logMessage;
                logMessage.AppendLine(DateTime.Now.ToString() + " - GetUploadReportConfigDetails method in Chempoint.GP.FtpDL.FtpDAL is started");

                using (sqlConnection = new SqlConnection(strConnectionString))
                {

                    using (SqlCommand sqlCommand = new SqlCommand(Chempoint.GP.Infrastructure.Config.Configuration.SPGetUploadReportConfigDetails, sqlConnection))
                    {
                        sqlConnection.Open();

                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.AddWithValue("@AppName", objFTPRequest.objFTPEntity.AppName);

                        sqlDataReader = sqlCommand.ExecuteReader();

                        while (sqlDataReader.Read())
                        {
                            objFTPEntity = new FTPEntity();

                            if (sqlDataReader["AppId"] != null)
                                objFTPEntity.AppId = Convert.ToInt32(sqlDataReader["AppId"]);

                            if (sqlDataReader["Company"] != null)
                                objFTPEntity.Company = Convert.ToString(sqlDataReader["Company"]);

                            if (sqlDataReader["ScriptName"] != null)
                                objFTPEntity.ScriptName = Convert.ToString(sqlDataReader["ScriptName"]);

                            if (sqlDataReader["IsHeaderRequired"] != null)
                                objFTPEntity.IsHeaderRequired = Convert.ToBoolean(sqlDataReader["IsHeaderRequired"]);

                            if (sqlDataReader["IsFtp"] != null)
                                objFTPEntity.IsFtp = Convert.ToBoolean(sqlDataReader["IsFtp"]);

                            if (sqlDataReader["IsMail"] != null)
                                objFTPEntity.IsMail = Convert.ToBoolean(sqlDataReader["IsMail"]);

                            if (sqlDataReader["IsZip"] != null)
                                objFTPEntity.IsZip = Convert.ToBoolean(sqlDataReader["IsZip"]);

                            if (sqlDataReader["FormatType"] != null)
                                objFTPEntity.FormatType = Convert.ToString(sqlDataReader["FormatType"]);

                            if (sqlDataReader["FileName"] != null)
                                objFTPEntity.FileName = Convert.ToString(sqlDataReader["FileName"]);

                            if (sqlDataReader["EmailConfigID"] != null)
                                objFTPEntity.EmailConfigID = Convert.ToInt32(sqlDataReader["EmailConfigID"]);

                            if (sqlDataReader["EmailFrom"] != null)
                                objFTPEntity.EmailFrom = Convert.ToString(sqlDataReader["EmailFrom"]);

                            if (sqlDataReader["Signature"] != null)
                                objFTPEntity.Signature = Convert.ToString(sqlDataReader["Signature"]);

                            if (sqlDataReader["IsDataTableBodyRequired"] != null)
                                objFTPEntity.IsDataTableBodyRequired = Convert.ToBoolean(sqlDataReader["IsDataTableBodyRequired"]);

                            objFTPEntityList.Add(objFTPEntity);
                            bIsDataFetch = true;
                        }
                        sqlDataReader.Close();
                        sqlConnection.Close();

                        if (objFTPEntityList.Count > 0)
                            objFTPResponse.objFTPEntityList = objFTPEntityList;

                        if (bIsDataFetch)
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Successfully Fetched the data from stored procedure GpCustomizations.Common.GetUploadReportConfigDetails");
                        else
                            logMessage.AppendLine(DateTime.Now.ToString() + " -  No data is fetched from stored procedure GpCustomizations.Common.GetUploadReportConfigDetails");

                    }
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - GetUploadReportConfigDetails method in Chempoint.GP.FtpDL.FtpDAL ended.");
                objFTPResponse.logMessage = logMessage;
            }
            return objFTPResponse;
        }

        public FTPResponse GetResulSet(FTPRequest objFTPRequest)
        {
            SqlConnection sqlConnection = null;
            StringBuilder logMessage = new StringBuilder();
            FTPResponse objFTPResponse = new FTPResponse();
            bool bIsDataFetch = false;
            DataSet dtResultSet = null;
            try
            {
                logMessage = objFTPRequest.logMessage;
                logMessage.AppendLine(DateTime.Now.ToString() + " - GetResulSet method in Chempoint.GP.FtpDL.FtpDAL is started");

                using (sqlConnection = new SqlConnection(strConnectionString))
                {

                    using (SqlCommand sqlCommand = new SqlCommand(objFTPRequest.objFTPEntity.ScriptName, sqlConnection))
                    {
                        sqlConnection.Open();

                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        sqlCommand.CommandTimeout =0;
                        

                        using (SqlDataAdapter da = new SqlDataAdapter(sqlCommand))
                        {

                            dtResultSet = new DataSet();
                            da.Fill(dtResultSet);
                        }
                       

                        if (dtResultSet != null)
                        {
                            objFTPResponse.ResultSet = ConvertDataTableToString(dtResultSet.Tables[0]);
                            bIsDataFetch = true;
                        }

                        if (bIsDataFetch)
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Successfully Fetched the data from stored procedure " + objFTPRequest.objFTPEntity.ScriptName);
                        else
                            logMessage.AppendLine(DateTime.Now.ToString() + " -  Fetching the data from stored procedure " + objFTPRequest.objFTPEntity.ScriptName);

                    }
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - GetResulSet method in Chempoint.GP.FtpDL.FtpDAL ended.");
                objFTPResponse.logMessage = logMessage;
            }
            return objFTPResponse;
        }

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


