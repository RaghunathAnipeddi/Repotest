using Chempoint.GP.FtpBL;
using Chempoint.GP.Infrastructure.Config;
using Chempoint.GP.Infrastructure.DataAccessEngine.Extensions;
using Chempoint.GP.Infrastructure.Maps.Base;
using Chempoint.GP.Infrastructure.Maps.Purchase;
using Chempoint.GP.Model.Interactions.Email;
using Chempoint.GP.Model.Interactions.FTP;
using Chempoint.GP.Model.Interactions.PayableManagement;
using ChemPoint.GP.BusinessContracts.PM;
using ChemPoint.GP.DataContracts.PM;
using ChemPoint.GP.Email;
using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP;
using ChemPoint.GP.PMDL.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using Excel = Microsoft.Office.Interop.Excel;

namespace ChemPoint.GP.PMBL
{
    /// <PayableManagement>
    /// Project Name        :   payable mangement migration
    /// Affected Module     :   Purchase
    /// Affected Windows    :   Business Logic
    /// Developed on        :   April2017
    /// Developed by        :   Muthu
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **Apr2017       Mthangaraj      Initial Creation.
    /// </PayableManagement>
    public class PMBusiness : IPayableManagementBusiness
    {
        private bool _isCtsi = false;
        private bool _isApi = false;
        string userId = string.Empty;
        string currencyId = string.Empty;
        int companyId;
        string failErrorId = "1";
        string passErrorId = "0";
        StringBuilder mailContent = new StringBuilder();
        StringBuilder ctsimMailContent = new StringBuilder();
        StringBuilder apimMailContent = new StringBuilder();
        StringBuilder logMessage = new StringBuilder();
        string source = string.Empty;
        public int InvoiceType = 1;
        public DataTable fileDetailsTable = null;
        DataTable ctsiDS = null;
        DataTable apiDS = null;
        string date = DateTime.Now.ToString("yyyyMMddHHmmss").Trim();
        string zipName = string.Empty;

        //CTSIFileUploader
        public int FileCount;
        private const string APP_NAME = "CTSI";
        public static int isSucessfullyRetured;
        public PMBusiness()
        {

        }
        #region PayableManagement
        /// <summary>
        /// API to GP job
        /// process the flat file data
        /// </summary>
        /// <param name="requestObj">Request contains file name withpath etc.</param>
        /// <returns>Reponse object contains logging information</returns>
        public PayableManagementResponse ProcessApOutFile(PayableManagementRequest payableRequest)
        {

            PayableManagementResponse responseObj = null;
            IPayableManagementRepository payableManagementDataAccess = null;
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine(DateTime.Now + " ***** Started : Service method ProcessApOutFile. *****");
            //Declare po and non-po invoice records set
            DataSet poInvDS = new DataSet { Locale = CultureInfo.InvariantCulture };
            DataSet nonPoInvDS = new DataSet { Locale = CultureInfo.InvariantCulture };

            DataSet validateInvDS = new DataSet { Locale = CultureInfo.InvariantCulture };

            DataSet saveInvDS = new DataSet { Locale = CultureInfo.InvariantCulture };
            List<PayableLineEntity> payableEntityPOList = new List<PayableLineEntity>();

            List<PayableLineEntity> payableEntityDuplicateList = new List<PayableLineEntity>();

            try
            {
                responseObj = new PayableManagementResponse();
                //Assign Default header of the Dataset.- PO invoices
                if (poInvDS == null || poInvDS.Tables.Count == 0)
                    poInvDS = CreatePayableDataSet(payableRequest, payableRequest.POInputHeader.Trim());

                //Assign Default header of the Dataset.- Non-PO invoices
                if (nonPoInvDS == null || nonPoInvDS.Tables.Count == 0)
                    nonPoInvDS = CreatePayableDataSet(payableRequest, payableRequest.PMInputHeader.Trim());

                // Reading all files and push to a dataset
                foreach (string fileName in payableRequest.PayableManagementDetails.Files.OrderByDescending(s => s))
                {
                    if (Path.GetFileNameWithoutExtension(fileName).StartsWith(payableRequest.CHMPTFileNameForPO.Trim())
                       || Path.GetFileNameWithoutExtension(fileName).StartsWith(payableRequest.CPEURFileNameForPO.Trim()))
                    {
                        payableRequest.PayableManagementDetails.FileName = fileName;
                        payableRequest.PayableManagementDetails.InvoiceType = 2;
                        FetchPaymentDetailsFromTextFile(payableRequest,
                                      ref poInvDS);
                    }
                    else if (Path.GetFileNameWithoutExtension(fileName).StartsWith(payableRequest.CHMPTFileNameForNonPO.Trim())
                       || Path.GetFileNameWithoutExtension(fileName).StartsWith(payableRequest.CPEURFileNameForNonPO.Trim()))
                    {
                        payableRequest.PayableManagementDetails.FileName = fileName;
                        payableRequest.PayableManagementDetails.InvoiceType = 1;
                        FetchPaymentDetailsFromTextFile(payableRequest,
                                      ref nonPoInvDS);
                    }
                }

                // Update duplicate and distrubtion flag in NonPOInovice table
                if (nonPoInvDS != null && nonPoInvDS.Tables.Count > 0 && nonPoInvDS.Tables[0].Rows.Count > 0
                    && poInvDS != null && poInvDS.Tables.Count > 0 && poInvDS.Tables[0].Rows.Count > 0)
                {
                    List<string> docNumbersPO = (from nonPO in nonPoInvDS.Tables[0].AsEnumerable()
                                                 join po in poInvDS.Tables[0].AsEnumerable()
                                                   on nonPO.Field<string>("DOCUMENTROWID") equals po.Field<string>("DOCUMENTROWID")
                                                 where nonPO.Field<string>("DOCTYPE").ToString().Trim() == "CRM"
                                                 select po.Field<string>("DOCUMENTROWID")).Distinct().ToList();

                    if (docNumbersPO.Count() > 0)
                    {
                        foreach (DataRow row in poInvDS.Tables[0].Rows)
                        {
                            if (docNumbersPO.Contains(row["DOCUMENTROWID"].ToString()))
                                row["IsDuplicate"] = 1;
                        }
                    }

                    List<string> docNumbersNonPO = (from nonPO in nonPoInvDS.Tables[0].AsEnumerable()
                                                    join po in poInvDS.Tables[0].AsEnumerable()
                                                      on nonPO.Field<string>("DOCUMENTROWID") equals po.Field<string>("DOCUMENTROWID")
                                                    where nonPO.Field<string>("DOCTYPE").ToString().Trim() != "CRM"
                                                    select nonPO.Field<string>("DOCUMENTROWID")).Distinct().ToList();

                    if (docNumbersNonPO.Count() > 0)
                    {
                        List<string> manualDistributionRows = new List<string>();

                        foreach (string docNumbe in docNumbersNonPO)
                        {
                            List<string> hasDistributions = (from nonPO in nonPoInvDS.Tables[0].AsEnumerable()
                                                             where nonPO.Field<string>("DOCUMENTROWID") == docNumbe
                                                              && nonPO.Field<string>("GLTABLE1VALUE") != "PO-Auto"
                                                              && nonPO.Field<string>("GLTABLE1VALUE") != "VAT-Auto"
                                                             select nonPO.Field<string>("DOCUMENTROWID")).Distinct().ToList();
                            if (hasDistributions.Count > 0)
                                manualDistributionRows.Add(docNumbe);
                        }

                        foreach (DataRow row in nonPoInvDS.Tables[0].Rows)
                        {
                            if (manualDistributionRows.Count() > 0)
                            {
                                if (manualDistributionRows.Contains(row["DOCUMENTROWID"].ToString())
                                    && row["GLTABLE1VALUE"].ToString().Trim() != "PO-Auto"
                                    && row["GLTABLE1VALUE"].ToString().Trim() != "VAT-Auto")
                                    row["RequiredDistribution"] = 1;
                            }

                            if (docNumbersNonPO.Contains(row["DOCUMENTROWID"].ToString()))
                                row["IsDuplicate"] = 1;
                        }
                    }
                }

                //Save the file details to DB
                if ((nonPoInvDS != null && nonPoInvDS.Tables.Count > 0 && nonPoInvDS.Tables[0].Rows.Count > 0) ||
                    (poInvDS != null && poInvDS.Tables.Count > 0 && poInvDS.Tables[0].Rows.Count > 0))
                {
                    logMessage.AppendLine(DateTime.Now + " Started : Saving the file details to DB tables.");

                    payableManagementDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(payableRequest.ConnectionString);
                    saveInvDS = payableManagementDataAccess.SaveAPIDetailsToDB(payableRequest, poInvDS, nonPoInvDS);

                    logMessage.AppendLine(DateTime.Now + " End : File details are successfully saved to DB.");
                    if (saveInvDS != null && saveInvDS.Tables.Count > 0 && saveInvDS.Tables[0].Rows.Count > 0)
                    {
                        DirectoryInfo dir = null;
                        foreach (string fileName in payableRequest.PayableManagementDetails.Files.OrderByDescending(s => s))
                        {
                            dir = new DirectoryInfo(fileName);
                            //deletes the processed files from the folder.  
                            logMessage.AppendLine(DateTime.Now + " Deleting the processed file from the path");
                            logMessage.AppendLine(DateTime.Now + " Moving the file to the path " + fileName);
                            File.Move(fileName, payableRequest.ExtractedFilesArchiveFolder +
                                   DateTime.Now.ToString("yyyyMMddHHmmss") + dir.Name);
                            logMessage.AppendLine(DateTime.Now + " File deleted successfully");
                        }
                        logMessage.AppendLine(DateTime.Now + " Started : validating the file details to DB tables.");
                        validateInvDS = payableManagementDataAccess.ValidateAPIDetailsToDB(saveInvDS, payableRequest.companyId);
                        logMessage.AppendLine(DateTime.Now + " End : File details are successfully validated to DB.");
                    }
                    else
                    {
                        logMessage.AppendLine(DateTime.Now + " No records available to save to database/No files to process.");
                    }
                }


                //Process the invoices
                if (validateInvDS != null && validateInvDS.Tables.Count > 0 && validateInvDS.Tables[0].Rows.Count > 0)
                {

                    logMessage.AppendLine(DateTime.Now + " --------------------------------------------------------------");
                    logMessage.AppendLine(DateTime.Now + " Started : processing saved invoices.");

                    //Fetch without duplicate records
                    DataTable nonDuplicateDT = null;
                    if (validateInvDS.Tables[0] != null && validateInvDS.Tables[0].Rows.Count > 0)
                        nonDuplicateDT = validateInvDS.Tables[0]; // -1 values

                    //Fetch duplicate records
                    DataTable duplicateDT = null;
                    if (validateInvDS.Tables[1] != null && validateInvDS.Tables[1].Rows.Count > 0)
                        duplicateDT = validateInvDS.Tables[1]; // != -1 values


                    if (nonDuplicateDT != null && nonDuplicateDT.Rows.Count > 0)
                    {
                        logMessage.AppendLine(DateTime.Now + " nonDuplicateDT.Rows.Count :" + nonDuplicateDT.Rows.Count.ToString());
                        //Fetch PO invoices without duplicate
                        DataTable poInvoiceeDT = nonDuplicateDT.Select("FormTypeCode = 2").Count() > 0 ? nonDuplicateDT.Select("FormTypeCode = 2").CopyToDataTable() : null;
                        //Fetch Non-PO invoices without duplicate
                        DataTable nonPoInvoiceDT = nonDuplicateDT.Select("FormTypeCode <> 2").Count() > 0 ? nonDuplicateDT.Select("FormTypeCode <> 2").CopyToDataTable() : null;

                        // Executed for po-invoices. included duplicate records
                        if (poInvoiceeDT != null && poInvoiceeDT.Rows.Count > 0)
                        {

                            logMessage.AppendLine(DateTime.Now + " poInvoiceeDT.Rows.Count :" + poInvoiceeDT.Rows.Count.ToString());
                            //Assign name to process data table to service
                            poInvoiceeDT.TableName = "poDetails";

                            payableEntityPOList = GetAllEntities<PayableLineEntity, PayableManagementAPIMap>(poInvoiceeDT).ToList();

                            if (duplicateDT != null && duplicateDT.Rows.Count > 0)
                            {
                                payableEntityDuplicateList = GetAllEntities<PayableLineEntity, PayableManagementAPIMap>(duplicateDT).ToList();
                            }
                            else
                                payableEntityDuplicateList = null;
                            payableRequest.PoValidationList = payableEntityPOList;
                            payableRequest.DuplicationValidationList = payableEntityDuplicateList;

                            payableRequest.InvoiceType = 2;
                            payableRequest.SourceFormName = string.Empty;
                            logMessage.AppendLine(DateTime.Now + " poInvoiceeDT UploadPayableDetailsIntoGpForApi Method Call Start...");
                            responseObj = UploadPayableDetailsIntoGpForApi(payableRequest);
                            if (responseObj != null)
                            {
                                logMessage.AppendLine(responseObj.LogMessage.ToString().Trim());
                            }
                        }

                        // Executed for Non-po-invoices. included duplicate records
                        if (nonPoInvoiceDT != null && nonPoInvoiceDT.Rows.Count > 0)
                        {
                            nonPoInvoiceDT.TableName = "nonPoDetails";
                            // responseObj = PushToGPForApi(1, string.Empty, payableRequest.PayableManagementDetails.CompanyId, nonPoInvoiceDT, null);
                            logMessage.AppendLine(DateTime.Now + " nonPoInvoiceDT.Rows.Count :" + nonPoInvoiceDT.Rows.Count.ToString());

                            payableEntityPOList = GetAllEntities<PayableLineEntity, PayableManagementAPIMap>(nonPoInvoiceDT).ToList();
                            payableRequest.PoValidationList = payableEntityPOList;

                            payableRequest.InvoiceType = 1;
                            payableRequest.SourceFormName = string.Empty;
                            logMessage.AppendLine(DateTime.Now + " nonPoInvoiceDT UploadPayableDetailsIntoGpForApi Method Call Start...");
                            responseObj = UploadPayableDetailsIntoGpForApi(payableRequest);
                            if (responseObj != null)
                            {
                                logMessage.AppendLine(responseObj.LogMessage.ToString().Trim());
                            }
                        }
                    }
                }
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(ex.ToString());
                responseObj.Status = Response.Error;
            }
            finally
            {
                responseObj.LogMessage = logMessage.ToString().Trim();
            }
            return responseObj;
        }

        protected IEnumerable<TModel> GetAllEntities<TModel, TMap>(DataTable dt)
            where TModel : class, IModelBase, new()
            where TMap : class, IDataRowMap<TModel>, new()
        {
            var lst = new List<TModel>();

            foreach (DataRow dataRow in dt.Rows)
                lst.Add(dataRow.SelectRow(MapperFactory<TModel, TMap>.Mapper().Map));
            return lst;
        }


        #region private methods
        /// <summary>
        ///Method to move the create  the log file and for todays date and write to log file.
        /// </summary>
        /// <param name="message">log message</param>
        /// <param name="logFileName">Name of the log file</param>
        /// <param name="logFilePath">Path for the log file</param>
        public static void LogDetailsToFile(string message, string logFileName, string logFilePath)
        {
            try
            {
                logFileName = logFileName + DateTime.Now.ToString("MM_dd_yy") + ".log";
                if (!Directory.Exists(logFilePath))
                    Directory.CreateDirectory(logFilePath);
                logFilePath = logFilePath + logFileName;

                if (!File.Exists(logFilePath))
                    File.Create(logFilePath).Close();

                int count = 0;
                int maxRetryCount = 0;

                int.TryParse(Configuration.RetryCount, out maxRetryCount);

                if (maxRetryCount == 0)
                {
                    maxRetryCount = 3;
                }
                while (count < maxRetryCount)
                {
                    try
                    {
                        using (StreamWriter w = File.AppendText(logFilePath))
                        {
                            w.WriteLine(message);
                            w.Flush();
                            w.Close();
                            count = maxRetryCount;
                        }
                    }
                    catch
                    {
                        count++;
                    }
                }
            }
            catch
            {
                throw;
            }

        }

        /// <summary>
        /// Set default value for numberic columns
        /// </summary>
        /// <param name="paymentDT"></param>
        private static void SetDefaultValueForAPI(int invoiceType, DataSet paymentDS, string fileName)
        {
            // InvoiceType = 1  (Payable), InvoiceType = 2 (PO invoice)

            string[] formats = { "yyyyMMdd", "M/dd/yyyy h:mm:ss tt", "M/d/yyyy h:mm:ss tt", "MM/dd/yyyy h:mm:ss tt", "MM/d/yyyy h:mm:ss tt" };

            if (invoiceType == 1)
            {
                DataTable paymentDT = paymentDS.Tables[0];
                //change the column names

                //int count = 1;
                foreach (DataRow row in paymentDT.Rows)
                {
                    foreach (DataColumn col in paymentDT.Columns)
                    {
                        //test for null here
                        if ((string.IsNullOrEmpty(row[col].ToString())) && ((col.ColumnName == "DocumentAmount")
                                                            || (col.ColumnName == "ApprovedAmount")
                                                            || (col.ColumnName == "FRTAMNT")
                                                            || (col.ColumnName == "TAXAMNT")
                                                            || (col.ColumnName == "PRCHAMNT")
                                                            || (col.ColumnName == "TRDISAMT")
                                                            || (col.ColumnName == "MSCCHAMT")))
                        {
                            row[col] = 0;
                        }
                    }

                    if (row["TAXSCHEDULEDESC"].ToString().Length > 0)
                        row["TAXSCHEDULEID"] = row["TAXSCHEDULEDESC"].ToString().Substring(0, row["TAXSCHEDULEDESC"].ToString().IndexOf(":"));

                    // update the invoice type to 2 for INV and 3 for CRM
                    row["FormTypeCode"] = ConvertDocTypeValueByInvoiceType(invoiceType, row["DOCTYPE"].ToString().Trim()).ToString();

                    row["DOCDATE"] = DateTime.ParseExact(row["DOCDATE"].ToString(), formats,
                                                DateTimeFormatInfo.InvariantInfo,
                                                DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite);

                }
            }
            else //Po invoices
            {
                DataTable paymentDT = paymentDS.Tables[0];
                //change the column names
                foreach (DataRow row in paymentDT.Rows)
                {
                    foreach (DataColumn col in paymentDT.Columns)
                    {
                        //test for null here
                        if ((string.IsNullOrEmpty(row[col].ToString())) && ((col.ColumnName == "DocumentAmount")
                                                            || (col.ColumnName == "ApprovedAmount")
                                                            || (col.ColumnName == "FRTAMNT")
                                                            || (col.ColumnName == "TAXAMNT")
                                                            || (col.ColumnName == "MISCAMNT")
                                                            || (col.ColumnName == "FEDTAX")
                                                            || (col.ColumnName == "POAmount")
                                                            || (col.ColumnName == "QTYSHPPD")
                                                            || (col.ColumnName == "ADJUSTEDITEMUNITQTY")
                                                            || (col.ColumnName == "UNITCOST")
                                                            || (col.ColumnName == "EXTDCOST")
                                                            || (col.ColumnName == "ADJUSTEDITEMUNITPRICE")))
                        {
                            row[col] = 0;
                        }
                    }

                    if (row["TAXSCHEDULEDESC"].ToString().Length > 0)
                        row["TAXSCHEDULEID"] = row["TAXSCHEDULEDESC"].ToString().Substring(0, row["TAXSCHEDULEDESC"].ToString().IndexOf(":"));

                    // update the invoice type to 2
                    row["FormTypeCode"] = ConvertDocTypeValueByInvoiceType(invoiceType, "");

                    row["RECEIPTDATE"] = DateTime.ParseExact(row["RECEIPTDATE"].ToString(), formats,
                                                DateTimeFormatInfo.InvariantInfo,
                                                DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite);
                    row["SHIPPEDDATE"] = DateTime.ParseExact(row["SHIPPEDDATE"].ToString(), formats,
                                                DateTimeFormatInfo.InvariantInfo,
                                                DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite);
                    row["PODATEOPENED"] = DateTime.ParseExact(row["PODATEOPENED"].ToString(), formats,
                                                DateTimeFormatInfo.InvariantInfo,
                                                DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite);

                }
            }
        }

        #endregion private methods

        #region API EMEA

        public static DataSet CreatePayableDataSet(PayableManagementRequest requestObj, string fileHeader)
        {
            try
            {
                // string file = requestObj.FileName.Trim();

                // reads all the lines from text file
                DataSet payableDS = new DataSet();
                DataTable payableDT = new DataTable();
                // adds columns to dataset.
                string header = fileHeader.Trim();
                foreach (string col in header.Split('|'))
                {
                    payableDT.Columns.Add(col.ToString());
                }
                payableDT.Columns.Add("FileName", typeof(string));
                payableDT.Columns.Add("FormTypeCode", typeof(int));
                payableDT.Columns.Add("Notes");
                payableDT.Columns.Add("UserId");
                payableDT.Columns.Add("IsDuplicate");
                payableDT.Columns.Add("RequiredDistribution");

                payableDS.Tables.Add(payableDT);
                return payableDS;
            }
            catch
            {
                throw;
            }
        }

        public static void FetchPaymentDetailsFromTextFile(PayableManagementRequest requestObj, ref DataSet payableDS)
        {
            try
            {
                // reads all the lines from text file
                string[] lines = File.ReadAllLines(requestObj.PayableManagementDetails.FileName.Trim());

                // add lines to the dataset
                foreach (string line in lines)
                {
                    DataRow payableDR = payableDS.Tables[0].NewRow();
                    int i = 0;

                    foreach (string col in line.Split('|'))
                    {
                        if (i < payableDS.Tables[0].Columns.Count)
                            payableDR[i] = col.ToString();
                        i++;
                    }
                    payableDR["FileName"] = requestObj.PayableManagementDetails.FileName.Substring(requestObj.PayableManagementDetails.FileName.LastIndexOf("\\") + 1);
                    payableDR["Notes"] = line;
                    payableDR["UserId"] = requestObj.PayableManagementDetails.UserId.ToString();
                    payableDR["IsDuplicate"] = "0";
                    payableDR["RequiredDistribution"] = "0";

                    payableDS.Tables[0].Rows.Add(payableDR);
                }

                if (requestObj.PayableManagementDetails.Source == Configuration.STR_APISOURCE)
                {
                    SetDefaultValueForAPI(requestObj.PayableManagementDetails.InvoiceType, payableDS, requestObj.PayableManagementDetails.FileName);
                }
            }
            catch
            {
                throw;
            }
        }


        #endregion API EMEA

        #endregion PayableManagement

        #region PayableService

        #region ExpenseVisor




        /// <summary>
        /// Push the invoice into GP
        /// </summary>
        /// <param name="paymentView"></param>
        /// <param name="distributionDt"></param>
        /// <param name="nextInvoiceNumber"></param>
        /// <param name="notes"></param>
        /// <param name="isRetryNeeded"></param>
        /// <returns></returns>
        private void PushToGP(PayableManagementRequest request, DataTable paymentView, string notes, int companyId, DataTable taxDetails, string source, DataTable distributedDetails, string ConnectionString, string EconnectConnectionString, ref StringBuilder logMessage)
        {
            DataTable lineTaxDetailsTable = null;
            DataTable lineDistributedDetails = null;
            string inputXml = string.Empty;
            string errorMsg = string.Empty;
            string errorCode = string.Empty;
            string nextInvoiceNumber = string.Empty;

            int doctype = 1;
            string curncyId = "";
            double ten99Amnt = 0.0;
            int isRequiredDistribution = 0;
            IPayableManagementRepository payableDataAccess = null;


            try
            {
                if (_isCtsi && !_isApi)
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " Processing CTSIId Is: " + paymentView.Rows[0]["CTSIId"].ToString());
                    if (companyId == Convert.ToInt16(Configuration.EUCompanyId))
                    {
                        lineTaxDetailsTable = new DataTable("@LineDetails");
                        lineTaxDetailsTable.Columns.Add("TaxScheduleId", typeof(string));
                        lineTaxDetailsTable.Columns.Add("TaxDetailId", typeof(string));
                        lineTaxDetailsTable.Columns.Add("TaxPercentage", typeof(decimal));
                        lineTaxDetailsTable.Columns.Add("TaxAmount", typeof(decimal));
                        bool isLocalCharge = false;
                        bool isZeroCharge = false;
                        bool isReverseCharge = false;
                        foreach (DataRow data in paymentView.Rows)
                        {
                            if (Convert.ToDecimal(data["BaseLocalCharge"].ToString()) > 0 && !isLocalCharge)
                            {
                                foreach (DataRow r in taxDetails.Rows)
                                {
                                    if (r["BaseCharge"].ToString().Trim() == Configuration.LocalCharge.ToString().Trim())
                                    {
                                        var row = lineTaxDetailsTable.NewRow();
                                        row["TaxScheduleId"] = r["TaxScheduleId"].ToString().Trim();
                                        row["TaxDetailId"] = r["TaxDetailId"].ToString().Trim();
                                        row["TaxPercentage"] = r["TaxPercentage"].ToString().Trim();
                                        row["TaxAmount"] = Convert.ToDecimal(data["TaxAmount"]);

                                        lineTaxDetailsTable.Rows.Add(row);
                                    }
                                }
                                isLocalCharge = true;
                            }

                            if (Convert.ToDecimal(data["BaseZeroRatedCharge"].ToString()) > 0 && !isZeroCharge)
                            {
                                foreach (DataRow r in taxDetails.Rows)
                                {
                                    if (r["BaseCharge"].ToString().Trim() == Configuration.ZeroCharge.ToString().Trim())
                                    {
                                        var row = lineTaxDetailsTable.NewRow();
                                        row["TaxScheduleId"] = r["TaxScheduleId"].ToString().Trim();
                                        row["TaxDetailId"] = r["TaxDetailId"].ToString().Trim();
                                        row["TaxPercentage"] = r["TaxPercentage"].ToString().Trim();
                                        row["TaxAmount"] = 0;

                                        lineTaxDetailsTable.Rows.Add(row);
                                    }
                                }
                                isZeroCharge = true;
                            }
                            if (Convert.ToDecimal(data["BaseReverseCharge"].ToString()) > 0 && !isReverseCharge)
                            {
                                foreach (DataRow r in taxDetails.Rows)
                                {
                                    if (r["BaseCharge"].ToString().Trim() == Configuration.ReverseCharge.ToString().Trim())
                                    {
                                        var row = lineTaxDetailsTable.NewRow();
                                        row["TaxScheduleId"] = r["TaxScheduleId"].ToString().Trim();
                                        row["TaxDetailId"] = r["TaxDetailId"].ToString().Trim();
                                        row["TaxDetailId"] = r["TaxDetailId"].ToString().Trim();
                                        //row["TaxAmount"] = Convert.ToDecimal(data["TotalApprovedDocumentAmount"]) * (Convert.ToDecimal(r["TaxPercentage"]) / 100);//(purchase amount * column percentage of TX00201/100) * Rounding currency decimal place 
                                        //Task Id 7237- Feb 2015 - Fix for CTSI Issue-Lavanya praveen
                                        row["TaxAmount"] = Math.Round(Convert.ToDecimal(data["TotalApprovedDocumentAmount"]) * (Convert.ToDecimal(r["TaxPercentage"]) / 100), 2);
                                        lineTaxDetailsTable.Rows.Add(row);
                                    }
                                }
                                isReverseCharge = true;
                            }
                        }
                    }
                }
                else if (!_isCtsi && _isApi) //APi
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " Processing ApiInvoiceId Is: " + paymentView.Rows[0]["DocumentRowId"].ToString());

                    if (companyId == Convert.ToInt16(Configuration.EUCompanyId))
                    {
                        int lineSeqNumber = 16384;
                        bool isNonPOTaxEnabled = false;
                        lineTaxDetailsTable = new DataTable("@LineDetails");
                        lineTaxDetailsTable.Columns.Add("TaxScheduleId", typeof(string));
                        lineTaxDetailsTable.Columns.Add("TaxDetailId", typeof(string));
                        lineTaxDetailsTable.Columns.Add("TaxPercentage", typeof(decimal));
                        lineTaxDetailsTable.Columns.Add("TaxAmount", typeof(decimal));
                        lineTaxDetailsTable.Columns.Add("SalesTaxAmount", typeof(decimal));
                        lineTaxDetailsTable.Columns.Add("ReceiptLineNumber", typeof(int));
                        lineTaxDetailsTable.Columns.Add("TaxablePurchases", typeof(decimal));
                        lineTaxDetailsTable.Columns.Add("TotalPurchases", typeof(decimal));
                        lineTaxDetailsTable.Columns.Add("GlAccountNumber", typeof(string));
                        lineTaxDetailsTable.Columns.Add("GlAccountDescription", typeof(string));

                        foreach (DataRow data in paymentView.Rows)
                        {
                            if (taxDetails != null && taxDetails.Rows.Count > 0 && !isNonPOTaxEnabled)
                            {
                                var rowsTax = (from p in taxDetails.AsEnumerable()
                                               where p.Field<string>("TaxScheduleId").ToString().Trim() == data["TaxScheduleId"].ToString().Trim()
                                               select new
                                               {
                                                   TaxScheduleId = p.Field<string>("TaxScheduleId"),
                                                   TaxDetailId = p.Field<string>("TaxDetailId"),
                                                   TaxPercentage = p.Field<decimal>("TaxPercentage"),
                                                   GlAccountNumber = p.Field<string>("GlAccountNumber"),
                                                   GlAccountDescription = p.Field<string>("GlAccountDescription"),
                                               }).Distinct().OrderByDescending(x => x.TaxPercentage);

                                if (rowsTax.Count() > 0)
                                {
                                    bool isLocalCharge = false;
                                    decimal TaxAmount = Convert.ToDecimal(data["SalesTaxAmount"]);
                                    decimal ApprovedAmount = 0;

                                    if (InvoiceType == 1)
                                    {
                                        ApprovedAmount = Convert.ToDecimal(Math.Round(Convert.ToDecimal(data["ApprovedAmount"]), 5));
                                    }
                                    else
                                    {
                                        ApprovedAmount = Convert.ToDecimal(
                                                                     Math.Round(Convert.ToDecimal(data["ItemUnitQty"]) * Convert.ToDecimal(data["ItemUnitPrice"]), 5));
                                    }
                                    foreach (var item in rowsTax)
                                    {
                                        var row = lineTaxDetailsTable.NewRow();
                                        row["TaxScheduleId"] = item.TaxScheduleId;
                                        row["TaxDetailId"] = item.TaxDetailId;
                                        row["TaxPercentage"] = item.TaxPercentage;
                                        if (TaxAmount == 0)
                                            row["TaxAmount"] = Convert.ToDecimal((ApprovedAmount * item.TaxPercentage / 100).ToString("#.00000"));//(purchase amount * column percentage of TX00201/100) * Rounding currency decimal place 
                                        else
                                            row["TaxAmount"] = isLocalCharge == false ? TaxAmount : 0;

                                        row["SalesTaxAmount"] = TaxAmount;
                                        row["ReceiptLineNumber"] = lineSeqNumber;
                                        row["TaxablePurchases"] = ApprovedAmount;
                                        row["TotalPurchases"] = ApprovedAmount;
                                        row["GlAccountNumber"] = item.GlAccountNumber;
                                        row["GlAccountDescription"] = item.GlAccountDescription;
                                        lineTaxDetailsTable.Rows.Add(row);
                                        isLocalCharge = true;

                                    }
                                }
                                if (InvoiceType == 1)
                                    isNonPOTaxEnabled = true;
                            }
                            lineSeqNumber = lineSeqNumber + 16384;
                        }
                    }

                    if (InvoiceType == 1)
                    {
                        if (Convert.ToInt32(paymentView.Rows[0]["FormTypeCode"]) == 1)
                        {
                            doctype = 1;  // DocTypeName: INV for non-po invoice
                        }
                        else
                        {
                            doctype = 3;  // DocTypeName: CRM for non-po invoice- credit memo
                        }
                    }
                    else
                    {
                        doctype = 2;     // DocTypeName: INV for po invoice
                    }

                    paymentView = ConvertToTableForAPIeConnect(paymentView, notes, InvoiceType);

                    string vendorId = paymentView.Rows[0]["VENDORID"].ToString().Trim();
                    curncyId = paymentView.Rows[0]["CURNCYID"].ToString().Trim();

                    if (InvoiceType == 2)
                    {
                        if (distributedDetails != null && distributedDetails.Rows.Count > 0)
                        {
                            distributedDetails = ConvertToTableForAPIeConnect(distributedDetails, notes, 1);
                        }
                    }
                    else
                    {
                        distributedDetails = paymentView.Copy();
                    }

                    string payAcntNumb = "";
                    string payAcntDesc = "";

                    if (distributedDetails != null && distributedDetails.Rows.Count > 0)
                    {
                        for (int i = 0; i < distributedDetails.Rows.Count; i++)
                        {
                            if (distributedDetails.Rows[i]["GLIndex"].ToString().ToUpper() == "0")
                                isRequiredDistribution = 1;

                            if (distributedDetails.Rows[i]["GLIndex"].ToString().ToUpper() != "0")
                            {
                                DataSet actDS = new DataSet();

                                payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(ConnectionString);
                                actDS = payableDataAccess.FetchAccountDetails(Convert.ToInt32(distributedDetails.Rows[i]["GLIndex"]),
                                    distributedDetails.Rows[i]["VENDORID"].ToString(), companyId);


                                if (actDS.Tables.Count == 2 && actDS.Tables[0].Rows.Count > 0 && actDS.Tables[1].Rows.Count > 0)
                                {
                                    distributedDetails.Rows[i]["ACTNUMB"] = actDS.Tables[0].Rows[0]["ACTNUMB"];
                                    distributedDetails.Rows[i]["ACTDESCR"] = actDS.Tables[0].Rows[0]["ACTDESCR"];
                                    payAcntDesc = actDS.Tables[1].Rows[0]["ACTDESCR"].ToString();
                                    payAcntNumb = actDS.Tables[1].Rows[0]["ACTNUMB"].ToString();
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now.ToString() + " One or more lines in the document has an inactive account");
                                    break;
                                }
                            }
                        }

                        lineDistributedDetails = new DataTable("@DistributedDetails");
                        lineDistributedDetails.Columns.Add("VENDORID", typeof(string));
                        lineDistributedDetails.Columns.Add("DISTTYPE", typeof(int));
                        lineDistributedDetails.Columns.Add("CRDTAMNT", typeof(decimal));
                        lineDistributedDetails.Columns.Add("DEBITAMT", typeof(decimal));
                        lineDistributedDetails.Columns.Add("ACTNUMST", typeof(string));
                        lineDistributedDetails.Columns.Add("ACTDESCR", typeof(string));

                        if (payAcntNumb != "" && payAcntDesc != "")
                        {

                            payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(ConnectionString);
                            DataTable distributionDT = payableDataAccess.GetNonPODistributionValues(distributedDetails,
                                                                                             payAcntNumb,
                                                                                             payAcntDesc,
                                                                                             companyId,
                                                                                             out ten99Amnt);


                            if (distributionDT != null && distributionDT.Rows.Count > 0)
                            {
                                foreach (DataRow data in distributionDT.Rows)
                                {
                                    if (companyId == Convert.ToInt16(Configuration.NACompanyId) ||
                                        (companyId == Convert.ToInt16(Configuration.EUCompanyId)))
                                    {
                                        var row = lineDistributedDetails.NewRow();
                                        row["VENDORID"] = data["VENDORID"].ToString().Trim();
                                        row["DISTTYPE"] = Convert.ToInt16(data["DISTTYPE"]);
                                        row["CRDTAMNT"] = Convert.ToDecimal(data["CRDTAMNT"]);
                                        row["DEBITAMT"] = Convert.ToDecimal(data["DEBITAMT"]);
                                        row["ACTNUMST"] = data["ACTNUMST"].ToString().Trim();
                                        row["ACTDESCR"] = data["ACTDESCR"].ToString().Trim();
                                        lineDistributedDetails.Rows.Add(row);
                                    }
                                }
                            }
                        }
                        if (companyId == Convert.ToInt16(Configuration.EUCompanyId) &&
                                    lineTaxDetailsTable != null && lineTaxDetailsTable.Rows.Count > 0)
                        {
                            foreach (DataRow data in lineTaxDetailsTable.Rows)
                            {
                                if (Convert.ToDecimal(data["TaxAmount"]) != 0 && Convert.ToDecimal(data["SalesTaxAmount"]) == 0)
                                {
                                    var row = lineDistributedDetails.NewRow();
                                    row["VENDORID"] = vendorId;
                                    row["DISTTYPE"] = 10;

                                    if (Convert.ToDecimal(data["TaxAmount"]) >= 0)
                                    {
                                        row["CRDTAMNT"] = 0;
                                        row["DEBITAMT"] = Convert.ToDecimal(data["TaxAmount"]);
                                    }
                                    else
                                    {
                                        row["CRDTAMNT"] = Math.Abs(Convert.ToDecimal(data["TaxAmount"]));
                                        row["DEBITAMT"] = 0;
                                    }
                                    row["ACTNUMST"] = data["GlAccountNumber"].ToString().Trim();
                                    row["ACTDESCR"] = data["GlAccountDescription"].ToString().Trim();
                                    lineDistributedDetails.Rows.Add(row);
                                }
                            }
                        }
                    }
                }
                if (_isCtsi)
                    inputXml = ParseTableToXmlStringForCtsi(paymentView, lineTaxDetailsTable);
                if (_isApi)
                    inputXml = ParseTableToXmlStringForApi(paymentView, lineDistributedDetails, lineTaxDetailsTable);

                logMessage.AppendLine(DateTime.Now.ToString() + " Input XML: " + inputXml);
                if (inputXml != string.Empty)
                {
                    // Gets the next invoice number.
                    logMessage.AppendLine(DateTime.Now.ToString() + " Fetching the nextinvoicenumber.");

                    if (_isApi)
                    {
                        int doctypecrction;
                        if (InvoiceType == 1)
                            doctypecrction = 2;
                        else
                            doctypecrction = 1;

                        payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(ConnectionString);
                        nextInvoiceNumber = payableDataAccess.GetNextInvoiceNumberForApi(companyId, doctypecrction);
                    }
                    else
                    {
                        payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(ConnectionString);
                        nextInvoiceNumber = payableDataAccess.GetNextInvoiceNumber(companyId);
                    }

                    logMessage.AppendLine(DateTime.Now.ToString() + " NextInvoiceNumber is : " + nextInvoiceNumber);
                    //Transform XML into Econnect XML
                    string eConnectXml = Transform(inputXml, nextInvoiceNumber.Trim(), notes, companyId, curncyId, ten99Amnt, isRequiredDistribution);
                    if (eConnectXml != "")
                    {
                        logMessage.AppendLine(DateTime.Now.ToString().ToString() + " Econnect Xml: " + eConnectXml);
                        logMessage.AppendLine(DateTime.Now.ToString() + " Calling the Econnect WCF service.");

                        bool isPushed = CallEconnectService(eConnectXml, companyId, EconnectConnectionString);
                        //logMessage.AppendLine(DateTime.Now.ToString() + " Econnect WCF service called successfully.");

                        if (isPushed)
                        {
                            errorMsg = "Payment " + nextInvoiceNumber.Trim() + " has been pushed successfully.";
                            LogMailContent(paymentView, errorMsg, passErrorId, nextInvoiceNumber.Trim());
                            logMessage.AppendLine(DateTime.Now.ToString() + " Status: " + errorMsg);
                            logMessage.AppendLine(DateTime.Now.ToString() + "----------------------------------------------------.");
                        }
                        logMessage.AppendLine(DateTime.Now.ToString() + " Updating the status in the table.");
                        UpdateStatusOfPayments(paymentView, ConnectionString,request);
                    }
                    else
                    {
                        logMessage.AppendLine(DateTime.Now.ToString() + " Econnect XML is not generated.");
                        LogMailContent(paymentView, "Econnect XML is not generated", failErrorId, string.Empty);
                    }
                }
                else
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " Input XML is not generated.");
                    LogMailContent(paymentView, "Input XML is not generated", failErrorId, string.Empty);
                }
            }
            catch (FaultException<Microsoft.Dynamics.GP.eConnect.eConnectFault> econEx)
            {
                logMessage.AppendLine("Error Detail:  econnect fault:" + econEx.Detail.Message);
                if (econEx.Detail.Message.Contains("Error Number = "))
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " Status : Error while creating payment into GP");

                    errorCode = econEx.Detail.Message.Substring(econEx.Detail.Message.IndexOf("Error Number = ") + 15,
                     (econEx.Detail.Message.IndexOf(" ", econEx.Detail.Message.IndexOf("Error Number = ") + 15) -
                     (econEx.Detail.Message.IndexOf("Error Number = ") + 15)));

                    logMessage.AppendLine(DateTime.Now.ToString() + "Econnect Error Code: " + errorCode);
                }

                LogMailContent(paymentView, econEx.Detail.Message.Substring(econEx.Detail.Message.IndexOf("Error Description = "), 254), errorCode.ToString(), "");
                logMessage.AppendLine(DateTime.Now.ToString() + "-----------------------------------------------------.");

                logMessage.AppendLine(DateTime.Now.ToString() + " Updating the status in the table.");
                UpdateStatusOfPayments(paymentView, ConnectionString,request);
            }
            catch (FaultException<Microsoft.Dynamics.GP.eConnect.eConnectSqlFault> econSqlEx)
            {
                logMessage.AppendLine("Error Detail: eConnectSqlFault" + econSqlEx.Detail.Message);
                if (econSqlEx.Detail.Message.Contains("Error Number = "))
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " Status : Error while creating payment into GP");

                    errorCode = econSqlEx.Detail.Message.Substring(econSqlEx.Detail.Message.IndexOf("Error Number = ") + 15,
                     (econSqlEx.Detail.Message.IndexOf(" ", econSqlEx.Detail.Message.IndexOf("Error Number = ") + 15) -
                     (econSqlEx.Detail.Message.IndexOf("Error Number = ") + 15)));

                    logMessage.AppendLine(DateTime.Now.ToString() + "SQL Error Code: " + errorCode);
                }
                //LogMailContent(paymentView, econSqlEx.Detail.Message, errorCode.ToString(),string.Empty);
                LogMailContent(paymentView, econSqlEx.Detail.Message.Substring(econSqlEx.Detail.Message.IndexOf("Error Description = "), 254), errorCode.ToString(), "");
                logMessage.AppendLine(DateTime.Now.ToString() + "-----------------------------------------------------.");

                logMessage.AppendLine(DateTime.Now.ToString() + " Updating the status in the table.");
                UpdateStatusOfPayments(paymentView, ConnectionString,request);
            }
            catch (FaultException ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + ex.ToString());
                LogExceptionDetails(ex.Message.Substring(0, 254), paymentView, ConnectionString, request);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + ex.ToString());
                LogExceptionDetails(ex.Message, paymentView, ConnectionString, request);
            }
        }

        /// <summary>
        /// Logging the exception details
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="paymentView"></param>
        private void LogExceptionDetails(string exception, DataTable paymentView, string ConnectionString,PayableManagementRequest request)
        {
            try
            {
                LogMailContent(paymentView, exception, failErrorId, string.Empty);
                logMessage.AppendLine(DateTime.Now.ToString() + "-----------------------------------------------------.");
                logMessage.AppendLine(DateTime.Now.ToString() + " Updating the status in the table.");
                UpdateStatusOfPayments(paymentView, ConnectionString,request);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// update the payment status in table
        /// </summary>
        /// <param name="paymentView"></param>
        private void UpdateStatusOfPayments(DataTable paymentView, string ConnectionString,PayableManagementRequest request)
        {
            IPayableManagementRepository payableDataAccess = null;
            try
            {
                if (_isCtsi)
                {
                    //if (String.IsNullOrEmpty(source))
                    //{
                    //    paymentView.Columns.Remove("OtherDuplicates");
                    //    paymentView.Columns.Remove("DocumentType");
                    //    paymentView.Columns.Remove("CtsiStatus");

                    //}
                    paymentView.Columns.Remove("OtherDuplicates");
                    paymentView.Columns.Remove("DocumentType");
                    paymentView.Columns.Remove("CtsiStatus");
                    paymentView.Columns.Remove("CurrencyDecimalPlaces");
                    paymentView.Columns.Remove("DebitDistributionType");
                    paymentView.Columns.Remove("CreditAmount");
                    paymentView.Columns.Remove("CreditAccountNumber");
                    paymentView.Columns.Remove("Notes");
                    if (string.IsNullOrEmpty(request.UserId))
                        paymentView.Columns.Remove("UserId");
                    paymentView.Columns.Remove("OriginialCTSIInvoiceId");
                    paymentView.Columns.Remove("CTSIFileId");
                    paymentView.Columns.Remove("DocumentDate");
                    paymentView.Columns.Remove("TotalApprovedDocumentAmount");
                    paymentView.Columns.Remove("ApprovedAmount");
                    paymentView.Columns.Remove("OverCharge");
                    paymentView.Columns.Remove("FreightAmount");
                    paymentView.Columns.Remove("AirWayInvoiceNumber");
                    paymentView.Columns.Remove("CptReference");
                    paymentView.Columns.Remove("CurrencyCode");
                    paymentView.Columns.Remove("GlAccount");
                    paymentView.Columns.Remove("TaxAmount");
                    paymentView.Columns.Remove("MiscellaneousAmount");
                    paymentView.Columns.Remove("TradeDiscounts");
                    paymentView.Columns.Remove("GLAccountDescription");
                    paymentView.Columns.Remove("PurchaseAmount");
                    paymentView.Columns.Remove("ValidDocumentNumber");

                    if (companyId == Convert.ToInt16(Configuration.EUCompanyId))
                    {
                        paymentView.Columns.Remove("BaseLocalCharge");
                        paymentView.Columns.Remove("BaseZeroRatedCharge");
                        paymentView.Columns.Remove("BaseReverseCharge");
                        if (string.IsNullOrEmpty(source) == true)
                        {
                            paymentView.Columns.Remove("TaxScheduleId");
                            paymentView.Columns.Remove("BaseChargeType");
                        }
                    }
                    //arrange the columns in the order to support table type
                    paymentView.Columns["VoucherNumber"].SetOrdinal(3);

                    //insert the document number and status in log table   
                    logMessage.AppendLine(DateTime.Now.ToString() + " ErrorId to update the payment status is ." + paymentView.Rows[0]["StatusId"].ToString());
                    payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(ConnectionString);
                    payableDataAccess.UpdateCtsiPaymentStatus(paymentView, userId, companyId);

                    logMessage.AppendLine(DateTime.Now.ToString() + " Updated the payment status successfully in the table.");
                    logMessage.AppendLine(DateTime.Now.ToString() + " -----------------------------------------------------.");
                    paymentView = null;
                }
                else if (_isApi)
                {
                    System.Data.DataView view = new System.Data.DataView(paymentView);
                    paymentView = view.ToTable("UpdateDetails", false, "DocumentRowId", "DocumentNumber", "VendorNumber", "VoucherNumber", "StatusId", "ErrorDescription");

                    paymentView.Columns["DocumentRowId"].SetOrdinal(0);
                    paymentView.Columns["DocumentNumber"].SetOrdinal(1);
                    paymentView.Columns["VendorNumber"].SetOrdinal(2);
                    paymentView.Columns["VoucherNumber"].SetOrdinal(3);
                    paymentView.Columns["StatusId"].SetOrdinal(4);
                    paymentView.Columns["ErrorDescription"].SetOrdinal(5);

                    //insert the document number and status in log table   
                    logMessage.AppendLine(DateTime.Now.ToString() + " ErrorId to update the payment status is ." + paymentView.Rows[0]["StatusId"].ToString());

                    payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(ConnectionString);
                    payableDataAccess.UpdateApiPaymentStatus(paymentView, userId, companyId, InvoiceType);
                    logMessage.AppendLine(DateTime.Now.ToString() + " Updated the payment status successfully in the table.");
                    logMessage.AppendLine(DateTime.Now.ToString() + " -----------------------------------------------------.");
                    paymentView = null;
                }

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Log the mail content
        /// </summary>
        /// <param name="documentRowId"></param>
        /// <param name="documentNumber"></param>
        /// <param name="vendorId"></param>
        /// <param name="errorMessage"></param>
        private void LogMailContent(DataTable paymentDv, string errorMessage, string errorId, string vouchernumber)
        {
            try
            {
                paymentDv.Columns.Add("VoucherNumber", typeof(string));
                if (_isCtsi)
                {
                    logMessage.AppendLine(DateTime.Now + " Updating the eror details in the datatable with the errorid " + errorId);
                    foreach (DataRow row in paymentDv.Rows)
                    {
                        row["StatusId"] = errorId;
                        row["VoucherNumber"] = string.IsNullOrEmpty(vouchernumber) ? string.Empty : vouchernumber.Trim();
                        logMessage.AppendLine(DateTime.Now + " updated voucher number :  " + vouchernumber.Trim());

                    }
                    //update the currently processed records status in the main table
                    var record = ctsiDS.AsEnumerable().Where(o => Convert.ToString(o["CTSIId"]) == paymentDv.Rows[0]["CTSIId"].ToString()).First();
                    record["StatusId"] = errorId;

                    //append the error details only for failed records
                    if (errorId != passErrorId)
                    {
                        ctsimMailContent.AppendLine("<tr><td>" + paymentDv.Rows[0]["CTSIId"].ToString() +
                                                         "</td><td>" + paymentDv.Rows[0]["ValidDocumentNumber"].ToString() +
                                                         "</td><td>" + paymentDv.Rows[0]["VendorId"].ToString() +
                                                         "</td><td>" + errorMessage + "</td></tr>");
                    }
                }
                if (_isApi)
                {
                    paymentDv.Columns.Add("StatusId", typeof(int));
                    paymentDv.Columns.Add("ErrorDescription", typeof(string), "");
                    paymentDv.Columns["VENDORID"].ColumnName = "VendorNumber";
                    if (InvoiceType == 1)
                    {
                        paymentDv.Columns["VCHNUMWK"].ColumnName = "DocumentNumber";
                    }

                    logMessage.AppendLine(DateTime.Now + " Updating the eror details in the datatable with the errorid " + errorId);
                    foreach (DataRow row in paymentDv.Rows)
                    {
                        row["StatusId"] = errorId;
                        row["VoucherNumber"] = string.IsNullOrEmpty(vouchernumber) ? string.Empty : vouchernumber.Trim();
                        logMessage.AppendLine(DateTime.Now + " updated voucher number :  " + vouchernumber.Trim());
                        row["ErrorDescription"] = errorMessage;
                    }

                    //update the currently processed records status in the main table 
                    var record = apiDS.AsEnumerable().Where(o => Convert.ToString(o["DocumentRowId"]) == paymentDv.Rows[0]["DocumentRowId"].ToString());
                    if (record != null && record.Count() > 0)
                    {
                        foreach (DataRow rowID in record)
                        {
                            //rowID["ErrorDescription"] = errorMessage; 
                            rowID["StatusId"] = errorId;
                        }
                    }

                    //append the error details only for failed records
                    if (errorId != passErrorId)
                    {
                        apimMailContent.AppendLine("<tr><td>" + paymentDv.Rows[0]["DocumentRowId"].ToString() +
                                                         "</td><td>" + paymentDv.Rows[0]["DocumentNumber"].ToString() +
                                                         "</td><td>" + paymentDv.Rows[0]["VendorNumber"].ToString() +
                                                         "</td><td>" + errorMessage + "</td></tr>");
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Call econnect method
        /// </summary>
        /// <param name="eConnectXml"></param>
        /// <param name="isRetryNeeded"></param>
        /// <returns></returns>
        private bool CallEconnectService(string eConnectXml, int companyId, string EconnectConnectionString)
        {
            bool result = false;

            try
            {
                Microsoft.Dynamics.GP.eConnect.eConnectMethods eConObj = eConObj = new Microsoft.Dynamics.GP.eConnect.eConnectMethods();
                // Call the eConnect method to create entries in GP.
                logMessage.AppendLine(DateTime.Now + " Calling the CreateTransactionEntity method.");
                logMessage.AppendLine(DateTime.Now + eConObj.CreateTransactionEntity(EconnectConnectionString, eConnectXml).ToString());
                logMessage.AppendLine(DateTime.Now + " Called CreateTransactionEntity method successfully.");
                result = true;

            }
            catch
            {
                result = false;
                throw;
            }
            return result;
        }

        /// <summary>
        /// Method to transform table information into xml file
        /// </summary>
        /// <param name="tableXml"></param>
        /// <param name="invoiceNumber"></param>
        /// <returns></returns>
        private string Transform(string tableXml, string invoiceNumber, string notes, int companyId, string curncyId = "", double ten99Amnt = 0.00, int isRequiredDistribution = 0)
        {
            // Local variables.
            string transformedXml = string.Empty;
            string documentRowId = string.Empty;
            //Create object for XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            //Create object for XslTransform 
            XslCompiledTransform xslTrans = new XslCompiledTransform();
            //Creating Argument List Object
            XsltArgumentList xsltArgsFO = new XsltArgumentList();
            //Loading ChemMsgEcomOrder XML
            xmlDoc.LoadXml(tableXml);

            if (_isCtsi)
            {
                xsltArgsFO.AddParam(Configuration.XsltParameterInvoiceNumber, "", invoiceNumber);
                xsltArgsFO.AddParam(Configuration.XsltParameterBatchNumber, "", Configuration.CtsiBatchNumber);
                xsltArgsFO.AddParam(Configuration.XsltParameterNotes, "", notes.ToString());
                xslTrans.Load(Configuration.CtsiInvoiceCreationStyleSheet);
            }
            else if (_isApi)
            {
                xsltArgsFO.AddParam(Configuration.XsltParameterInvoiceNumber, "", invoiceNumber);
                xsltArgsFO.AddParam(Configuration.XsltParameterBatchNumber, "", "PAY" + DateTime.Now.ToString("MMddyy") + "-API");
                xsltArgsFO.AddParam(Configuration.XsltParameterCurncyid, "", curncyId.ToString());
                xsltArgsFO.AddParam(Configuration.XsltParameterCompanyID, "", companyId == Convert.ToInt16(Configuration.NACompanyId) ? "CHMPT" : "CPEUR");

                if (InvoiceType == 1)
                {
                    xsltArgsFO.AddParam(Configuration.XsltParameterTen99amnt, "", ten99Amnt.ToString());
                    xsltArgsFO.AddParam(Configuration.XsltParameterIsRequiredDistribution, "", isRequiredDistribution.ToString());
                    xslTrans.Load(Configuration.PaymentCreationStyleSheet);
                }
                else
                {
                    xslTrans.Load(Configuration.POPaymentCreationStyleSheet);
                }
            }

            //Creating StringWriter Object
            StringWriter sWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);
            //Peroforming transformation
            xslTrans.Transform(xmlDoc, xsltArgsFO, sWriter);
            // Set the transformed xml.
            transformedXml = sWriter.ToString().Trim();
            // Dispose the objects.                                                
            sWriter.Dispose();
            // Return the transformed xml to the caller
            return transformedXml;
        }

        /// <summary>
        /// Method to transform object into xml string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string ParseTableToXmlStringForCtsi(DataTable inputTable, DataTable taxTable)
        {
            try
            {
                string resultXml = string.Empty;
                string distDetailsXml = "";

                // creates the serializer object
                using (StringWriter writer = new StringWriter())
                {
                    inputTable.TableName = "Details";
                    inputTable.WriteXml(writer);
                    resultXml = writer.ToString();
                    resultXml = resultXml.Replace("<DocumentElement>", "<Payment>");
                    resultXml = resultXml.Replace("</DocumentElement>", "</Payment>");
                }
                if (companyId == Convert.ToInt16(Configuration.EUCompanyId))
                {
                    // creates the serializer object for distribution
                    using (StringWriter writer = new StringWriter())
                    {
                        taxTable.TableName = "TaxDetails";
                        taxTable.WriteXml(writer);
                        distDetailsXml = writer.ToString();
                        distDetailsXml = distDetailsXml.Replace("<DocumentElement>", "<Tax>");
                        distDetailsXml = distDetailsXml.Replace("</DocumentElement>", "</Tax>");
                        resultXml = resultXml.Replace("</Payment>", distDetailsXml);
                        resultXml = resultXml + "\n" + "</Payment>";
                    }
                }
                return resultXml;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Method to transform object into xml string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string ParseTableToXmlStringForApi(DataTable inputTable, DataTable distributionTable, DataTable taxScheduleTable)
        {
            try
            {
                string resultXml = string.Empty;
                string distDetailsXml = "";
                string taxDetailsXml = "";
                // creates the serializer object
                using (StringWriter writer = new StringWriter())
                {
                    inputTable.TableName = "Details";
                    inputTable.WriteXml(writer);
                    resultXml = writer.ToString();

                    resultXml = resultXml.Replace("<DocumentElement>", "<Payment>");
                    resultXml = resultXml.Replace("</DocumentElement>", "</Payment>");
                    resultXml = resultXml.Replace("<Table>", "<Details>");
                    resultXml = resultXml.Replace("</Table>", "</Details>");
                    resultXml = resultXml.Replace("<Table2>", "<Details>");
                    resultXml = resultXml.Replace("</Table2>", "</Details>");
                }
                if (distributionTable != null && distributionTable.Rows.Count > 0)
                {
                    // creates the serializer object for distribution
                    using (StringWriter writer = new StringWriter())
                    {
                        distributionTable.TableName = "DistributionDetails";
                        distributionTable.WriteXml(writer);
                        distDetailsXml = writer.ToString();
                        distDetailsXml = distDetailsXml.Replace("<DocumentElement>", "<Distributions>");
                        distDetailsXml = distDetailsXml.Replace("</DocumentElement>", "</Distributions>");
                        distDetailsXml = distDetailsXml.Replace("<Table>", "<Distribution>");
                        distDetailsXml = distDetailsXml.Replace("</Table>", "</Distribution>");
                        resultXml = resultXml.Replace("</Payment>", distDetailsXml);
                        resultXml = resultXml + "\n" + "</Payment>";
                    }
                }
                if (companyId == Convert.ToInt16(Configuration.EUCompanyId))
                {
                    if (taxScheduleTable != null && taxScheduleTable.Rows.Count > 0)
                    {
                        // creates the serializer object for distribution
                        using (StringWriter writer = new StringWriter())
                        {
                            taxScheduleTable.TableName = "TaxDetails";
                            taxScheduleTable.WriteXml(writer);
                            taxDetailsXml = writer.ToString();
                            taxDetailsXml = taxDetailsXml.Replace("<DocumentElement>", "<Tax>");
                            taxDetailsXml = taxDetailsXml.Replace("</DocumentElement>", "</Tax>");
                            resultXml = resultXml.Replace("</Payment>", taxDetailsXml);
                            resultXml = resultXml + "\n" + "</Payment>";
                        }
                    }
                }
                return resultXml;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Convert the object into datatable
        /// </summary>
        /// <param name="requsetObj"></param>
        /// <returns>Dataset</returns>
        public DataTable ConvertToTableForAPIeConnect(DataTable dataView, string notes, int invType)
        {
            int rowId = 1;
            DataTable paymentDetailsTable = null;
            DateTime documentDate;
            if (invType == 1)
            {
                paymentDetailsTable = new DataTable();
                paymentDetailsTable.Columns.Add("DOCUMENTROWID", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("DOCTYPE", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("VCHNUMWK", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("DOCDATE", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("VENDORID", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("SHIPTOSTATE", System.Type.GetType("System.String")); //
                paymentDetailsTable.Columns.Add("SHIPPEDDATE", System.Type.GetType("System.String")); //
                paymentDetailsTable.Columns.Add("DOCUMENTAMOUNT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("APPROVEDAMOUNT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("FRTAMNT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("TAXAMNT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("PRCHAMNT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("GLINDEX", System.Type.GetType("System.Int32"));
                paymentDetailsTable.Columns.Add("ACTNUMB", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("ACTDESCR", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("TRDISAMT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("MSCCHAMT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("LOCATIONKEY", System.Type.GetType("System.String")); //
                paymentDetailsTable.Columns.Add("ROWTEXT", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("ROWID", System.Type.GetType("System.Int32"));
                paymentDetailsTable.Columns.Add("TAXSCHID", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("CURNCYID", System.Type.GetType("System.String"));

                foreach (DataRow row in dataView.Rows)
                {
                    DataRow newRow = paymentDetailsTable.NewRow();
                    newRow["DOCUMENTROWID"] = row["DocumentRowId"];
                    if (Convert.ToInt32(row["FormTypeCode"]) == 1)
                    {
                        newRow["DOCTYPE"] = 1;
                    }
                    else
                    {
                        newRow["DOCTYPE"] = 5;
                    }

                    newRow["VCHNUMWK"] = row["DocumentNumber"];
                    documentDate = Convert.ToDateTime(row["DocumentDate"]);
                    newRow["DOCDATE"] = documentDate.Date;
                    newRow["VENDORID"] = row["VendorNumber"];
                    newRow["SHIPTOSTATE"] = row["ShipToState"];
                    newRow["SHIPPEDDATE"] = row["ShippedDate"];
                    newRow["DOCUMENTAMOUNT"] = row["DocumentAmount"];
                    newRow["APPROVEDAMOUNT"] = row["ApprovedAmount"];
                    newRow["FRTAMNT"] = row["FreightAmount"];
                    newRow["TAXAMNT"] = row["SalesTaxAmount"];
                    newRow["PRCHAMNT"] = row["PurchasingAmount"];
                    newRow["GLIndex"] = row["GLIndex"];
                    newRow["ACTNUMB"] = "";
                    newRow["ACTDESCR"] = "";
                    newRow["TRDISAMT"] = row["TradeDiscountAmount"];
                    newRow["MSCCHAMT"] = row["MiscellaneousAmount"];
                    newRow["LOCATIONKEY"] = row["LocationKey"];
                    newRow["ROWTEXT"] = notes;
                    newRow["ROWID"] = rowId;
                    newRow["TAXSCHID"] = row["TaxScheduleId"];
                    newRow["CURNCYID"] = row["CurrencyId"];
                    paymentDetailsTable.Rows.Add(newRow);
                    rowId++;

                }
            }
            else
            {
                paymentDetailsTable = new DataTable();
                paymentDetailsTable.Columns.Add("DOCUMENTNUMBER", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("RECEIPTDATE", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("VENDORID", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("DOCUMENTAMOUNT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("APPROVEDAMOUNT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("FRTAMNT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("TAXAMNT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("SHIPPEDDATE", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("SHIPTOSTATE", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("MISCAMNT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("FEDTAX", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("PONUMBER", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("POAMOUNT", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("PODATEOPENED", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("LOCATIONKEY", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("DOCUMENTROWID", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("POLNENUM", System.Type.GetType("System.Int32"));
                paymentDetailsTable.Columns.Add("ITEMNMBR", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("QTYSHPPD", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("UNITCOST", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("ADJUSTEDITEMUNITQTY", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("ADJUSTEDITEMUNITPRICE", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("EXTDCOST", System.Type.GetType("System.Double"));
                paymentDetailsTable.Columns.Add("POPRCTNM", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("RCPTLNNM", System.Type.GetType("System.Int32"));
                paymentDetailsTable.Columns.Add("ROWTEXT", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("ROWID", System.Type.GetType("System.Int32"));
                paymentDetailsTable.Columns.Add("TAXSCHID", System.Type.GetType("System.String"));
                paymentDetailsTable.Columns.Add("CURNCYID", System.Type.GetType("System.String"));

                foreach (DataRow row in dataView.Rows)
                {
                    DataRow newRow = paymentDetailsTable.NewRow();
                    newRow["DOCUMENTNUMBER"] = row["DocumentNumber"];
                    newRow["RECEIPTDATE"] = row["DocumentDate"];
                    newRow["VENDORID"] = row["VendorNumber"];
                    newRow["DOCUMENTAMOUNT"] = row["DocumentAmount"];
                    newRow["APPROVEDAMOUNT"] = row["ApprovedAmount"];
                    newRow["FRTAMNT"] = row["FreightAmount"];
                    newRow["TAXAMNT"] = row["SalesTaxAmount"];
                    newRow["SHIPPEDDATE"] = row["ShippedDate"];
                    newRow["SHIPTOSTATE"] = row["ShipToState"];
                    newRow["MISCAMNT"] = row["MiscellaneousAmount"];
                    newRow["FEDTAX"] = 0.00;
                    newRow["PONUMBER"] = row["PONumber"];
                    newRow["POAMOUNT"] = row["POAmount"];
                    newRow["PODATEOPENED"] = row["PODateOpened"];
                    newRow["LOCATIONKEY"] = row["LocationKey"];
                    newRow["DOCUMENTROWID"] = row["DocumentRowId"];
                    newRow["POLNENUM"] = row["POLineNumber"];  // row["POLineNumber"];
                    newRow["ITEMNMBR"] = row["ItemNumber"];
                    newRow["QTYSHPPD"] = row["ItemUnitQty"];
                    newRow["UNITCOST"] = row["ItemUnitPrice"];
                    newRow["ADJUSTEDITEMUNITQTY"] = row["AdjustedItemUnitQty"];
                    newRow["ADJUSTEDITEMUNITPRICE"] = row["AdjustedItemUnitPrice"];
                    newRow["EXTDCOST"] = row["ItemExtendedAmount"];
                    newRow["POPRCTNM"] = row["ReceiptNumber"];
                    newRow["RCPTLNNM"] = row["ReceiptLineNumber"]; //row["ReceivingLineNumber"];
                    newRow["ROWTEXT"] = notes;
                    newRow["ROWID"] = rowId;
                    newRow["TAXSCHID"] = row["TaxScheduleId"];
                    newRow["CURNCYID"] = row["CurrencyId"];
                    paymentDetailsTable.Rows.Add(newRow);
                    rowId++;
                }
            }
            return paymentDetailsTable;
        }

        /// <summary>
        /// Read the excel file content
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileHeader"></param>
        /// <returns></returns>
        public static DataTable ReadFileDetails(PayableManagementRequest requestObj)
        {
            DataTable fileDetailsTable = null;
            try
            {
                Excel.Application xlApp;
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                Excel.Range range;
                DataRow row = null;


                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(requestObj.PayableManagementDetails.FileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                            Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                            Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                            Type.Missing, Type.Missing);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
                range = xlWorkSheet.UsedRange;
                if ((null != range) && (null != range.Cells) &&
                    (requestObj.PayableManagementDetails.Source != Configuration.STR_CTSISOURCE ? range.Columns.Count.ToString() == Configuration.ColumnsCount :
                    (requestObj.PayableManagementDetails.FileName.Contains(Configuration.CPEURFileName) || requestObj.PayableManagementDetails.FileName.Contains(Configuration.GBPFileName) ? range.Columns.Count.ToString() == Configuration.CtsiEuColumnsCount : range.Columns.Count.ToString() == Configuration.CtsiNaColumnsCount)))
                {
                    fileDetailsTable = new DataTable("ExcelData");
                    for (int rowCount = 1; rowCount <= range.Rows.Count; rowCount++)
                    {
                        if (range.Cells[rowCount, 1].Value2 != null)
                        {
                            if (rowCount > 1)
                                row = fileDetailsTable.Rows.Add();
                            for (int col = 1; col <= range.Columns.Count; col++)
                            {
                                if (rowCount == 1)

                                {
                                    fileDetailsTable.Columns.Add((string)(range.Cells[rowCount, col] as Excel.Range).Value2).ToString().Trim();
                                }
                                else
                                {
                                    if (requestObj.PayableManagementDetails.Source == Configuration.STR_CTSISOURCE)
                                    {
                                        row[col - 1] = ((range.Cells[rowCount, col] as Excel.Range).Value);
                                    }
                                    else
                                    {
                                        string data = Convert.ToString((range.Cells[rowCount, col] as Excel.Range).Value);
                                        if (data != null)
                                        {
                                            switch (fileDetailsTable.Columns[col - 1].Caption.ToString().ToUpper())
                                            {
                                                case "SECONDARY ID":
                                                    row[col - 1] = data.Trim().ToString().Truncate(15);
                                                    break;
                                                case "ACCOUNT NAME":
                                                    row[col - 1] = data.Trim().ToString().Truncate(255);
                                                    break;
                                                case "BUSINESS UNIT1":
                                                    row[col - 1] = data.Trim().ToString().Truncate(100);
                                                    break;
                                                case "BUSINESS UNIT2":
                                                    row[col - 1] = data.Trim().ToString().Truncate(100);
                                                    break;
                                                case "DESCRIPTION":
                                                    row[col - 1] = data.Trim().ToString().Truncate(100);
                                                    break;
                                                case "VENDOR":
                                                    row[col - 1] = data.Trim().ToString().Truncate(51);
                                                    break;
                                                case "CITY":
                                                    row[col - 1] = data.Trim().ToString().Truncate(35);
                                                    break;
                                                default:
                                                    row[col - 1] = data.Trim().ToString();
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            if (fileDetailsTable.Columns[col - 1].Caption.ToString().ToUpper() == "REPORT TOTAL"
                                                || fileDetailsTable.Columns[col - 1].Caption.ToString().ToUpper() == "TRANSACTION AMOUNT"
                                                    || fileDetailsTable.Columns[col - 1].Caption.ToString().ToUpper() == "ACCOUNT NO"
                                                        )
                                            {
                                                row[col - 1] = 0;
                                            }
                                            else if (fileDetailsTable.Columns[col - 1].Caption.ToString().ToUpper() == "REPORT TOTAL"
                                                        || fileDetailsTable.Columns[col - 1].Caption.ToString().ToUpper() == "TRANSACTION AMOUNT")
                                            {
                                                row[col - 1] = DateTime.MinValue;
                                            }
                                            else
                                            {
                                                row[col - 1] = string.Empty;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    xlWorkBook.Close(true, requestObj.PayableManagementDetails.FileName, Type.Missing);
                    xlApp.Quit();

                    ReleaseObject(xlWorkSheet);
                    ReleaseObject(xlWorkBook);
                    ReleaseObject(xlApp);

                    if (requestObj.PayableManagementDetails.Source == Configuration.STR_CTSISOURCE)
                    {
                        SetDefaultValueForCTSI(fileDetailsTable, requestObj.companyId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return fileDetailsTable;
        }

        /// <summary>
        /// Set default value for numberic columns
        /// </summary>
        /// <param name="paymentDT"></param>
        private static void SetDefaultValueForCTSI(DataTable paymentDT, int companyId)
        {
            //change the column names
            if (paymentDT.Rows.Count > 0)
            {
                paymentDT.Columns.Add("OriginialCTSIInvoiceId", typeof(int), "0").SetOrdinal(0);
                paymentDT.Columns.Add("CTSIFileId", typeof(int), "0").SetOrdinal(1);
                paymentDT.Columns["DOCUMENT_ROWID"].ColumnName = "CTSIId";
                paymentDT.Columns["DOC_TYPE"].ColumnName = "DocumentType";
                paymentDT.Columns["VCHNUMWK"].ColumnName = "DocumentNumber";
                paymentDT.Columns["DOCDATE"].ColumnName = "DocumentDate";
                paymentDT.Columns["VENDORID"].ColumnName = "VendorId";
                paymentDT.Columns["TOT_APPROVED_DOC_AMT"].ColumnName = "TotalApprovedDocumentAmount";
                paymentDT.Columns["APPROVED_AMOUNT"].ColumnName = "ApprovedAmountt";
                paymentDT.Columns["TOT_OVERCHARGE_AMT"].ColumnName = "OverCharge";
                paymentDT.Columns["FRT_AMOUNT"].ColumnName = "FreightAmount";
                paymentDT.Columns["TAX_AMOUNT"].ColumnName = "TaxAmount";
                paymentDT.Columns["MISCELLANEOUS"].ColumnName = "MiscellaneousAmount";
                paymentDT.Columns["TRADE_DISCOUNTS"].ColumnName = "TradeDiscounts";
                paymentDT.Columns["PRCHAMNT"].ColumnName = "PurchaseAmount";
                paymentDT.Columns["CURRENCY_CODE"].ColumnName = "CurrencyCode";
                paymentDT.Columns["GL_ACCOUNT"].ColumnName = "GlAccount";
                paymentDT.Columns["GL_CODE_DESC_LOC"].ColumnName = "GlAccountdescription";
                paymentDT.Columns["CPT_REFERENCE"].ColumnName = "CptReference";
                if (companyId == 1)
                {
                    paymentDT.Columns["AIRWAY_INV_NUM"].ColumnName = "AirWayInvoiceNumber";
                }
                else if (companyId == 2)
                {
                    paymentDT.Columns["AIRWAY_NUMBER"].ColumnName = "AirWayInvoiceNumber";
                    paymentDT.Columns["BASE_NL_LOCAL_CHARGE"].ColumnName = "BaseLocalCharge";
                    paymentDT.Columns["BASE_NL_ZERO_RATED"].ColumnName = "BaseZeroRatedCharge";
                    paymentDT.Columns["BASE_NL_REVERSE_CHARGE"].ColumnName = "BaseReverseCharge";

                    paymentDT.Columns["TaxAmount"].SetOrdinal(11);
                    paymentDT.Columns["CurrencyCode"].SetOrdinal(15);
                    paymentDT.Columns["GlAccount"].SetOrdinal(16);
                    paymentDT.Columns["GlAccountdescription"].SetOrdinal(17);
                    paymentDT.Columns["CptReference"].SetOrdinal(18);
                    paymentDT.Columns["AirWayInvoiceNumber"].SetOrdinal(19);

                }
            }
            foreach (DataRow row in paymentDT.Rows)
            {
                foreach (DataColumn col in paymentDT.Columns)
                {
                    //test for null here
                    if ((string.IsNullOrEmpty(row[col].ToString())) && ((col.ColumnName == "TotalApprovedDocumentAmount")
                                                        || (col.ColumnName == "ApprovedAmount")
                                                        || (col.ColumnName == "OverCharge")
                                                        || (col.ColumnName == "FreightAmount")
                                                        || (col.ColumnName == "TaxAmount")
                                                        || (col.ColumnName == "MiscellaneousAmount")
                                                        || (col.ColumnName == "TradeDiscounts")
                                                        || (col.ColumnName == "PurchaseAmount")))
                    {
                        row[col] = 0;
                    }
                }

                // update the invoice type to 1 for INV
                DataRow[] rowList = paymentDT.Select("DocumentType= '" + Configuration.DocType + "' ");
                row["DocumentType"] = 1;
            }
        }

        /// <summary>
        /// Release the object
        /// </summary>
        /// <param name="obj"></param>
        public static void ReleaseObject(object value)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(value);
                value = null;
            }
            catch (Exception)
            {
                value = null;
                throw;
            }
        }

        #endregion

        #region CTSI Implementation

        public PayableManagementResponse ProcessCtsiFile(PayableManagementRequest payableManagementRequest)
        {
            DataTable fileDetailsTable = null;
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository payableManagementDataAccess = null;
            StringBuilder logMessage = new StringBuilder();
            //StringBuilder mailContent = null;
            try
            {
                responseObj = new PayableManagementResponse();
                //mailContent = new StringBuilder();

                //mailContent.Append("<tr><td colspan=2>File : " + dir.Name + "</td></tr>");
                //Read the excel file content
                logMessage.AppendLine(DateTime.Now + " Reading the Excel File");
                fileDetailsTable = ReadFileDetails(payableManagementRequest);

                if (fileDetailsTable != null && fileDetailsTable.Rows.Count > 0)
                {
                    //call the sp to validate and caluclate from datalayer class
                    DataSet validateTransactionDs = new DataSet();
                    logMessage.AppendLine(DateTime.Now + " Validating the excel details by calling the validate SP");
                    payableManagementDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(payableManagementRequest.ConnectionString);
                    validateTransactionDs = payableManagementDataAccess.ValidateAndGetCtsiTransactions(payableManagementRequest.PayableManagementDetails.UserId, payableManagementRequest.companyId, fileDetailsTable);

                    if (validateTransactionDs != null && validateTransactionDs.Tables.Count > 0)
                    {
                        if (payableManagementRequest.companyId == 1)
                            payableManagementRequest.CTSIValidationList = GetAllEntities<PayableLineEntity, PayableManagementCTSIMap>(validateTransactionDs.Tables[0]).ToList();
                        else
                        {
                            payableManagementRequest.CTSIValidationList = GetAllEntities<PayableLineEntity, PayableManagementCTSIMap>(validateTransactionDs.Tables[0]).ToList();
                            payableManagementRequest.CTSITaxValidationList = GetAllEntities<PayableLineEntity, PayableManagementCTSITaxMap>(validateTransactionDs.Tables[1]).ToList();
                        }

                        responseObj = UploadPayableDetailsIntoGpForCtsi(payableManagementRequest);

                        if (responseObj != null)
                            logMessage.AppendLine(responseObj.LogMessage.ToString());
                    }
                    else
                    {
                        logMessage.AppendLine(DateTime.Now + " No valid records found in payment file.");
                    }

                    responseObj.Status = Response.Success;
                    responseObj.LogMessage = logMessage.ToString().Trim();
                }
                else
                {
                    responseObj.Status = Response.Success;
                    logMessage.AppendLine(DateTime.Now + " Invalid file / columns count mismatch.");
                }

            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.LogMessage = ex.StackTrace;
                throw;
            }
            return responseObj;
        }
        public PayableManagementResponse UploadPayableDetailsIntoGpForCtsi(PayableManagementRequest request)
        {
            PayableManagementResponse payableResponse = null;
            IPayableManagementRepository payableDataAccess = null;
            DataTable validRows = null;
            DataTable inValidRows = null;
            DataTable TaxDetails = new DataTable();
            DirectoryInfo dir = null;
            string fileName = string.Empty;
            ctsimMailContent = new StringBuilder();
            bool isAlreadyProcessed = false;
            _isCtsi = true;

            
            try
            {
                payableResponse = new PayableManagementResponse();
                logMessage = new StringBuilder();

                //Need to convert list to DS which is comes from GPAddIns and Task Scheduler
                DataSet dtInvoiceDetails = new DataSet();
                DataTable dtInvoiceDetailsDT = new DataTable();
                DataTable TaxDT = new DataTable();


                if ((request.CTSIValidationList != null && request.CTSIValidationList.Count > 0))
                {
                    logMessage.AppendLine(DateTime.Now + " CTSIValidationList ");
                    if (request.companyId == 1)
                    {
                        dtInvoiceDetailsDT = DataTableMapper.CTSIInvoiceDetailsNA(request, DataColumnMappings.CTSIInvoiceDetailsNA);
                        dtInvoiceDetails.Tables.Add(dtInvoiceDetailsDT);
                    }
                    else
                    {
                        dtInvoiceDetailsDT = DataTableMapper.CTSIInvoiceDetailsEU(request, DataColumnMappings.CTSIInvoiceDetailsEU);
                        TaxDT = DataTableMapper.CTSIInvoiceDetailsTaxEU(request, DataColumnMappings.CTSIInvoiceDetailsTaxEU);
                        dtInvoiceDetails.Tables.Add(dtInvoiceDetailsDT);
                        dtInvoiceDetails.Tables.Add(TaxDT);
                    }
                    

                    logMessage.AppendLine(DateTime.Now + " CTSIValidationList Count : " + dtInvoiceDetails.Tables[0].Rows.Count.ToString());
                }

                if (!string.IsNullOrEmpty(request.PayableManagementDetails.FileName) && string.IsNullOrEmpty(request.PayableManagementDetails.SourceFormName))
                {

                    fileName = request.PayableManagementDetails.FileName;
                    dir = new DirectoryInfo(request.PayableManagementDetails.FileName);
                    logMessage.AppendLine(DateTime.Now + " Fetching details from file : " + dir.Name);
                    //request.PayableManagementDetails.SourceFormName = dir.Name;
                }

                if (dtInvoiceDetails != null && dtInvoiceDetails.Tables[0].Rows.Count > 0)
                {
                    if (string.IsNullOrEmpty(request.PayableManagementDetails.SourceFormName)) dtInvoiceDetails.Tables[0].Columns.Add("UserId");

                    foreach (DataRow row in dtInvoiceDetails.Tables[0].Rows)
                    {
                        StringBuilder notes = new StringBuilder();
                        foreach (DataColumn col in dtInvoiceDetails.Tables[0].Columns)
                        {
                            if (col.ColumnName == "CTSIId" || col.ColumnName == "DocumentNumber" || col.ColumnName == "DocumentType"
                                || col.ColumnName == "DocumentDate" || col.ColumnName == "VendorId"
                                || col.ColumnName == "TotalApprovedDocumentAmount" || col.ColumnName == "ApprovedAmount"
                                || col.ColumnName == "FreightAmount" || col.ColumnName == "TaxAmount" || col.ColumnName == "MiscellaneousAmount"
                                || col.ColumnName == "TradeDiscounts" || col.ColumnName == "PurchaseAmount" || col.ColumnName == "OverCharge"
                                || col.ColumnName == "CurrencyCode" || col.ColumnName == "GlAccount" || col.ColumnName == "GlAccountDescription"
                                || col.ColumnName == "CptReference" || col.ColumnName == "AirWayInvoiceNumber")
                                notes.Append(row[col].ToString() + "|");
                        }
                        //remove the pipeline at the end
                        row["Notes"] = notes.Remove(notes.Length - 1, 1);

                        if (string.IsNullOrEmpty(request.PayableManagementDetails.SourceFormName))
                            row["UserId"] = request.PayableManagementDetails.UserId.ToString().Trim();
                    }

                    logMessage.AppendLine(DateTime.Now + " Calling eConnect Wrapper service");


                    //Calling Service method
                    source = request.PayableManagementDetails.SourceFormName;
                    logMessage.AppendLine(DateTime.Now.ToString() + " Parsing the xml string into Data table");
                    ctsiDS = new DataTable();
                    ctsiDS = dtInvoiceDetails.Tables[0];
                    if (string.IsNullOrEmpty(request.UserId))
                        userId = ctsiDS.Rows[0]["UserId"].ToString();
                    else
                        userId = request.UserId.ToString().Trim();
                    companyId = request.companyId;
                    currencyId = Convert.ToString(ctsiDS.Rows[0]["CurrencyCode"]).Trim();
                    if (ctsiDS.Rows.Count > 0)
                    {
                        logMessage.AppendLine(DateTime.Now.ToString() + " Total records count in the table is :" + ctsiDS.Rows.Count.ToString());
                        ctsimMailContent.AppendLine(request.EmailRequest.EmailInformation.Body);
                        ctsimMailContent.Replace("</table>", "").Replace("</html>", "");
                        if (ctsiDS.Select("StatusId = 0").Count() > 0)
                        {
                            validRows = ctsiDS.Select("StatusId=0", "CTSIId").CopyToDataTable();
                        }

                        if (string.IsNullOrEmpty(request.PayableManagementDetails.SourceFormName))
                        {
                            ctsiDS.Columns.Remove("CtsiStatus");
                            ctsiDS.Columns.Remove("CreditAccountNumber");
                            ctsiDS.Columns.Remove("CurrencyDecimalPlaces");
                            ctsiDS.Columns.Remove("OtherDuplicates");
                            ctsiDS.Columns.Remove("DocumentType");
                            ctsiDS.Columns.Remove("DebitDistributionType");
                            ctsiDS.Columns.Remove("CreditAmount");
                            if (companyId == Convert.ToInt16(Configuration.EUCompanyId))
                            {
                                ctsiDS.Columns.Remove("TaxScheduleId");
                                ctsiDS.Columns.Remove("BaseChargeType");
                            }
                        }
                        ctsiDS.Columns.Remove("Notes");
                        if (string.IsNullOrEmpty(request.UserId))
                            ctsiDS.Columns.Remove("UserId");
                        ctsiDS.Columns["CTSIId"].SetOrdinal(0);
                        logMessage.AppendLine(DateTime.Now + "Form Name : " + request.PayableManagementDetails.SourceFormName);

                        if (string.IsNullOrEmpty(request.PayableManagementDetails.SourceFormName))
                        {
                            if (ctsiDS.Select("StatusId <> 0").Count() > 0)
                            {
                                inValidRows = ctsiDS.Select("StatusId <> 0").AsEnumerable().OrderBy(row => row.Field<string>("CTSIId")).ThenByDescending(row => row.Field<int>("StatusId")).CopyToDataTable();

                                //Insert the invalid rows into mailmessage
                                if (null != inValidRows && inValidRows.Rows.Count > 0)
                                {
                                    //Append the invalid records in mail
                                    logMessage.AppendLine(DateTime.Now.ToString() + " Appending the invalid records in the mail message.");
                                    foreach (DataRow row in inValidRows.Rows)
                                    {
                                        ctsimMailContent.AppendLine("<tr><td>" + row["CTSIId"].ToString() +
                                                                   "</td><td>" + row["DocumentNumber"].ToString() +
                                                                   "</td><td>" + row["VendorId"].ToString() +
                                                                   "</td><td>" + row["ErrorDescription"].ToString() + "</td></tr>");
                                    }
                                }
                            }

                            //insert the document number and status in log table  
                            logMessage.AppendLine(DateTime.Now.ToString() + " Saving the ctsi details into table.");
                            ctsiDS.Columns.Remove("ErrorDescription");

                            if (companyId == Convert.ToInt16(Configuration.EUCompanyId))
                            {
                                ctsiDS.Columns["CurrencyCode"].SetOrdinal(14);
                                ctsiDS.Columns["GlAccount"].SetOrdinal(15);
                                ctsiDS.Columns["GlAccountdescription"].SetOrdinal(16);
                                ctsiDS.Columns["CptReference"].SetOrdinal(17);
                                ctsiDS.Columns["AirWayInvoiceNumber"].SetOrdinal(18);
                                ctsiDS.Columns["StatusId"].SetOrdinal(19);
                                ctsiDS.Columns["ValidDocumentNumber"].SetOrdinal(20);
                                ctsiDS.Columns["BaseLocalCharge"].SetOrdinal(21);
                            }

                            payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(request.ConnectionString);
                            isAlreadyProcessed = payableDataAccess.SaveCTSITransactions(ctsiDS, userId, companyId, request.PayableManagementDetails.FileName, currencyId);

                        }

                        //The following operation is only for the CTSIReportReUploadFailureForm
                        else if (request.PayableManagementDetails.SourceFormName.ToLower().Trim() == Configuration.CTSIReportReUploadFailureForm.ToString().Trim())
                        {
                            ctsiDS.Columns.Remove("DebitDistributionType");
                            ctsiDS.Columns.Remove("CurrencyDecimalPlaces");
                            ctsiDS.Columns.Remove("CreditAccountNumber");
                            ctsiDS.Columns.Remove("CreditAmount");
                            ctsiDS.Columns.Remove("ErrorDescription");

                            //insert the document number and status in log table  
                            logMessage.AppendLine(DateTime.Now.ToString() + " Saving the reuploaded Ctsi details into table.");
                            //ctsiDS.Columns.Remove("ErrorDescription");
                            payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(request.ConnectionString);
                            payableDataAccess.SaveCTSIFileDetailsForReupload(ctsiDS, userId, companyId);
                        }


                        if (!isAlreadyProcessed)
                        {
                            if (validRows != null && validRows.Rows.Count > 0)
                            {
                                var groupedRows = validRows.AsEnumerable().GroupBy(g => g.Field<string>("CTSIId"));
                                foreach (var groupedItem in groupedRows)
                                {
                                    StringBuilder notes = new StringBuilder();
                                    //Appending the notes details per RecordId
                                    groupedItem.AsEnumerable().ToList().ForEach(f => notes.AppendLine((f.Field<string>("Notes")).ToString()));
                                    //Processing the records ony by one

                                    TaxDetails = companyId == Convert.ToInt16(Configuration.EUCompanyId) ? dtInvoiceDetails.Tables[1] : null;
                                    PushToGP(request,groupedItem.CopyToDataTable(), notes.ToString(), companyId, TaxDetails, source, null, request.ConnectionString, request.EconnectConString, ref logMessage);
                                }
                            }

                            // Sends email if it has any failure records
                            if ((ctsimMailContent.ToString().Contains("<tr><td>")
                               || (ctsimMailContent.ToString().Contains("<tr><td colspan=4>")
                                || ctsimMailContent.ToString().Contains("<tr><td colspan=3>"))))
                            {
                                logMessage.AppendLine(DateTime.Now + " Sending mail.");
                                ctsimMailContent.Append("</table></html>");

                                logMessage.AppendLine(DateTime.Now +" Email From : " + request.EmailRequest.EmailInformation.EmailFrom.ToString().Trim());
                                logMessage.AppendLine(DateTime.Now + " Email To : " + request.EmailRequest.EmailInformation.EmailTo.ToString().Trim());
                                logMessage.AppendLine(DateTime.Now + " Email BCC : " + request.EmailRequest.EmailInformation.EmailBcc.ToString().Trim());
                                logMessage.AppendLine(DateTime.Now + " Email CC : " + request.EmailRequest.EmailInformation.EmailCc.ToString().Trim());
                                logMessage.AppendLine(DateTime.Now + " Email Subject :" + request.EmailRequest.EmailInformation.Subject + (request.companyId == 1 ? "CHMPT" : "CPEUR"));
                                logMessage.AppendLine(DateTime.Now + " Email Body : " + ctsimMailContent.ToString());
                                logMessage.AppendLine(DateTime.Now + " Email SMTP Address : " + request.EmailRequest.EmailInformation.SmtpAddress);

                                if (SendMail(ctsimMailContent.ToString(), request))
                                    logMessage.AppendLine(DateTime.Now + " Mail has been sent successfully.");
                                else
                                    logMessage.AppendLine(DateTime.Now + " Mail is not sent successfully.");

                                logMessage.AppendLine(DateTime.Now + " ----------------------------------------------------.");
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now.ToString() + " The same file is processed already.");
                        }
                    }

                    if (ctsiDS.Select("StatusId <> 0 ").Count() > 0)
                    {
                        payableResponse.ErrorMessage = Configuration.PartialNone;
                    }
                    else if (ctsiDS.Select("StatusId = 0 ").Count() == validRows.Rows.Count)
                    {
                        payableResponse.ErrorMessage = Configuration.AllPass;
                    }
                    else if (logMessage.ToString().Contains(Configuration.AlreadyProcessed))
                    {
                        payableResponse.ErrorMessage = Configuration.AlreadyProcessed;
                    }
                }

                if (!string.IsNullOrEmpty(request.PayableManagementDetails.FileName) && string.IsNullOrEmpty(request.PayableManagementDetails.SourceFormName))
                {
                    //deletes the processed files from the folder.  
                    logMessage.AppendLine(DateTime.Now + " Deleting the processed file from the path");
                    logMessage.AppendLine(DateTime.Now + " Moving the file to the path " + Configuration.ExtractedFilesArchiveFolder);
                    File.Move(request.PayableManagementDetails.FileName, Configuration.ExtractedFilesArchiveFolder +
                           DateTime.Now.ToString("yyyyMMddHHmmss") + dir.Name);
                    logMessage.AppendLine(DateTime.Now + " File deleted successfully");
                }

                logMessage.AppendLine(DateTime.Now + " Econnect wrapper service called successfully");
                payableResponse.Status = Response.Success;
                payableResponse.LogMessage = logMessage.ToString().Trim();

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + ex.StackTrace);
                payableResponse.Status = Response.Error;
                logMessage.AppendLine(DateTime.Now + ex.Message.ToString().Trim());
                payableResponse.ErrorMessage = ex.Message.ToString().Trim();
                payableResponse.LogMessage = logMessage.ToString().Trim();
            }
            return payableResponse;
        }

        private DataSet ConvertStringToDataSet(string data)
        {
            DataSet dataSet = new DataSet();
            bool columnsAdded = false;
            int count = 0;
            foreach (string row in data.Split('#'))
            {
                DataRow dataRow = dataSet.Tables[count].NewRow();
                foreach (string cell in row.Split('|'))
                {
                    string[] keyValue = cell.Split('~');
                    if (!columnsAdded)
                    {
                        DataColumn dataColumn = new DataColumn(keyValue[0]);
                        dataSet.Tables[count].Columns.Add(dataColumn);
                    }
                    dataRow[keyValue[0]] = keyValue[1];
                }
                columnsAdded = true;
                dataSet.Tables[count].Rows.Add(dataRow);
                count++;
            }
            return dataSet;
        }

        public PayableManagementResponse GetFailedCTSIIdsList(PayableManagementRequest request)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                responseObj = new PayableManagementResponse();
                logMessage.AppendLine("**********GetFailedCTSIIdsList started********");
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(request.ConnectionString);

                logMessage.AppendLine("**********GetFailedCTSIIdsList DL calling...********");
                DataSet CtsiIdLookupDs = poIndicatorDataAccess.GetFailedCTSIIdsList(request.companyId);
                logMessage.AppendLine("**********GetFailedCTSIIdsList DL called Successfully...********");
                if (CtsiIdLookupDs.Tables.Count > 0 && CtsiIdLookupDs.Tables[0].Rows.Count > 0)
                {
                    responseObj.LookupDetails = new PayableLookupDetailEntity();
                    foreach (DataRow r in CtsiIdLookupDs.Tables[0].Rows)
                    {
                        responseObj.LookupDetails.AddLookupLine(new PayableLookupLineEntity()
                        {
                            CtsiId = string.IsNullOrEmpty(r["CtsiId"].ToString()) ? string.Empty : r["CtsiId"].ToString().Trim(),
                        });
                    }
                }
                responseObj.LogMessage = logMessage.ToString().Trim();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(ex.Message.ToString().Trim());
                responseObj.LogMessage = logMessage.ToString().Trim();
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();

            }
            return responseObj;
        }
        public PayableManagementResponse GetFailedCtsiDocuments(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Converting the request object into dataset");
                logMessageDetail.AppendLine(DateTime.Now + " Fetch the failed CTSI records to load into Re-Upload Failed Documents Scroll");

                //call the sp to validate and caluclate from datalayer class
                DataSet transactionDs = new DataSet();
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                transactionDs = poIndicatorDataAccess.GetFailedCtsiDocuments(requestObj.SearchType, requestObj.FromCtsiId, requestObj.ToCtsiId, requestObj.FromDocumentDate,
                    requestObj.ToDocumentDate, requestObj.FromVendorId, requestObj.ToVendorId, requestObj.companyId);

                if (transactionDs.Tables.Count > 0 && transactionDs.Tables[0].Rows.Count > 0)
                {
                    responseObj.PayableDetailsEntity = new PayableDetailsEntity();
                    foreach (DataRow r in transactionDs.Tables[0].Rows)
                    {
                        PayableLineEntity linedetails = new PayableLineEntity()
                        {
                            OriginalCTSIInvoiceId = Convert.ToInt32(r["OriginialCTSIInvoiceId"].ToString()),
                            CtsiId = string.IsNullOrEmpty(r["CtsiId"].ToString()) ? string.Empty : r["CtsiId"].ToString().Trim(),
                            DocumentNumber = string.IsNullOrEmpty(r["DocumentNumber"].ToString()) ? string.Empty : r["DocumentNumber"].ToString().Trim(),
                            DocumentDate = Convert.ToDateTime(r["DocumentDate"].ToString()),
                            VendorId = string.IsNullOrEmpty(r["VendorId"].ToString()) ? string.Empty : r["VendorId"].ToString().Trim(),
                            TotalApprovedDocumentAmount = Convert.ToDecimal(r["TotalApprovedDocumentAmount"]),
                            ApprovedAmount = Convert.ToDecimal(r["ApprovedAmount"]),
                            OverCharge = Convert.ToDecimal(r["OverChargeAmount"]),
                            FreightAmount = Convert.ToDecimal(r["FreightAmount"]),
                            TaxAmount = Convert.ToDecimal(r["TaxAmount"]),
                            MiscellaneousAmount = Convert.ToDecimal(r["MiscellaneousAmount"]),
                            TradeDiscounts = Convert.ToDecimal(r["TradeDiscounts"]),
                            PurchaseAmount = Convert.ToDecimal(r["PurchaseAmount"]),
                            CurrencyCode = string.IsNullOrEmpty(r["CurrencyCode"].ToString()) ? string.Empty : r["CurrencyCode"].ToString().Trim(),
                            GLAccount = string.IsNullOrEmpty(r["GlAccountNumber"].ToString()) ? string.Empty : r["GlAccountNumber"].ToString().Trim(),
                            GLAccountDescription = string.IsNullOrEmpty(r["GlAccountDescription"].ToString()) ? string.Empty : r["GlAccountDescription"].ToString().Trim(),
                            CPTReference = string.IsNullOrEmpty(r["CptReference"].ToString()) ? string.Empty : r["CptReference"].ToString().Trim(),
                            AirwayInvoiceNumber = string.IsNullOrEmpty(r["AirWayInvoiceNumber"].ToString()) ? string.Empty : r["AirWayInvoiceNumber"].ToString().Trim(),
                            GPVoucherNumber = string.IsNullOrEmpty(r["GpVoucherNumber"].ToString()) ? string.Empty : r["GpVoucherNumber"].ToString().Trim(),
                        };
                        if (requestObj.Company == Configuration.STR_EURCompany)
                        {
                            linedetails.BaseLocalCharge = Convert.ToDecimal(r["BaseLocalCharge"]);
                            linedetails.BaseZeroRatedCharge = Convert.ToDecimal(r["BaseZeroRatedCharge"]);
                            linedetails.BaseReverseCharge = Convert.ToDecimal(r["BaseReverseCharge"]);
                        }
                        responseObj.PayableDetailsEntity.AddInvoiceLine(linedetails);
                    }
                }
                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }
        public PayableManagementResponse ValidateAndGetCtsiTransactions(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            DataTable fileDetailsTable = null;
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Converting the request object into dataset");
                fileDetailsTable = ConvertListToTableForCTSIRevalidate(requestObj);

                //call the sp to validate and caluclate from datalayer class
                DataSet validateTransactionDs = new DataSet();
                logMessageDetail.AppendLine(DateTime.Now + " Validating the excel details by calling the validate SP");
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                validateTransactionDs = poIndicatorDataAccess.ValidateAndGetCtsiTransactions(requestObj.userId, requestObj.companyId, fileDetailsTable);
                logMessageDetail.AppendLine(DateTime.Now + " Converting the dataset into response object");
                if (validateTransactionDs.Tables.Count > 0 && validateTransactionDs.Tables[0].Rows.Count > 0)
                {
                    responseObj.CurrencyDecimal = Convert.ToInt32(validateTransactionDs.Tables[0].Rows[0]["CurrencyDecimalPlaces"].ToString());
                    responseObj.PayableDetailsEntity = new PayableDetailsEntity();
                    foreach (DataRow r in validateTransactionDs.Tables[0].Rows)
                    {
                        PayableLineEntity linedetails = new PayableLineEntity()
                        {
                            OriginalCTSIInvoiceId = Convert.ToInt32(r["OriginialCTSIInvoiceId"].ToString()),
                            CtsiId = (r["CtsiId"] != null && string.IsNullOrEmpty(r["CtsiId"].ToString())) ? string.Empty : r["CtsiId"].ToString().Trim(),
                            DocumentNumber = string.IsNullOrEmpty(r["DocumentNumber"].ToString()) ? string.Empty : r["DocumentNumber"].ToString().Trim(),
                            DocumentDate = Convert.ToDateTime(r["DocumentDate"].ToString()),
                            VendorId = string.IsNullOrEmpty(r["VendorId"].ToString()) ? string.Empty : r["VendorId"].ToString().Trim(),
                            TotalApprovedDocumentAmount = Convert.ToDecimal(r["TotalApprovedDocumentAmount"]),
                            ApprovedAmount = Convert.ToDecimal(r["ApprovedAmount"]),
                            OverCharge = Convert.ToDecimal(r["OverCharge"]),
                            FreightAmount = Convert.ToDecimal(r["FreightAmount"]),
                            TaxAmount = Convert.ToDecimal(r["TaxAmount"]),
                            MiscellaneousAmount = Convert.ToDecimal(r["MiscellaneousAmount"]),
                            TradeDiscounts = Convert.ToDecimal(r["TradeDiscounts"]),
                            PurchaseAmount = Convert.ToDecimal(r["PurchaseAmount"]),
                            CurrencyCode = string.IsNullOrEmpty(r["CurrencyCode"].ToString()) ? string.Empty : r["CurrencyCode"].ToString().Trim(),
                            GLAccountNumber = string.IsNullOrEmpty(r["GlAccount"].ToString()) ? string.Empty : r["GlAccount"].ToString().Trim(),
                            GLAccountDescription = string.IsNullOrEmpty(r["GlAccountDescription"].ToString()) ? string.Empty : r["GlAccountDescription"].ToString().Trim(),
                            CPTReference = string.IsNullOrEmpty(r["CptReference"].ToString()) ? string.Empty : r["CptReference"].ToString().Trim(),
                            AirwayInvoiceNumber = string.IsNullOrEmpty(r["AirWayInvoiceNumber"].ToString()) ? string.Empty : r["AirWayInvoiceNumber"].ToString().Trim(),
                            CtsiStatus = string.IsNullOrEmpty(r["CtsiStatus"].ToString()) ? string.Empty : r["CtsiStatus"].ToString().Trim(),
                            ErrorDescription = string.IsNullOrEmpty(r["ErrorDescription"].ToString()) ? string.Empty : r["ErrorDescription"].ToString().Trim(),
                            ValidDocumentNumber = string.IsNullOrEmpty(r["ValidDocumentNumber"].ToString()) ? string.Empty : r["ValidDocumentNumber"].ToString().Trim(),
                            CreditAccountNumber = string.IsNullOrEmpty(r["CreditAccountNumber"].ToString()) ? string.Empty : r["CreditAccountNumber"].ToString().Trim(),
                            CurrencyDecimal = Convert.ToInt32(r["CurrencyDecimalPlaces"].ToString()),
                            CreditAmount = Convert.ToDecimal(r["CreditAmount"]),
                            DebitDistributionType = Convert.ToInt32(r["DebitDistributionType"].ToString())
                        };
                        if (requestObj.Company == Configuration.STR_EURCompany)
                        {
                            linedetails.BaseLocalCharge = Convert.ToDecimal(r["BaseLocalCharge"]);
                            linedetails.BaseZeroRatedCharge = Convert.ToDecimal(r["BaseZeroRatedCharge"]);
                            linedetails.BaseReverseCharge = Convert.ToDecimal(r["BaseReverseCharge"]);
                        }
                        responseObj.PayableDetailsEntity.AddInvoiceLine(linedetails);
                    }
                }
                if (requestObj.Company == Configuration.STR_EURCompany && validateTransactionDs.Tables[1] != null && validateTransactionDs.Tables[1].Rows.Count > 0)
                {
                    // responseObj.PayableDetailsEntity.TaxDetails = validateTransactionDs.Tables[1];
                    //validateTransactionDs.Tables.Remove(validateTransactionDs.Tables[1]);
                }

                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }
        public static DataTable ConvertListToTableForCTSIRevalidate(PayableManagementRequest requestObj)
        {
            DataTable lineDetailsTable = null;
            lineDetailsTable = new DataTable("@LineDetails");
            lineDetailsTable.Columns.Add("OriginialCTSIInvoiceId", typeof(int));
            lineDetailsTable.Columns.Add("CtsiFileId", typeof(int));
            lineDetailsTable.Columns.Add("CtsiId", typeof(string));
            lineDetailsTable.Columns.Add("DocumentType", typeof(string));
            lineDetailsTable.Columns.Add("DocumentNumber", typeof(string));
            lineDetailsTable.Columns.Add("DocumentDate", typeof(DateTime));
            lineDetailsTable.Columns.Add("VendorId", typeof(string));
            lineDetailsTable.Columns.Add("TotalApprovedDocumentAmount", typeof(decimal));
            lineDetailsTable.Columns.Add("ApprovedAmount", typeof(decimal));
            lineDetailsTable.Columns.Add("OverCharge", typeof(decimal));
            lineDetailsTable.Columns.Add("FreightAmount", typeof(decimal));
            lineDetailsTable.Columns.Add("TaxAmount", typeof(decimal));
            lineDetailsTable.Columns.Add("MiscellaneousAmount", typeof(decimal));
            lineDetailsTable.Columns.Add("TradeDiscounts", typeof(decimal));
            lineDetailsTable.Columns.Add("PurchaseAmount", typeof(decimal));
            lineDetailsTable.Columns.Add("CurrencyCode", typeof(string));
            lineDetailsTable.Columns.Add("GlAccount", typeof(string));
            lineDetailsTable.Columns.Add("GlAccountDescription", typeof(string));
            lineDetailsTable.Columns.Add("CptReference", typeof(string));
            lineDetailsTable.Columns.Add("AirWayInvoiceNumber", typeof(string));

            if (requestObj.Company == Configuration.STR_EURCompany)
            {
                lineDetailsTable.Columns.Add("BaseLocalCharge", typeof(string));
                lineDetailsTable.Columns.Add("BaseZeroRatedCharge", typeof(string));
                lineDetailsTable.Columns.Add("BaseReverseCharge", typeof(string));

            }
            foreach (var data in requestObj.PayableDetailsEntity.GetInvoiceLineDetails)
            {
                var row = lineDetailsTable.NewRow();
                row["OriginialCTSIInvoiceId"] = data.OriginalCTSIInvoiceId;
                row["CtsiFileId"] = data.CtsiFileId;
                row["CtsiId"] = data.CtsiId;
                row["DocumentType"] = data.DocumentType;
                row["DocumentNumber"] = data.VoucherNumber;
                row["DocumentDate"] = data.DocumentDate;
                row["VendorId"] = data.VendorId;
                row["TotalApprovedDocumentAmount"] = data.TotalApprovedDocumentAmount;
                row["ApprovedAmount"] = data.ApprovedAmount;
                row["OverCharge"] = data.OverCharge;
                row["FreightAmount"] = data.FreightAmount;
                row["MiscellaneousAmount"] = data.MiscellaneousAmount;
                row["TradeDiscounts"] = data.TradeDiscounts;
                row["PurchaseAmount"] = data.PurchaseAmount;
                row["TaxAmount"] = data.TaxAmount;
                row["CurrencyCode"] = data.CurrencyCode;
                row["GlAccount"] = data.GLAccount;
                row["GlAccountDescription"] = data.GLAccountDescription;
                row["CptReference"] = data.CPTReference;
                row["AirWayInvoiceNumber"] = data.AirwayInvoiceNumber;

                if (requestObj.Company == Configuration.STR_EURCompany)
                {
                    row["BaseLocalCharge"] = data.BaseLocalCharge;
                    row["BaseZeroRatedCharge"] = data.BaseZeroRatedCharge;
                    row["BaseReverseCharge"] = data.BaseReverseCharge;
                }

                lineDetailsTable.Rows.Add(row);
            }
            return lineDetailsTable;
        }
        public PayableManagementResponse ValidateVoucherExistsForVendor(PayableManagementRequest request)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            try
            {
                logMessageDetail.AppendLine("ValidateVoucherExistsForVendor BL calling....");
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine("Connection string - " + request.ConnectionString.ToString().Trim());
                logMessageDetail.AppendLine("Company Id - " + request.companyId.ToString().Trim());
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(request.ConnectionString);
                responseObj.IsValid = poIndicatorDataAccess.ValidateVoucherExistsForVendor(request.ManualPaymentNumber, request.FromVendorId, request.companyId);
                logMessageDetail.AppendLine("ValidateVoucherExistsForVendor BL called successfully:");
                responseObj.Status = Response.Success;
                responseObj.LogMessage = logMessageDetail.ToString();
            }
            catch (Exception ex)
            {
                logMessageDetail.AppendLine(ex.Message.ToString().Trim());
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
                responseObj.LogMessage = logMessageDetail.ToString();
            }
            return responseObj;
        }
        public PayableManagementResponse UpdateManualPaymentNumberForCTSIDocuments(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            DataSet manualUpdateDS = null;
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Update payment number manually for the expense records..  ");
                manualUpdateDS = new DataSet();
                if (null != requestObj.PayableDetailsEntity.GetInvoiceLineDetails)
                {
                    poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                    responseObj.IsValidStatus = poIndicatorDataAccess.UpdateManualPaymentNumberForCTSIDocuments(requestObj);
                    if (responseObj.IsValidStatus == 3)
                    {
                        responseObj.Status = Response.Success;
                    }
                    else
                    {
                        responseObj.Status = Response.Error;
                    }
                }
                responseObj.LogMessage = logMessageDetail.ToString();
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }
        public PayableManagementResponse GetFailedCtsiIdsToLinkManualPayments(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Fetch the failed expense records to load into Link Manual Payments Scroll");

                //call the sp to validate and caluclate from datalayer class
                DataSet LinktransactionDs = new DataSet();
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                LinktransactionDs = poIndicatorDataAccess.GetFailedCtsiIdsToLinkManualPayments(requestObj.SearchType, requestObj.FromCtsiId, requestObj.ToCtsiId, requestObj.FromDocumentDate,
                    requestObj.ToDocumentDate, requestObj.FromVendorId, requestObj.ToVendorId, requestObj.companyId);
                if (LinktransactionDs != null && LinktransactionDs.Tables.Count > 0 && LinktransactionDs.Tables[0].Rows.Count > 0)
                {
                    responseObj.PayableDetailsEntity = new PayableDetailsEntity();
                    foreach (DataRow r in LinktransactionDs.Tables[0].Rows)
                    {
                        responseObj.PayableDetailsEntity.AddInvoiceLine(new PayableLineEntity()
                        {
                            CtsiId = (r["CtsiId"] != null && string.IsNullOrEmpty(r["CtsiId"].ToString())) ? string.Empty : r["CtsiId"].ToString().Trim(),
                            GPVoucherNumber = string.IsNullOrEmpty(r["GpVoucherNumber"].ToString()) ? string.Empty : r["GpVoucherNumber"].ToString().Trim(),
                            DocumentDate = Convert.ToDateTime(r["DocumentDate"].ToString()),
                            VendorId = string.IsNullOrEmpty(r["VendorId"].ToString()) ? string.Empty : r["VendorId"].ToString().Trim(),
                            VoucherNumber = string.IsNullOrEmpty(r["DocumentNumber"].ToString()) ? string.Empty : r["DocumentNumber"].ToString().Trim(),
                            TotalApprovedDocumentAmount = Convert.ToDecimal(r["TotalApprovedDocumentAmount"]),
                            StatusId = Convert.ToInt32(r["StatusId"].ToString())
                        });
                    }

                }

                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }
        public PayableManagementResponse VerifyCtsiLookupValueExists(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Verify the updated lookup value exists. ");
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                responseObj.LookupValueExists = poIndicatorDataAccess.VerifyCtsiLookupValueExists(requestObj.SourceLookup, requestObj.LookupValue, requestObj.companyId);
                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }


        #endregion

        #region API Implementation

        public PayableManagementResponse UploadPayableDetailsIntoGpForApi(PayableManagementRequest request)
        {
            PayableManagementResponse payableResponse = null;
            IPayableManagementRepository payableDataAccess = null;
            DataTable validRows = null;
            DataTable inValidRows = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                payableResponse = new PayableManagementResponse();

                DataTable dtInvoiceDetails = new DataTable();
                DataTable dtDistributedDetails = new DataTable();

                if ((request.PoValidationList != null && request.PoValidationList.Count > 0))
                {
                    logMessage.AppendLine(DateTime.Now + " ApiTransactionDtValue ");

                    dtInvoiceDetails = DataTableMapper.POInvoiceDetails(request,
                DataColumnMappings.POInvoiceDetails);
                    logMessage.AppendLine(DateTime.Now + " ApiTransactionDtValue Count : " + dtInvoiceDetails.Rows.Count.ToString());
                }
                if ((request.DuplicationValidationList != null && request.DuplicationValidationList.Count > 0))
                {
                    logMessage.AppendLine(DateTime.Now + " ApiDistributionDtValue ");
                    dtDistributedDetails = DataTableMapper.DistributedDetails(request,
                DataColumnMappings.POInvoiceDetails);
                    logMessage.AppendLine(DateTime.Now + " ApiTransactionDtValue Count : " + dtDistributedDetails.Rows.Count.ToString());
                }

                logMessage.AppendLine(DateTime.Now + " dtInvoiceDetails.Rows.Count " + dtInvoiceDetails.Rows.Count.ToString());
                if (dtInvoiceDetails != null && dtInvoiceDetails.Rows.Count > 0)
                {
                    foreach (DataRow row in dtInvoiceDetails.Rows)
                    {
                        StringBuilder notes = new StringBuilder();
                        // Append string for note array
                        if (request.InvoiceType == 1)
                        {
                            foreach (DataColumn col in dtInvoiceDetails.Columns)
                            {
                                if (col.ColumnName == "DocumentRowID" || col.ColumnName == "FormTypeCode" || col.ColumnName == "DocumentNumber"
                                    || col.ColumnName == "DocumentDate" || col.ColumnName == "VendorNumber"
                                    || col.ColumnName == "DocumentAmount" || col.ColumnName == "ApprovedAmount"
                                    || col.ColumnName == "FreightAmount" || col.ColumnName == "SalesTaxAmount" || col.ColumnName == "POAmount"
                                    || col.ColumnName == "GLIndex"
                                    || col.ColumnName == "TradeDiscountAmount" || col.ColumnName == "MiscellaneousAmount"
                                    || col.ColumnName == "LocationKey" || col.ColumnName == "TaxScheduleId")
                                    notes.Append(row[col].ToString() + "|");
                            }
                        }
                        else
                        {
                            foreach (DataColumn col in dtInvoiceDetails.Columns)
                            {
                                if (col.ColumnName == "DocumentRowID" || col.ColumnName == "FormTypeCode" || col.ColumnName == "DocumentNumber"
                                    || col.ColumnName == "DocumentDate" || col.ColumnName == "VendorNumber"
                                    || col.ColumnName == "DocumentAmount" || col.ColumnName == "ApprovedAmount"
                                    || col.ColumnName == "FreightAmount" || col.ColumnName == "SalesTaxAmount" || col.ColumnName == "MiscellaneousAmount" || col.ColumnName == "PurchasingAmount"
                                    || col.ColumnName == "POAmount" || col.ColumnName == "PONumber" || col.ColumnName == "POLineNumber"
                                    || col.ColumnName == "ItemNumber" || col.ColumnName == "ReceiptNumber" || col.ColumnName == "ReceiptLineNumber"
                                    || col.ColumnName == "ItemUnitQty" || col.ColumnName == "AdjustedItemUnitQty"
                                    || col.ColumnName == "ItemUnitPrice" || col.ColumnName == "ItemExtendedAmount" || col.ColumnName == "AdjustedItemUnitPrice"
                                    || col.ColumnName == "LocationKey" || col.ColumnName == "TaxScheduleId")
                                    notes.Append(row[col].ToString() + "|");
                            }
                        }
                        //remove the pipeline at the end
                        row["Notes"] = notes.Remove(notes.Length - 1, 1);
                    }


                    logMessage.AppendLine(DateTime.Now + " *********Calling eConnect Wrapper service***********");

                    //Callling Api service method
                    _isApi = true;
                    InvoiceType = request.InvoiceType;
                    source = request.SourceFormName;
                    logMessage.AppendLine(DateTime.Now.ToString() + " Parsing the xml string into Data table");
                    apiDS = new DataTable();
                    apiDS = dtInvoiceDetails;   //Assign Transaction Details to api dataset
                    if (string.IsNullOrEmpty(request.UserId))
                        userId = apiDS.Rows[0]["UserId"].ToString();
                    else
                        userId = request.UserId.ToString().Trim();
                    companyId = request.companyId;
                    logMessage.AppendLine(DateTime.Now + " Company Id " + companyId.ToString());

                    if (apiDS.Rows.Count > 0)
                    {
                        logMessage.AppendLine(DateTime.Now.ToString() + " Total records count in the table is : " + apiDS.Rows.Count.ToString());
                        apimMailContent.AppendLine(request.EmailRequest.EmailInformation.Body);
                        apimMailContent.Replace("</table>", "").Replace("</html>", "");

                        //Fetch valid records
                        if (apiDS.Select("StatusId = -1").Count() > 0)
                        {
                            validRows = apiDS.Select("StatusId=-1", "DocumentRowId").CopyToDataTable();
                        }

                        logMessage.AppendLine(DateTime.Now.ToString() + " InvoiceType:" + InvoiceType);
                        logMessage.AppendLine(DateTime.Now.ToString() + " DistributedDetails Record:");

                        //Fetch invalid records and append email body
                        if (string.IsNullOrEmpty(request.SourceFormName))
                        {
                            if (apiDS.Select("StatusId <> -1").Count() > 0)
                            {
                                inValidRows = apiDS.Select("StatusId <> -1").AsEnumerable().OrderBy(row => row.Field<string>("DocumentRowId")).ThenByDescending(row => row.Field<int>("StatusId")).CopyToDataTable();

                                //Insert the invalid rows into mailmessage
                                if (null != inValidRows && inValidRows.Rows.Count > 0)
                                {
                                    //apimMailContent.AppendLine("<tr><td>" + row["DocumentRowId"].ToString() +
                                    //                               "</td></tr>");

                                    //Append the invalid records in mail
                                    logMessage.AppendLine(DateTime.Now.ToString() + " Appending the invalid records in the mail message.");
                                    foreach (DataRow row in inValidRows.Rows)
                                    {
                                        apimMailContent.AppendLine("<tr><td>" + row["DocumentRowId"].ToString() +
                                                                   "</td><td>" + row["DocumentNumber"].ToString() +
                                                                   "</td><td>" + row["VendorNumber"].ToString() +
                                                                   "</td><td>" + row["ErrorDescription"].ToString() +
                                                                    "</td></tr>");
                                    }
                                }
                            }
                        }

                        //The following operation is only for the ReUpload Failure Form to save detaills if any value has been changes.
                        else if (request.SourceFormName.ToLower().Trim() == Configuration.APIReportReUploadFailureForm.Trim())
                        {
                            apiDS.Columns.Remove("Notes");
                            apiDS.Columns.Remove("UserId");
                            apiDS.Columns.Remove("ErrorDescription");

                            payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(request.ConnectionString);
                            payableDataAccess.SaveAPIFileDetailsForReupload(apiDS, userId, companyId, InvoiceType, dtDistributedDetails);
                        }

                        if (validRows != null && validRows.Rows.Count > 0)
                        {
                            DataTable dtTaxDetails = null;
                            if (companyId == Convert.ToInt16(Configuration.EUCompanyId))
                            {
                                var rowsTax = (from p in apiDS.AsEnumerable()
                                               where p.Field<string>("TaxScheduleId").ToString().Trim() != ""
                                               select new
                                               {
                                                   TaxScheduleId = p.Field<string>("TaxScheduleId"),
                                               }).Distinct().OrderByDescending(x => x.TaxScheduleId);

                                if (rowsTax.Count() > 0)
                                {
                                    DataTable taxScheduleTable = new DataTable("@LineDetails");
                                    taxScheduleTable.Columns.Add("TaxScheduleId", typeof(string));

                                    foreach (var item in rowsTax)
                                    {
                                        var row = taxScheduleTable.NewRow();
                                        row["TaxScheduleId"] = item.TaxScheduleId;
                                        taxScheduleTable.Rows.Add(row);
                                    }

                                    payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(request.ConnectionString);
                                    dtTaxDetails = payableDataAccess.GetAPITaxScheduleDetails(taxScheduleTable, companyId);

                                    if (dtTaxDetails != null && dtTaxDetails.Rows.Count > 0)
                                        dtTaxDetails.TableName = "TaxDetails";
                                }
                            }

                            var groupedRows = validRows.AsEnumerable().GroupBy(g => g.Field<string>("DocumentRowId"));
                            foreach (var groupedItem in groupedRows)
                            {
                                StringBuilder notes = new StringBuilder();
                                //Appending the notes details per RecordId
                                groupedItem.AsEnumerable().ToList().ForEach(f => notes.AppendLine((f.Field<string>("Notes")).ToString()));

                                try
                                {
                                    DataTable distributedDetails = null;
                                    if (dtDistributedDetails != null && dtDistributedDetails.Rows.Count > 0)
                                    {
                                        //TODO Convert to list insead of data table
                                        // Fetch current records from distributed table for current document row id
                                        var validDistributedRecords =
                                                        (from u in dtDistributedDetails.AsEnumerable()
                                                         where u.Field<string>("DocumentRowID") == groupedItem.Key.ToString().Trim()
                                                            && u.Field<bool>("RequiredDistribution") == true
                                                         select u).Distinct();

                                        if (validDistributedRecords != null && validDistributedRecords.Count() > 0)
                                        {
                                            distributedDetails = validDistributedRecords.CopyToDataTable<DataRow>();
                                        }
                                    }
                                    //Processing the records ony by one
                                    PushToGP(request, groupedItem.CopyToDataTable(), notes.ToString(), companyId, dtTaxDetails, source, distributedDetails, request.ConnectionString, request.EconnectConString, ref logMessage);

                                }
                                catch (Exception ex)
                                {
                                    logMessage.AppendLine(DateTime.Now.ToString() + ex.Message.ToString());
                                }
                            }
                        }

                        // Sends email if it has any failure records
                        if ((apimMailContent.ToString().Contains("<tr><td>")
                           || (apimMailContent.ToString().Contains("<tr><td colspan=4>")
                            || apimMailContent.ToString().Contains("<tr><td colspan=3>"))))
                        {
                            logMessage.AppendLine(DateTime.Now + " Sending mail.");
                            apimMailContent.Append("</table></html>");

                            logMessage.AppendLine(DateTime.Now + " Email From : " + request.EmailRequest.EmailInformation.EmailFrom.ToString().Trim());
                            logMessage.AppendLine(DateTime.Now + " Email To : " + request.EmailRequest.EmailInformation.EmailTo.ToString().Trim());
                            logMessage.AppendLine(DateTime.Now + " Email BCC : " + request.EmailRequest.EmailInformation.EmailBcc.ToString().Trim());
                            logMessage.AppendLine(DateTime.Now + " Email CC : " + request.EmailRequest.EmailInformation.EmailCc.ToString().Trim());
                            logMessage.AppendLine(DateTime.Now + " Email Subject :" + request.EmailRequest.EmailInformation.Subject + (request.companyId == 1 ? "CHMPT" : "CPEUR"));
                            logMessage.AppendLine(DateTime.Now + " Email Body : " + apimMailContent.ToString());
                            logMessage.AppendLine(DateTime.Now + " Email SMTP Address : " + request.EmailRequest.EmailInformation.SmtpAddress);


                            if (SendMail(apimMailContent.ToString(), request))
                                logMessage.AppendLine(DateTime.Now + " Mail has been sent successfully.");
                            else
                                logMessage.AppendLine(DateTime.Now + " Mail is not sent successfully.");

                            logMessage.AppendLine(DateTime.Now + " ----------------------------------------------------.");
                        }

                        if (apiDS.Select("StatusId <> 0 ").Count() > 0)
                        {
                            payableResponse.ErrorMessage = Configuration.PartialNone;
                        }
                        else if (apiDS.Select("StatusId = 0 ").Count() == validRows.Rows.Count)
                        {
                            payableResponse.ErrorMessage = Configuration.AllPass;
                        }
                    }

                    logMessage.AppendLine(DateTime.Now + " Econnect wrapper service called successfully");
                    payableResponse.LogMessage = logMessage.ToString();
                    payableResponse.Status = Response.Success;
                }
            }
            catch (Exception ex)
            {
                payableResponse.Status = Response.Error;
                logMessage.AppendLine(DateTime.Now.ToString() + ex.ToString());
                payableResponse.LogMessage = logMessage.ToString();
                payableResponse.ErrorMessage = Configuration.PartialNone;

            }
            finally
            {
                payableResponse.LogMessage = logMessage.ToString();
            }

            return payableResponse;
        }

        private bool SendMail(string message, PayableManagementRequest request)
        {
            try
            {
                bool result = false;
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(request.EmailRequest.EmailInformation.EmailFrom);
                mail.To.Add(request.EmailRequest.EmailInformation.EmailTo);
                mail.Bcc.Add(request.EmailRequest.EmailInformation.EmailBcc);
                mail.CC.Add(request.EmailRequest.EmailInformation.EmailCc);
                //if (source == Configuration.SourceApi)
                //    mail.Subject = Configuration.MailApiToGPSubject + companyName;
                //else if (source == Configuration.Source)
                //    mail.Subject = request.EmailRequest.EmailInformation.;
                //else
                //    mail.Subject = Configuration.MailSubject;
                mail.Subject = request.EmailRequest.EmailInformation.Subject + (request.companyId == 1 ? "CHMPT" : "CPEUR");
                mail.IsBodyHtml = true;
                mail.Body = message;

                SmtpClient objSMTP = new SmtpClient(request.EmailRequest.EmailInformation.SmtpAddress);
                //objSMTP.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["FromAddressUserId"], ConfigurationManager.AppSettings["FromAddressPassword"]);
                objSMTP.Send(mail);
                result = true;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //API
        public PayableManagementResponse VerifyApiLookupValueExists(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Verify the updated lookup value exists. ");
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                responseObj.LookupValueExists = poIndicatorDataAccess.VerifyApiLookupValueExists(requestObj.SourceLookup, requestObj.LookupValue, requestObj.companyId);
                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }

        public PayableManagementResponse ValidateAndGetApiTransactions(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            DataTable fileDetailsTable = null;
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Converting the request object into dataset");
                fileDetailsTable = ConvertListToTableForAPIRevalidate(requestObj);

                //call the sp to validate and caluclate from datalayer class
                DataSet validateTransactionDs = new DataSet();
                logMessageDetail.AppendLine(DateTime.Now + " Validating the excel details by calling the validate SP");
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                validateTransactionDs = poIndicatorDataAccess.ValidateAndGetApiTransactions(requestObj.InvoiceType, requestObj.userId, requestObj.companyId, fileDetailsTable, "");

                logMessageDetail.AppendLine(DateTime.Now + " Converting the dataset into response object");
                if (validateTransactionDs.Tables.Count > 0 && validateTransactionDs.Tables[0].Rows.Count > 0)
                {
                    responseObj.PayableDetailsEntity = new PayableDetailsEntity();
                    foreach (DataRow r in validateTransactionDs.Tables[0].Rows)
                    {
                        if (requestObj.InvoiceType == 1)
                        {
                            PayableLineEntity linedetails = new PayableLineEntity()
                            {
                                DocumentIdStatus = string.IsNullOrEmpty(r["Status"].ToString()) ? string.Empty : r["Status"].ToString().Trim(),
                                OriginalApiInvoiceId = Convert.ToInt32(r["OriginalApiInvoiceId"]),
                                DocumentId = string.IsNullOrEmpty(r["DocumentRowId"].ToString()) ? string.Empty : r["DocumentRowId"].ToString().Trim(),
                                VoucherNumber = string.IsNullOrEmpty(r["DocumentNumber"].ToString()) ? string.Empty : r["DocumentNumber"].ToString().Trim(),//string.IsNullOrEmpty(r["GPVoucherNumber"].ToString()) ? string.Empty : r["GPVoucherNumber"].ToString().Trim(),
                                DocumentDate = Convert.ToDateTime(r["DocumentDate"].ToString()),
                                VendorId = string.IsNullOrEmpty(r["VendorNumber"].ToString()) ? string.Empty : r["VendorNumber"].ToString().Trim(),
                                DocumentAmount = Convert.ToDecimal(r["DocumentAmount"]),
                                ApprovedAmount = Convert.ToDecimal(r["ApprovedAmount"]),
                                FreightAmount = Convert.ToDecimal(r["FreightAmount"]),
                                TaxAmount = Convert.ToDecimal(r["SalesTaxAmount"]),
                                PurchaseAmount = Convert.ToDecimal(r["PurchasingAmount"]),
                                TaxScheduleId = string.IsNullOrEmpty(r["TaxScheduleId"].ToString()) ? string.Empty : r["TaxScheduleId"].ToString().Trim(),
                                CurrencyCode = string.IsNullOrEmpty(r["CurrencyId"].ToString()) ? string.Empty : r["CurrencyId"].ToString().Trim(),
                                DocumentTypeName = string.IsNullOrEmpty(r["DocumentType"].ToString()) ? string.Empty : r["DocumentType"].ToString().Trim(),
                                GLAccount = string.IsNullOrEmpty(r["GlAccountNumber"].ToString()) ? string.Empty : r["GlAccountNumber"].ToString().Trim(),
                                GLAccountDescription = string.IsNullOrEmpty(r["GlAccountDescription"].ToString()) ? string.Empty : r["GlAccountDescription"].ToString().Trim(),
                                TradeDiscounts = Convert.ToDecimal(r["TradeDiscountAmount"]),
                                MiscellaneousAmount = Convert.ToDecimal(r["MiscellaneousAmount"]),
                                LocationName = string.IsNullOrEmpty(r["LocationName"].ToString()) ? string.Empty : r["LocationName"].ToString().Trim(),
                                StatusId = Convert.ToInt16(r["StatusId"]),
                                ErrorDescription = string.IsNullOrEmpty(r["ErrorDescription"].ToString()) ? string.Empty : r["ErrorDescription"].ToString().Trim(),
                                GLIndex = Convert.ToInt32(r["GLIndex"]),
                            };

                            responseObj.PayableDetailsEntity.AddInvoiceLine(linedetails);
                        }
                        else
                        {
                            PayableLineEntity linedetails = new PayableLineEntity()
                            {
                                DocumentIdStatus = string.IsNullOrEmpty(r["Status"].ToString()) ? string.Empty : r["Status"].ToString().Trim(),
                                OriginalApiInvoiceId = Convert.ToInt32(r["OriginalApiInvoiceId"]),
                                DocumentId = string.IsNullOrEmpty(r["DocumentRowId"].ToString()) ? string.Empty : r["DocumentRowId"].ToString().Trim(),
                                DocumentNumber = string.IsNullOrEmpty(r["DocumentNumber"].ToString()) ? string.Empty : r["DocumentNumber"].ToString().Trim(),
                                ReceiptDate = Convert.ToDateTime(r["DocumentDate"].ToString()),
                                VendorId = string.IsNullOrEmpty(r["VendorNumber"].ToString()) ? string.Empty : r["VendorNumber"].ToString().Trim(),
                                DocumentAmount = Convert.ToDecimal(r["DocumentAmount"]),
                                ApprovedAmount = Convert.ToDecimal(r["ApprovedAmount"]),
                                FreightAmount = Convert.ToDecimal(r["FreightAmount"]),
                                TaxAmount = Convert.ToDecimal(r["SalesTaxAmount"]),
                                MiscellaneousAmount = Convert.ToDecimal(r["MiscellaneousAmount"]),
                                PurchaseAmount = Convert.ToDecimal(r["POAmount"]),
                                CurrencyCode = string.IsNullOrEmpty(r["CurrencyId"].ToString()) ? string.Empty : r["CurrencyId"].ToString().Trim(),
                                DocumentTypeName = string.IsNullOrEmpty(r["DocumentType"].ToString()) ? string.Empty : r["DocumentType"].ToString().Trim(),
                                PurchaseOrderNumber = string.IsNullOrEmpty(r["PONumber"].ToString()) ? string.Empty : r["PONumber"].ToString().Trim(),
                                POLineNumber = Convert.ToInt32(r["POLineNumber"]),
                                ItemNumber = string.IsNullOrEmpty(r["ItemNumber"].ToString()) ? string.Empty : r["ItemNumber"].ToString().Trim(),
                                ReceiptNumber = string.IsNullOrEmpty(r["ReceiptNumber"].ToString()) ? string.Empty : r["ReceiptNumber"].ToString().Trim(),
                                ReceiptLineNumber = Convert.ToInt32(r["ReceiptLineNumber"]),
                                QuantityShipped = Convert.ToDecimal(r["ItemUnitQty"]),
                                AdjustedItemUnitQuantity = Convert.ToDecimal(r["AdjustedItemUnitQty"]),
                                UnitCost = Convert.ToDecimal(r["ItemUnitPrice"]),
                                ExtendedCost = Convert.ToDecimal(r["ItemExtendedAmount"]),
                                AdjustedItemUnitPrice = Convert.ToDecimal(r["AdjustedItemUnitPrice"]),
                                GLAccount = string.IsNullOrEmpty(r["GlAccountNumber"].ToString()) ? string.Empty : r["GlAccountNumber"].ToString().Trim(),
                                TaxScheduleId = string.IsNullOrEmpty(r["TaxScheduleId"].ToString()) ? string.Empty : r["TaxScheduleId"].ToString().Trim(),
                                LocationName = string.IsNullOrEmpty(r["LocationName"].ToString()) ? string.Empty : r["LocationName"].ToString().Trim(),
                                StatusId = Convert.ToInt16(r["StatusId"]),
                                ErrorDescription = string.IsNullOrEmpty(r["ErrorDescription"].ToString()) ? string.Empty : r["ErrorDescription"].ToString().Trim(),
                                GLIndex = Convert.ToInt32(r["GLIndex"]),
                            };
                            responseObj.PayableDetailsEntity.AddInvoiceLine(linedetails);
                        }
                    }
                }
                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }
        public static DataTable ConvertListToTableForAPIRevalidate(PayableManagementRequest requestObj)
        {
            DataTable lineDetailsTable = null;
            if (requestObj.InvoiceType == 1)
            {
                lineDetailsTable = new DataTable("@APINonPOTransactionsReceived");

                //lineDetailsTable.Columns.Add("ApiInvoiceId", typeof(int));
                lineDetailsTable.Columns.Add("OriginalApiInvoiceId", typeof(int));
                lineDetailsTable.Columns.Add("DocumentRowId", typeof(string));
                lineDetailsTable.Columns.Add("FormTypeCode", typeof(int));
                lineDetailsTable.Columns.Add("DocumentNumber", typeof(string));
                lineDetailsTable.Columns.Add("DocumentDate", typeof(DateTime));
                lineDetailsTable.Columns.Add("VendorNumber", typeof(string));
                lineDetailsTable.Columns.Add("DocumentAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("ApprovedAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("FreightAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("SalesTaxAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("PurchaseAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("CurrencyId", typeof(string));
                lineDetailsTable.Columns.Add("GLAccount", typeof(string));
                lineDetailsTable.Columns.Add("GLAccountDescription", typeof(string));
                lineDetailsTable.Columns.Add("TradeDiscountAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("MiscellaneousAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("TaxScheduleId", typeof(string));
                lineDetailsTable.Columns.Add("LocationName", typeof(string));
                lineDetailsTable.Columns.Add("GLIndex", typeof(int));

                foreach (var data in requestObj.PayableDetailsEntity.GetInvoiceLineDetails)
                {
                    var row = lineDetailsTable.NewRow();
                    //row["ApiInvoiceId"] = 0;
                    row["OriginalApiInvoiceId"] = data.OriginalApiInvoiceId;
                    row["DocumentRowId"] = data.DocumentId;
                    row["FormTypeCode"] = ConvertDocTypeValueByInvoiceType(requestObj.InvoiceType, data.DocumentTypeName);
                    row["DocumentNumber"] = data.VoucherNumber;
                    row["DocumentDate"] = data.DocumentDate;
                    row["VendorNumber"] = data.VendorId;
                    row["DocumentAmount"] = data.DocumentAmount;
                    row["ApprovedAmount"] = data.ApprovedAmount;
                    row["FreightAmount"] = data.FreightAmount;
                    row["SalesTaxAmount"] = data.TaxAmount;
                    row["PurchaseAmount"] = data.PurchaseAmount;
                    row["CurrencyId"] = data.CurrencyCode;
                    row["GLAccount"] = data.GLAccount;
                    row["GLAccountDescription"] = data.GLAccountDescription;
                    row["TradeDiscountAmount"] = data.TradeDiscounts;
                    row["MiscellaneousAmount"] = data.MiscellaneousAmount;
                    row["TaxScheduleId"] = data.TaxScheduleId;
                    row["LocationName"] = data.LocationName;
                    row["GLIndex"] = data.GLIndex;

                    lineDetailsTable.Rows.Add(row);
                }
            }
            else
            {
                lineDetailsTable = new DataTable("@APIPOTransactionsReceived");

                //lineDetailsTable.Columns.Add("ApiInvoiceId", typeof(int));
                lineDetailsTable.Columns.Add("OriginalApiInvoiceId", typeof(int));
                lineDetailsTable.Columns.Add("DocumentRowId", typeof(string));
                lineDetailsTable.Columns.Add("FormTypeCode", typeof(int));
                lineDetailsTable.Columns.Add("DocumentNumber", typeof(string));
                lineDetailsTable.Columns.Add("DocumentDate", typeof(DateTime));
                lineDetailsTable.Columns.Add("VendorNumber", typeof(string));
                lineDetailsTable.Columns.Add("DocumentAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("ApprovedAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("FreightAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("SalesTaxAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("MiscellaneousAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("POAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("PONumber", typeof(string));
                lineDetailsTable.Columns.Add("CurrencyId", typeof(string));
                lineDetailsTable.Columns.Add("POLineNumber", typeof(int));
                lineDetailsTable.Columns.Add("ItemNumber", typeof(string));
                lineDetailsTable.Columns.Add("ReceiptNumber", typeof(string));
                lineDetailsTable.Columns.Add("ReceiptLineNumber", typeof(int));
                lineDetailsTable.Columns.Add("ItemUnitQty", typeof(decimal));
                lineDetailsTable.Columns.Add("AdjustedItemUnitQty", typeof(decimal));
                lineDetailsTable.Columns.Add("ItemUnitPrice", typeof(decimal));
                lineDetailsTable.Columns.Add("ItemExtendedAmount", typeof(decimal));
                lineDetailsTable.Columns.Add("AdjustedItemUnitPrice", typeof(decimal));
                lineDetailsTable.Columns.Add("GlAccountNumber", typeof(string));
                lineDetailsTable.Columns.Add("TaxScheduleId", typeof(string));
                lineDetailsTable.Columns.Add("LocationName", typeof(string));
                lineDetailsTable.Columns.Add("GLIndex", typeof(string));

                foreach (var data in requestObj.PayableDetailsEntity.GetInvoiceLineDetails)
                {
                    var row = lineDetailsTable.NewRow();

                    //row["ApiInvoiceId"] = 0;
                    row["OriginalAPIInvoiceId"] = data.OriginalApiInvoiceId;
                    row["DocumentRowId"] = data.DocumentId;
                    row["FormTypeCode"] = ConvertDocTypeValueByInvoiceType(requestObj.InvoiceType, data.DocumentTypeName);
                    row["DocumentNumber"] = data.DocumentNumber;
                    row["DocumentDate"] = data.ReceiptDate;
                    row["VendorNumber"] = data.VendorId;
                    row["DocumentAmount"] = data.DocumentAmount;
                    row["ApprovedAmount"] = data.ApprovedAmount;
                    row["FreightAmount"] = data.FreightAmount;
                    row["SalesTaxAmount"] = data.TaxAmount;
                    row["MiscellaneousAmount"] = data.MiscellaneousAmount;
                    row["POAmount"] = data.PurchaseAmount;
                    row["PONumber"] = data.PurchaseOrderNumber;
                    row["CurrencyId"] = data.CurrencyCode;
                    row["POLineNumber"] = data.POLineNumber;
                    row["ItemNumber"] = data.ItemNumber;
                    row["ReceiptNumber"] = data.ReceiptNumber;
                    row["ReceiptLineNumber"] = data.ReceiptLineNumber;
                    row["ItemUnitQty"] = data.QuantityShipped;
                    row["AdjustedItemUnitQty"] = data.AdjustedItemUnitQuantity;
                    row["ItemUnitPrice"] = data.UnitCost;
                    row["ItemExtendedAmount"] = data.ExtendedCost;
                    row["AdjustedItemUnitPrice"] = data.AdjustedItemUnitPrice;
                    row["GlAccountNumber"] = data.GLAccount;
                    row["TaxScheduleId"] = data.TaxScheduleId;
                    row["LocationName"] = data.LocationName;
                    row["GLIndex"] = data.GLIndex;


                    lineDetailsTable.Rows.Add(row);
                }
            }
            return lineDetailsTable;
        }
        public static int ConvertDocTypeValueByInvoiceType(int invType, string docTypeName)
        {
            int docType = 1;
            if (invType == 1)
            {
                if (docTypeName == "INV")
                {
                    docType = 1;
                }
                else if (docTypeName == "CRM")
                {
                    docType = 3;
                }
            }
            else
            {
                docType = 2;
            }
            return docType;
        }
        public PayableManagementResponse GetApiDuplicateDocumentRowDetails(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            try
            {
                responseObj = new PayableManagementResponse();
                DataSet duplicateDocumentRowDetails = new DataSet();
                //TODO Convert to list insead of data table
                // Fetch current records only from distributed table
                DataTable DistributedDetailsDT = new DataTable();
                DataTable TransactionDT = new DataTable();

                TransactionDT = DataTableMapper.POInvoiceDetails(requestObj,
               DataColumnMappings.POInvoiceDetails);

                fileDetailsTable = TransactionDT.Select().CopyToDataTable().DefaultView.ToTable(true, "DocumentRowID");
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                duplicateDocumentRowDetails = poIndicatorDataAccess.GetApiDuplicateDocumentRowDetails(requestObj.userId, requestObj.companyId, fileDetailsTable);
                if (duplicateDocumentRowDetails.Tables.Count > 0 && duplicateDocumentRowDetails.Tables[0].Rows.Count > 0)
                {
                    responseObj.PayableDetailsEntity = new PayableDetailsEntity();
                    //responseObj.ApiDistributionDtValue = ConvertDataTableToString(duplicateDocumentRowDetails.Tables[0]);
                    responseObj.DuplicationValidationList = GetAllEntities<PayableLineEntity, PayableManagementAPIMap>(duplicateDocumentRowDetails.Tables[0]).ToList();
                }
                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }
        private DataTable ConvertStringToDataTable(string data)
        {
            DataTable dataTable = new DataTable();
            bool columnsAdded = false;
            foreach (string row in data.Split('#'))
            {
                DataRow dataRow = dataTable.NewRow();
                foreach (string cell in row.Split('|'))
                {
                    string[] keyValue = cell.Split('~');
                    if (!columnsAdded)
                    {
                        DataColumn dataColumn = new DataColumn(keyValue[0]);
                        dataTable.Columns.Add(dataColumn);
                    }
                    dataRow[keyValue[0]] = keyValue[1];
                }
                columnsAdded = true;
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        public PayableManagementResponse GetFailedAPIIdsList(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Fetch the failed api records to load into Lookup ");
                //call the sp to datalayer class
                DataSet APIIdLookupDs = new DataSet();
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                APIIdLookupDs = poIndicatorDataAccess.GetFailedAPIIdsList(requestObj.InvoiceType, requestObj.companyId);
                if (APIIdLookupDs.Tables.Count > 0 && APIIdLookupDs.Tables[0].Rows.Count > 0)
                {
                    responseObj.LookupDetails = new PayableLookupDetailEntity();
                    foreach (DataRow r in APIIdLookupDs.Tables[0].Rows)
                    {
                        responseObj.LookupDetails.AddLookupLine(new PayableLookupLineEntity()
                        {
                            ApiId = string.IsNullOrEmpty(r["DocumentRowId"].ToString()) ? string.Empty : r["DocumentRowId"].ToString().Trim(),
                        });
                    }
                }
                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }

        public PayableManagementResponse UpdateManualPaymentNumberForAPIDocuments(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            DataSet manualUpdateDS = null;
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Update payment number manually for the API records..  ");
                manualUpdateDS = new DataSet();
                if (null != requestObj.PayableDetailsEntity.GetInvoiceLineDetails)
                {
                    poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                    responseObj.IsValidStatus = poIndicatorDataAccess.UpdateManualPaymentNumberForAPIDocuments(requestObj);
                    if (responseObj.IsValidStatus == 3)
                    {
                        responseObj.Status = Response.Success;
                    }
                    else
                    {
                        responseObj.Status = Response.Error;
                    }
                }
                responseObj.LogMessage = logMessageDetail.ToString();
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }

        public PayableManagementResponse GetFailedApiIdsToLinkManualPayments(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Update payment number manually for the API records..  ");
                logMessageDetail.AppendLine(DateTime.Now + " Fetch the failed API records to load into Link Manual Payments Scroll");

                //call the sp to validate and calculate from datalayer class
                DataSet LinktransactionDs = new DataSet();
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                LinktransactionDs = poIndicatorDataAccess.GetFailedApiIdsToLinkManualPayments(requestObj.InvoiceType, requestObj.SearchType, requestObj.FromApiId, requestObj.ToApiId, requestObj.FromVendorId, requestObj.ToVendorId, requestObj.companyId);
                if (LinktransactionDs != null && LinktransactionDs.Tables.Count > 0 && LinktransactionDs.Tables[0].Rows.Count > 0)
                {
                    responseObj.PayableDetailsEntity = new PayableDetailsEntity();
                    if (requestObj.InvoiceType == 1)//Non PO
                    {
                        foreach (DataRow r in LinktransactionDs.Tables[0].Rows)
                        {
                            responseObj.PayableDetailsEntity.AddInvoiceLine(new PayableLineEntity()
                            {
                                DocumentRowId = (r["DocumentRowId"] != null && string.IsNullOrEmpty(r["DocumentRowId"].ToString())) ? string.Empty : r["DocumentRowId"].ToString().Trim(),
                                GPVoucherNumber = string.IsNullOrEmpty(r["GpVoucherNumber"].ToString()) ? string.Empty : r["GpVoucherNumber"].ToString().Trim(),
                                DocumentDate = Convert.ToDateTime(r["DocumentDate"]),
                                VendorId = string.IsNullOrEmpty(r["VendorNumber"].ToString()) ? string.Empty : r["VendorNumber"].ToString().Trim(),
                                DocumentNumber = string.IsNullOrEmpty(r["DocumentNumber"].ToString()) ? string.Empty : r["DocumentNumber"].ToString().Trim(),
                                DocumentAmount = Convert.ToDecimal(r["DocumentAmount"]),
                                StatusId = Convert.ToInt32(r["StatusId"].ToString())
                            });
                        }
                    }
                    else if (requestObj.InvoiceType == 2)//Po invoice
                    {
                        foreach (DataRow r in LinktransactionDs.Tables[0].Rows)
                        {
                            responseObj.PayableDetailsEntity.AddInvoiceLine(new PayableLineEntity()
                            {
                                DocumentRowId = (r["DocumentRowId"] != null && string.IsNullOrEmpty(r["DocumentRowId"].ToString())) ? string.Empty : r["DocumentRowId"].ToString().Trim(),
                                GPVoucherNumber = string.IsNullOrEmpty(r["GpVoucherNumber"].ToString()) ? string.Empty : r["GpVoucherNumber"].ToString().Trim(),
                                ReceiptDate = Convert.ToDateTime(r["DocumentDate"]),
                                VendorId = string.IsNullOrEmpty(r["VendorNumber"].ToString()) ? string.Empty : r["VendorNumber"].ToString().Trim(),
                                DocumentNumber = string.IsNullOrEmpty(r["DocumentNumber"].ToString()) ? string.Empty : r["DocumentNumber"].ToString().Trim(),
                                DocumentAmount = Convert.ToDecimal(r["DocumentAmount"]),
                                StatusId = Convert.ToInt32(r["StatusId"].ToString())
                            });
                        }
                    }
                }
                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }
        public PayableManagementResponse ValidatePODocumentExistsForVendor(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Verify the updated document value exists. ");
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                responseObj.IsValid = poIndicatorDataAccess.ValidatePODocumentExistsForVendor(requestObj.ManualPaymentNumber, requestObj.FromVendorId, requestObj.companyId);
                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }
        public PayableManagementResponse ValidateNonPODocumentExistsForVendor(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Verify the updated document value exists. ");
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                responseObj.IsValid = poIndicatorDataAccess.ValidateNonPODocumentExistsForVendor(requestObj.ManualPaymentNumber, requestObj.FromVendorId, requestObj.companyId);
                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }

        #endregion



        public PayableManagementResponse VerifyLookupValueExists(PayableManagementRequest request)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + "Verifying Lookup Value Exists");
                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(request.ConnectionString);
                responseObj.LookupValueExists = poIndicatorDataAccess.VerifyLookupValueExists(request.SourceLookup, request.LookupValue, request.companyId);
                logMessageDetail.AppendLine(DateTime.Now + "Lookup Value Exists in DB");
                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }


        public PayableManagementResponse GetFailedApiDocuments(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository poIndicatorDataAccess = null;
            StringBuilder logMessageDetail = new StringBuilder();
            try
            {
                responseObj = new PayableManagementResponse();
                logMessageDetail.AppendLine(DateTime.Now + " Converting the request object into dataset");
                logMessageDetail.AppendLine(DateTime.Now + " Fetch the failed API records to load into Re-Upload Failed Documents Scroll");

                //call the sp to validate and caluclate from datalayer class
                DataSet transactionDs = new DataSet();

                poIndicatorDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                transactionDs = poIndicatorDataAccess.GetFailedApiDocuments(requestObj.InvoiceType, requestObj.SearchType, requestObj.FromApiId, requestObj.ToApiId, requestObj.FromDocumentDate, requestObj.ToDocumentDate, requestObj.FromVendorId, requestObj.ToVendorId, requestObj.companyId);
                if (transactionDs.Tables.Count > 0 && transactionDs.Tables[0].Rows.Count > 0)
                {
                    responseObj.PayableDetailsEntity = new PayableDetailsEntity();
                    foreach (DataRow r in transactionDs.Tables[0].Rows)
                    {
                        if (requestObj.InvoiceType == 1)
                        {
                            PayableLineEntity linedetails = new PayableLineEntity()
                            {
                                OriginalApiInvoiceId = Convert.ToInt32(requestObj.companyId == 1 ? r["NAOriginalApiInvoiceId"] : r["EUOriginalApiInvoiceId"]),
                                DocumentId = string.IsNullOrEmpty(r["DocumentRowId"].ToString()) ? string.Empty : r["DocumentRowId"].ToString().Trim(),
                                VoucherNumber = string.IsNullOrEmpty(r["VoucherNumber"].ToString()) ? string.Empty : r["VoucherNumber"].ToString().Trim(),
                                DocumentDate = Convert.ToDateTime(r["DocumentDate"]),
                                VendorId = string.IsNullOrEmpty(r["VendorNumber"].ToString()) ? string.Empty : r["VendorNumber"].ToString().Trim(),
                                DocumentAmount = Convert.ToDecimal(r["DocumentAmount"]),
                                ApprovedAmount = Convert.ToDecimal(r["ApprovedAmount"]),
                                FreightAmount = Convert.ToDecimal(r["FreightAmount"]),
                                TaxAmount = Convert.ToDecimal(r["SalesTaxAmount"]),
                                PurchaseAmount = Convert.ToDecimal(r["PurchasingAmount"]),
                                CurrencyCode = string.IsNullOrEmpty(r["CurrencyId"].ToString()) ? string.Empty : r["CurrencyId"].ToString().Trim(),
                                DocumentTypeName = string.IsNullOrEmpty(r["DocumentType"].ToString()) ? string.Empty : r["DocumentType"].ToString().Trim(),
                                GLAccount = string.IsNullOrEmpty(r["GlAccountNumber"].ToString()) ? string.Empty : r["GlAccountNumber"].ToString().Trim(),
                                GLAccountDescription = string.IsNullOrEmpty(r["GlAccountDescription"].ToString()) ? string.Empty : r["GlAccountDescription"].ToString().Trim(),
                                TradeDiscounts = Convert.ToDecimal(r["TradeDiscountAmount"]),
                                MiscellaneousAmount = Convert.ToDecimal(r["MiscellaneousAmount"]),
                                TaxScheduleId = string.IsNullOrEmpty(r["TaxScheduleId"].ToString()) ? string.Empty : r["TaxScheduleId"].ToString().Trim(),
                                LocationName = string.IsNullOrEmpty(r["LocationName"].ToString()) ? string.Empty : r["LocationName"].ToString().Trim(),
                                GLIndex = Convert.ToInt32(r["GLIndex"]),
                            };
                            responseObj.PayableDetailsEntity.AddInvoiceLine(linedetails);
                        }
                        else
                        {
                            PayableLineEntity linedetails = new PayableLineEntity()
                            {
                                OriginalApiInvoiceId = Convert.ToInt32(requestObj.companyId == 1 ? r["NAOriginalApiInvoiceId"] : r["EUOriginalApiInvoiceId"]),
                                DocumentId = string.IsNullOrEmpty(r["DocumentRowId"].ToString()) ? string.Empty : r["DocumentRowId"].ToString().Trim(),
                                DocumentNumber = string.IsNullOrEmpty(r["DocumentNumber"].ToString()) ? string.Empty : r["DocumentNumber"].ToString().Trim(),
                                ReceiptDate = Convert.ToDateTime(r["DocumentDate"]),
                                VendorId = string.IsNullOrEmpty(r["VendorNumber"].ToString()) ? string.Empty : r["VendorNumber"].ToString().Trim(),
                                DocumentAmount = Convert.ToDecimal(r["DocumentAmount"]),
                                ApprovedAmount = Convert.ToDecimal(r["ApprovedAmount"]),
                                FreightAmount = Convert.ToDecimal(r["FreightAmount"]),
                                TaxAmount = Convert.ToDecimal(r["SalesTaxAmount"]),
                                MiscellaneousAmount = Convert.ToDecimal(r["MiscellaneousAmount"]),
                                PurchaseAmount = Convert.ToDecimal(r["POAmount"]),
                                PurchaseOrderNumber = string.IsNullOrEmpty(r["PONumber"].ToString()) ? string.Empty : r["PONumber"].ToString().Trim(),
                                CurrencyCode = string.IsNullOrEmpty(r["CurrencyId"].ToString()) ? string.Empty : r["CurrencyId"].ToString().Trim(),
                                DocumentTypeName = string.IsNullOrEmpty(r["DocumentType"].ToString()) ? string.Empty : r["DocumentType"].ToString().Trim(),
                                POLineNumber = Convert.ToInt32(r["POLineNumber"]),
                                ItemNumber = string.IsNullOrEmpty(r["ItemNumber"].ToString()) ? string.Empty : r["ItemNumber"].ToString().Trim(),
                                ReceiptNumber = string.IsNullOrEmpty(r["ReceiptNumber"].ToString()) ? string.Empty : r["ReceiptNumber"].ToString().Trim(),
                                ReceiptLineNumber = Convert.ToInt32(r["ReceiptLineNumber"]),
                                QuantityShipped = Convert.ToDecimal(r["ItemUnitQty"]),
                                AdjustedItemUnitQuantity = Convert.ToDecimal(r["AdjustedItemUnitQty"]),
                                UnitCost = Convert.ToDecimal(r["ItemUnitPrice"]),
                                ExtendedCost = Convert.ToDecimal(r["ItemExtendedAmount"]),
                                AdjustedItemUnitPrice = Convert.ToDecimal(r["AdjustedItemUnitPrice"]),
                                GLAccount = string.IsNullOrEmpty(r["GlAccountNumber"].ToString()) ? string.Empty : r["GlAccountNumber"].ToString().Trim(),
                                TaxScheduleId = string.IsNullOrEmpty(r["TaxScheduleId"].ToString()) ? string.Empty : r["TaxScheduleId"].ToString().Trim(),
                                LocationName = string.IsNullOrEmpty(r["LocationKey"].ToString()) ? string.Empty : r["LocationKey"].ToString().Trim(),
                                GLIndex = Convert.ToInt32(r["GLIndex"])
                            };
                            responseObj.PayableDetailsEntity.AddInvoiceLine(linedetails);
                        }
                    }
                }

                responseObj.LogMessage = logMessageDetail.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }

        private static string ConvertDataSetToString(DataSet dataSet)
        {
            string data = string.Empty;
            int totCount = dataSet.Tables.Count;

            for (int count = 0; count < totCount; count++)
            {
                int rowsCount = dataSet.Tables[count].Rows.Count;
                for (int rowValue = 0; rowValue < rowsCount; rowValue++)
                {
                    DataRow row = dataSet.Tables[count].Rows[rowValue];
                    int columnsCount = dataSet.Tables[count].Columns.Count;
                    for (int colValue = 0; colValue < columnsCount; colValue++)
                    {
                        data += dataSet.Tables[count].Columns[colValue].ColumnName + "~" + row[colValue];
                        if (colValue == columnsCount - 1)
                        {
                            if (rowValue != (rowsCount - 1))
                                data += "#";
                        }
                        else
                            data += "|";
                    }
                }
            }
            return data;
        }

        #endregion

        #region APIFileGenerator

        public PayableManagementResponse GetAPIFiles(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository payableDataAccess = null;
            FTPRequest fTPRequest = null;
            FTPResponse fTPResponse = null;
            FtpBL ftpBL = null;
            FTPEntity fTPEntity = null;
            try
            {
                responseObj = new PayableManagementResponse();
                DataSet apiTables = new DataSet();
                logMessage = new StringBuilder();
                AppendLogMessage("------------------------------------------------------------------------------------------");
                AppendLogMessage("Retrieving daily updates in data.");

                payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                apiTables = payableDataAccess.GetAPIFiles(requestObj.CutofMonth, requestObj.companyId);

                if (apiTables.Tables.Count > 0)
                {
                    #region Preparing Text
                    // vendor file
                    AppendLogMessage("Fetching Vendor Details.");
                    StringBuilder vendorFile = new StringBuilder();
                    foreach (DataRow row in apiTables.Tables[0].Rows)
                    {
                        string rowData = string.Empty;
                        foreach (DataColumn col in apiTables.Tables[0].Columns)
                        {
                            rowData += row[col.ColumnName].ToString().Trim() + "|";
                        }
                        vendorFile.AppendLine(rowData.Substring(0, rowData.Length - 1));
                    }

                    // Location/Router file
                    AppendLogMessage("Fetching Location Details.");
                    StringBuilder locFile = new StringBuilder();
                    foreach (DataRow row in apiTables.Tables[1].Rows)
                    {
                        string rowData = string.Empty;
                        foreach (DataColumn col in apiTables.Tables[1].Columns)
                        {
                            rowData += row[col.ColumnName].ToString().Trim() + "|";
                        }
                        locFile.AppendLine(rowData.Substring(0, rowData.Length - 1));
                    }



                    // GL File
                    AppendLogMessage("Fetching GL Details.");
                    StringBuilder glFile = new StringBuilder();
                    foreach (DataRow row in apiTables.Tables[2].Rows)
                    {
                        string rowData = string.Empty;
                        foreach (DataColumn col in apiTables.Tables[2].Columns)
                        {
                            rowData += row[col.ColumnName].ToString().Trim() + "|";
                        }
                        glFile.AppendLine(rowData.Substring(0, rowData.Length - 1));
                    }

                    // PO File
                    AppendLogMessage("Fetching PO Details.");
                    StringBuilder poFile = new StringBuilder();
                    foreach (DataRow row in apiTables.Tables[3].Rows)
                    {
                        string rowData = string.Empty;
                        foreach (DataColumn col in apiTables.Tables[3].Columns)
                        {
                            rowData += row[col.ColumnName].ToString().Trim() + "|";
                        }
                        poFile.AppendLine(rowData.Substring(0, rowData.Length - 1));

                        foreach (DataRow poLineRow in apiTables.Tables[4].Rows)
                        {
                            rowData = string.Empty;
                            if (poLineRow["po Number"].ToString().Trim() == row["po Number"].ToString().Trim())
                            {
                                foreach (DataColumn col in apiTables.Tables[4].Columns)
                                {
                                    rowData += poLineRow[col.ColumnName].ToString().Trim() + "|";
                                }
                                poFile.AppendLine(rowData.Substring(0, rowData.Length - 1));
                            }
                        }
                    }
                    StringBuilder payFile = new StringBuilder();
                    if (requestObj.companyId == Convert.ToInt16(Configuration.NACompanyId))
                    {
                        // Payment File
                        AppendLogMessage("Fetching Payment Details.");

                        foreach (DataRow row in apiTables.Tables[7].Rows)
                        {
                            string rowData = string.Empty;
                            foreach (DataColumn col in apiTables.Tables[7].Columns)
                            {
                                rowData += row[col.ColumnName].ToString().Trim() + "|";
                            }
                            payFile.AppendLine(rowData.Substring(0, rowData.Length - 1));
                        }
                    }

                    // Receiving File
                    AppendLogMessage("Fetching Receiving Details.");
                    StringBuilder rcvFile = new StringBuilder();
                    foreach (DataRow row in apiTables.Tables[5].Rows)
                    {
                        string rowData = string.Empty;
                        foreach (DataColumn col in apiTables.Tables[5].Columns)
                        {
                            rowData += row[col.ColumnName].ToString().Trim() + "|";
                        }
                        rcvFile.AppendLine(rowData.Substring(0, rowData.Length - 1));

                        foreach (DataRow rcvRow in apiTables.Tables[6].Rows)
                        {
                            rowData = string.Empty;
                            if (rcvRow["ReceiverNumber"].ToString().Trim() == row["ReceiverNumber"].ToString().Trim())
                            {
                                foreach (DataColumn col in apiTables.Tables[6].Columns)
                                {
                                    rowData += rcvRow[col.ColumnName].ToString().Trim() + "|";
                                }
                                rcvFile.AppendLine(rowData.Substring(0, rowData.Length - 1));
                            }
                        }
                    }
                    #endregion

                    string path = requestObj.OutputFilePath.Trim();
                    int fileCount = 0;

                    //Saving to corresponding files
                    AppendLogMessage("Saving all the details into corresponding files.");

                    if (vendorFile.Length > 0)
                    {
                        this.LogInformationToFile(path, vendorFile.ToString(), requestObj.VendorFileName.Trim(), requestObj.companyId, 1);
                        fileCount += 1;
                    }
                    if (locFile.Length > 0)
                    {
                        this.LogInformationToFile(path, locFile.ToString(), requestObj.LocFileName.Trim(), requestObj.companyId, 1);
                        fileCount += 1;
                    }
                    if (glFile.Length > 0)
                    {
                        this.LogInformationToFile(path, glFile.ToString(), requestObj.GlFileName.Trim(), requestObj.companyId, 1);
                        fileCount += 1;
                    }
                    if (poFile.Length > 0)
                    {
                        this.LogInformationToFile(path, poFile.ToString(), requestObj.PoFileName.Trim(), requestObj.companyId, 1);
                        fileCount += 1;
                    }
                    if (requestObj.companyId == Convert.ToInt16(Configuration.NACompanyId))
                    {
                        if (payFile.Length > 0)
                        {
                            this.LogInformationToFile(path, payFile.ToString(), requestObj.PayFileName.Trim(), requestObj.companyId, 1);
                            fileCount += 1;
                        }
                    }
                    if (rcvFile.Length > 0)
                    {
                        this.LogInformationToFile(path, rcvFile.ToString(), requestObj.RcvFileName.Trim(), requestObj.companyId, 1);
                        fileCount += 1;
                    }


                    string outPath = path;
                    bool isSuccessfullyZipped = false;
                    string zipFileName = string.Empty;
                    string outputPathAndFile = string.Empty;
                    bool isZipCreated = false;
                    bool isZipFileMoved = false;
                    if (fileCount > 0)
                    {
                        //Zipping the files
                        #region ZippingFiles

                        // gets the Zip file
                        AppendLogMessage("Zipping the files.");


                        if (requestObj.companyId == Convert.ToInt16(Configuration.NACompanyId))
                        {
                            outputPathAndFile = Configuration.OutputNAFileName.Trim();
                        }
                        else if (requestObj.companyId == Convert.ToInt16(Configuration.EUCompanyId))
                        {
                            outputPathAndFile = Configuration.OutputEUFileName.Trim();
                        }

                        zipFileName = outputPathAndFile + date + ".zip";

                        fTPRequest = new FTPRequest();
                        fTPResponse = new FTPResponse();
                        ftpBL = new FtpBL();
                        fTPEntity = new FTPEntity();
                        fTPEntity.LoggingFilePath = outPath;
                        fTPEntity.FileName = zipFileName;
                        fTPRequest.objFTPEntity = fTPEntity;
                        fTPResponse = ftpBL.GenerateZipFile(fTPRequest);

                        if (fTPResponse.ErrorStatus == Configuration.FtpSuccess)
                        {
                            AppendLogMessage(fTPResponse.logMessage.ToString());
                            isZipCreated = true;
                        }
                        else
                        {
                            AppendLogMessage("Error on zipping the file.");
                        }
                        #endregion
                    }
                    else
                    {
                        AppendLogMessage("No files to zip.");

                    }


                    #region MoveZipFileToTargetFolder
                    if (isZipCreated)
                    {
                        string destinationPath = requestObj.GPToAPITempFolderPath.Trim();

                        fTPRequest = new FTPRequest();
                        fTPResponse = new FTPResponse();
                        ftpBL = new FtpBL();
                        fTPEntity = new FTPEntity();
                        fTPEntity.SourcePath = path;
                        fTPEntity.DestinationPath = destinationPath;
                        fTPEntity.FileName = zipFileName;
                        //fTPRequest.objFTPEntity.SourcePath = path;
                        //fTPRequest.objFTPEntity.DestinationPath = destinationPath;
                        //fTPRequest.objFTPEntity.FileName = zipFileName;
                        fTPRequest.objFTPEntity = fTPEntity;
                        fTPResponse = ftpBL.MoveZipFileToTargetFolder(fTPRequest);
                        if (fTPResponse.ErrorStatus == Configuration.FtpSuccess)
                        {
                            AppendLogMessage(fTPResponse.logMessage.ToString());
                            isZipFileMoved = true;
                        }
                        else
                        {
                            AppendLogMessage("Error in moving zip files to target location");
                        }
                    }

                    #endregion MoveZipFileToTargetFolder

                    #region FTP_Uploading_Downloading_Files
                    try
                    {
                        string ftpInFolderPath = requestObj.FTPInFolderPath.Trim();
                        string ftpOutFolderPath = requestObj.FTPOutFolderPath.Trim();
                        string ftpDownloadFolderPath = requestObj.ZipExtractedFilePath.Trim();//objHelper.GetConfigurationValue("FTPFileDownloadFolderPath");
                        string ftpPath = requestObj.FTPPath.Trim();
                        string userName = requestObj.FTPUserName.Trim();
                        string password = requestObj.FTPPassword.Trim();


                        // Uploads the files to FTP site
                        if (fileCount > 0 && isZipFileMoved)
                        {
                            string ftpFullPath = requestObj.FTPPath.Trim() + ftpInFolderPath + "/" + zipFileName;
                            AppendLogMessage("Uploading the details to FTP Site.");
                            AppendLogMessage("FTP upload path = " + ftpFullPath);
                            FtpWebRequest ftpUploadRequest = (FtpWebRequest)FtpWebRequest.Create(ftpFullPath);
                            ftpUploadRequest.Credentials = new NetworkCredential(userName, password);
                            ftpUploadRequest.KeepAlive = true;
                            ftpUploadRequest.UseBinary = true;
                            ftpUploadRequest.Method = WebRequestMethods.Ftp.UploadFile;

                            FileStream uploadedFileStream = File.OpenRead(outPath + zipFileName);
                            byte[] uploadedFileBuffer = new byte[uploadedFileStream.Length];
                            uploadedFileStream.Read(uploadedFileBuffer, 0, uploadedFileBuffer.Length);
                            uploadedFileStream.Close();

                            Stream ftpUploadStream = ftpUploadRequest.GetRequestStream();
                            ftpUploadStream.Write(uploadedFileBuffer, 0, uploadedFileBuffer.Length);
                            ftpUploadStream.Close();
                            AppendLogMessage("Successfully uploaded the file to FTP site.");

                        }
                        else
                            AppendLogMessage("No files are uploaded.");


                        try
                        {
                            // Downloads the fiels
                            AppendLogMessage("Downloading the details from FTP site.");
                            AppendLogMessage("FTP download path = " + ftpPath + ftpOutFolderPath + "/");
                            AppendLogMessage("Gets the file list from FTP site.");
                            FtpWebRequest ftpGetFilesRequest = (FtpWebRequest)FtpWebRequest.Create(ftpPath + ftpOutFolderPath + "/");
                            ftpGetFilesRequest.Credentials = new NetworkCredential(userName, password);
                            ftpGetFilesRequest.KeepAlive = true;
                            ftpGetFilesRequest.UseBinary = true;
                            ftpGetFilesRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                            FtpWebResponse ftpGetFilesResponse = (FtpWebResponse)ftpGetFilesRequest.GetResponse();
                            StreamReader ftpGetFilesReader = new StreamReader(ftpGetFilesResponse.GetResponseStream());

                            string line = ftpGetFilesReader.ReadLine();
                            if (line != null)
                            {
                                StringBuilder ftpGetFilesResult = new StringBuilder();
                                while (line != null)
                                {
                                    ftpGetFilesResult.Append(line);
                                    ftpGetFilesResult.Append("|");
                                    line = ftpGetFilesReader.ReadLine();
                                }
                                ftpGetFilesResult.Remove(ftpGetFilesResult.ToString().LastIndexOf("|"), 1);
                                ftpGetFilesReader.Close();
                                ftpGetFilesResponse.Close();
                                string[] listFiles = ftpGetFilesResult.ToString().Split('|');
                                AppendLogMessage("Successfully retrieves the file list from FTP site.");
                                AppendLogMessage("Total files fetched :  " + listFiles.Length);


                                foreach (string file in listFiles)
                                {
                                    if ((file.ToUpper().StartsWith(requestObj.FTPDownloadFile1.Trim()) ||
                                    file.ToUpper().StartsWith(requestObj.FTPDownloadFile2.Trim())) &&
                                        file.ToUpper().EndsWith(requestObj.TextFileExtension.Trim()))
                                    {
                                        AppendLogMessage("Started downloading the file : " + file);
                                        AppendLogMessage("Downloaded File Path : " + ftpPath + ftpOutFolderPath + "/" + file);
                                        FtpWebRequest ftpDownloadRequest = (FtpWebRequest)FtpWebRequest.Create(ftpPath + ftpOutFolderPath + "/" + file);
                                        ftpDownloadRequest.Credentials = new NetworkCredential(userName, password);
                                        ftpDownloadRequest.KeepAlive = true;
                                        ftpDownloadRequest.UseBinary = true;
                                        ftpDownloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                                        FtpWebResponse ftpDownloadFileResponse = (FtpWebResponse)ftpDownloadRequest.GetResponse();
                                        Stream ftpFileDownloadStream = ftpDownloadFileResponse.GetResponseStream();

                                        long cl = ftpDownloadFileResponse.ContentLength;
                                        int bufferSize = 2048;
                                        int readCount;
                                        FileStream outputDownloadFileStream = new FileStream(ftpDownloadFolderPath + file, FileMode.Create);
                                        byte[] fileBuffer = new byte[bufferSize];
                                        readCount = ftpFileDownloadStream.Read(fileBuffer, 0, bufferSize);
                                        while (readCount > 0)
                                        {
                                            outputDownloadFileStream.Write(fileBuffer, 0, readCount);
                                            readCount = ftpFileDownloadStream.Read(fileBuffer, 0, bufferSize);
                                        }

                                        ftpFileDownloadStream.Close();
                                        outputDownloadFileStream.Close();
                                        ftpDownloadFileResponse.Close();
                                        AppendLogMessage("Succussfully downloads the file : " + file);

                                        // Deletes the files in FTP Site
                                        AppendLogMessage("Started deleting the file : " + file);
                                        AppendLogMessage("Deleted File Path : " + ftpPath + ftpOutFolderPath + "/" + file);
                                        FtpWebRequest ftpDeleteRequest = (FtpWebRequest)FtpWebRequest.Create(ftpPath + ftpOutFolderPath + "/" + file);
                                        ftpDeleteRequest.Credentials = new NetworkCredential(userName, password);
                                        ftpDeleteRequest.KeepAlive = true;
                                        ftpDeleteRequest.UseBinary = true;

                                        ftpDeleteRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                                        FtpWebResponse ftpDeleteResponse = (FtpWebResponse)ftpDeleteRequest.GetResponse();
                                        ftpDeleteResponse.Close();
                                        AppendLogMessage("Sucussfully deletes the file : " + file);
                                    }
                                }
                            }
                            else
                            {
                                AppendLogMessage("No files exists to download.");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.ToUpper().Contains("FILE UNAVAILABLE"))
                                AppendLogMessage("No files exists to download.");
                            else
                                AppendLogMessage("Error while downloading files. Error : " + ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                        //write exception to file
                    }

                    #endregion
                }
                else
                {
                    AppendLogMessage("API Details are not available .");
                }
                responseObj.LogMessage = logMessage.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
                AppendLogMessage("Error: " + ex.Message.Trim());
                AppendLogMessage("Stack Trace: " + ex.StackTrace.Trim());
                if (requestObj.executionCount == Convert.ToInt16(Configuration.ExecuRetryCount))
                {
                    if (SendMailForApiFileGenerator(ex.Message.Trim() + "\n" + ex.StackTrace, requestObj))
                        AppendLogMessage("Mail has been sent successfully.");
                    else
                        AppendLogMessage("Mail is not sent successfully.");
                }
            }
            finally
            {
                AppendLogMessage("Daily update file generation ended.");
            }
            responseObj.LogMessage = logMessage.ToString();
            return responseObj;
        }

        public PayableManagementResponse GetVenGLDetails(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository payableDataAccess = null;
            FTPRequest fTPRequest = null;
            FTPResponse fTPResponse = null;
            FtpBL ftpBL = null;
            FTPEntity fTPEntity = null;
            try
            {
                responseObj = new PayableManagementResponse();

                AppendLogMessage("Retrieving changes in Vendor GL details.");
                payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                DataSet venGLTables = payableDataAccess.GetVenGLDetails(requestObj.companyId);
                if (venGLTables != null && venGLTables.Tables.Count > 0)
                {
                    #region Preparing Text
                    // vendor GL Details file
                    AppendLogMessage("Fetching Vendor GL Details.");
                    StringBuilder vendorGLFile = new StringBuilder();
                    foreach (DataRow row in venGLTables.Tables[0].Rows)
                    {
                        string rowData = "";
                        foreach (DataColumn col in venGLTables.Tables[0].Columns)
                        {
                            rowData += row[col.ColumnName].ToString().Trim() + "|";
                        }

                        vendorGLFile.Append(rowData.Substring(0, rowData.Length - 1));
                        vendorGLFile.AppendLine("|||||||");
                    }
                    string path = requestObj.OutputFilePath.Trim();

                    #endregion
                    int fileCount1 = 0;
                    //Saving to corresponding files
                    AppendLogMessage("Saving all the details into corresponding files.");
                    if (vendorGLFile.Length > 0)
                    {
                        this.LogInformationToFile(path, vendorGLFile.ToString(), requestObj.VenGLFileName.Trim(), requestObj.companyId, 1);
                        fileCount1 += 1;
                    }

                    //Zipping the files                  

                    string outPath = path;
                    bool isSuccessfullyZipped = false;
                    string zipFileName = string.Empty;
                    string outputPathAndFile = string.Empty;
                    if (fileCount1 > 0)
                    {
                        //Zipping the files
                        #region ZippingFiles

                        // gets the Zip file
                        AppendLogMessage("Zipping the files.");


                        if (requestObj.companyId == Convert.ToInt16(Configuration.NACompanyId.Trim()))
                        {
                            outputPathAndFile = Configuration.OutputNAGLVFileName.Trim();
                        }
                        else if (requestObj.companyId == Convert.ToInt16(Configuration.EUCompanyId.Trim()))
                        {
                            outputPathAndFile = Configuration.OutputEUGLVFileName.Trim();
                        }

                        zipFileName = outputPathAndFile + date + ".zip";

                        fTPRequest = new FTPRequest();
                        fTPResponse = new FTPResponse();
                        ftpBL = new FtpBL();
                        fTPEntity = new FTPEntity();
                        fTPEntity.LoggingFilePath = outPath;
                        fTPEntity.FileName = zipFileName;
                        fTPRequest.objFTPEntity = fTPEntity;
                        fTPResponse = ftpBL.GenerateZipFile(fTPRequest);

                        if (fTPResponse.ErrorStatus == Configuration.FtpSuccess)
                        {
                            AppendLogMessage(fTPResponse.logMessage.ToString());
                        }
                        else
                        {
                            AppendLogMessage("Error on zipping the file.");
                        }
                        #endregion
                    }
                }
                else
                {
                    AppendLogMessage("Vendor GL Details are not available .");
                }
                responseObj.LogMessage = logMessage.ToString();
                responseObj.Status = Response.Success;
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.LogMessage = logMessage.ToString();
                AppendLogMessage("Error: " + ex.Message.Trim());
                AppendLogMessage("Stack Trace: " + ex.StackTrace.Trim());
                if (requestObj.executionCount == Convert.ToInt16(Configuration.ExecuRetryCount))
                {
                    if (SendMailForApiFileGenerator(ex.Message.Trim() + "\n" + ex.StackTrace, requestObj))
                        AppendLogMessage("Mail has been sent successfully.");
                    else
                        AppendLogMessage("Mail is not sent successfully.");
                }
            }

            return responseObj;
        }

        public void AppendLogMessage(String strMessage)
        {
            logMessage.Append(System.DateTime.Now.ToString() + "-- " + strMessage.ToString());
            logMessage.AppendLine();
        }

        /// <summary>
        /// Log Information to File
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="strMessage"></param>
        /// <param name="strLogFileName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        internal int LogInformationToFile(string strPath, string strMessage, string strLogFileName, int comnpany, int type)
        {

            try
            {
                // Get the file name from the configuration xml.

                if (type == 1)
                {
                    if (comnpany == Convert.ToInt16(Configuration.NACompanyId))
                    {
                        if (strLogFileName == "GLV")
                            strLogFileName = Configuration.NAFileNamePrefix + strLogFileName + ".txt";
                        else
                            strLogFileName = Configuration.NAFileNamePrefix + strLogFileName + date + ".txt";
                    }
                    else if (comnpany == Convert.ToInt16(Configuration.EUCompanyId))
                    {
                        if (strLogFileName == "GLV")
                            strLogFileName = Configuration.EUFileNamePrefix + strLogFileName + ".txt";
                        else
                            strLogFileName = Configuration.EUFileNamePrefix + strLogFileName + date + ".txt";
                    }
                    else if (comnpany == 0)
                    {
                        strLogFileName = strLogFileName + date + ".txt";
                    }
                }
                if (type == 2)
                {
                    strLogFileName = strLogFileName + DateTime.Now.ToString("MM_dd_yy") + ".log";
                }


                if (!Directory.Exists(strPath))
                    Directory.CreateDirectory(strPath);

                strPath = strPath + strLogFileName;

                if (!File.Exists(strPath))
                    File.Create(strPath).Close();

                //Log the details into the file.
                using (StreamWriter w = File.AppendText(strPath))
                {
                    if (type == 2)
                        w.WriteLine(strMessage.Trim());
                    else
                        w.Write(strMessage.Trim());
                    w.Flush();
                    w.Close();
                }

                // Return success to the caller.                
                return 0;
            }
            catch (Exception ex)
            {
                return -1;
                throw ex;
            }
        }

        public bool SendMailForApiFileGenerator(String message, PayableManagementRequest request)
        {
            bool res;

            SendEmailRequest emailRequest = new SendEmailRequest();
            EMailInformation emailInformation = new EMailInformation();
            EmailBusiness emailBusiness = new EmailBusiness();
            try
            {
                string body = "<html><body><br>" +
                       "<br><br><font color=\"#ff0000\">" +
                       "<br><br>" + "Error: " + message +
                        "</body></html>";
                emailRequest.EmailConfigID = request.AppConfigID;
                emailRequest.ConnectionString = request.ConnectionString;
                //emailInformation.Subject = request.EmailRequest.EmailInformation.Subject;
                emailInformation.EmailFrom = request.EmailRequest.EmailInformation.EmailFrom;
                emailInformation.Body = body;
                emailInformation.SmtpAddress = request.EmailRequest.EmailInformation.SmtpAddress;
                emailInformation.Signature = request.EmailRequest.EmailInformation.Signature;
                emailInformation.IsDataTableBodyRequired = false;
                emailRequest.EmailInformation = emailInformation;
                emailBusiness.SendEmail(emailRequest);
                res = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }

        #endregion

        #region CTSI File Generator
        public PayableManagementResponse GenerateCtsiFilesToBeUploaded(PayableManagementRequest requestObj)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository payableDataAccess = null;
            DataSet ctsiTables = null;
            
            string generateZipFileStatus = string.Empty;
            string movedFileStatus = string.Empty;
            try
            {
                responseObj = new PayableManagementResponse();

                AppendLogMessage("*************************************************************************************************************************************");
                AppendLogMessage(" Chempoint Ctsi File Generator Started For " + requestObj.Company + " - Processing Date: " + requestObj.processingDate.ToShortDateString());

                AppendLogMessage(" Started collecting Ctsi transaction details from GP");
                payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(requestObj.ConnectionString);
                ctsiTables = payableDataAccess.FetchAllCtsiTransactionDetails(requestObj.Company, requestObj.processingDate, requestObj.companyId);

                if ((requestObj.Company.Equals("CHMPT") && ctsiTables.Tables.Count == 5 && ctsiTables.Tables.Cast<DataTable>().Any(x => x.Rows.Count > 0))
                        || (requestObj.Company.Equals("CPEUR") && ctsiTables.Tables.Count == 3 && ctsiTables.Tables.Cast<DataTable>().Any(x => x.Rows.Count > 0)))
                {
                    AppendLogMessage(" Ctsi documents fetched successfully");
                }

                #region GenerateTextFile

                if ((requestObj.Company.Equals("CHMPT") && ctsiTables.Tables.Count == 5 && ctsiTables.Tables.Cast<DataTable>().Any(x => x.Rows.Count > 0))
                        || (requestObj.Company.Equals("CPEUR") && ctsiTables.Tables.Count == 3 && ctsiTables.Tables.Cast<DataTable>().Any(x => x.Rows.Count > 0)))
                {
                    if (GenerateTextFile(requestObj,requestObj.Company, ctsiTables, requestObj.TempFolderPath))
                    {
                        AppendLogMessage(" Text files generated successfully.");
                    }
                    else
                    {
                        AppendLogMessage(" Text files are not generated");
                    }
                }
                else
                {
                    AppendLogMessage(" Ctsi Details are not available to upload.");
                }
                #endregion GenerateTextFile

                #region GenerateZipFile
                string outputPathAndFile = (requestObj.Company.ToUpper().Trim() == "CHMPT") ? requestObj.OutputNaFileName : requestObj.OutputEuFileName;

                zipName = outputPathAndFile + requestObj.processingDate + ".zip";
                if (FileCount > 0)
                {
                    generateZipFileStatus = GenerateZipFile(requestObj, out zipName);
                }
                else
                {
                    AppendLogMessage(" No files to zip.");
                }
                #endregion GenerateZipFile

                SaveAllUploadedDetails(requestObj.CTSIUserId, ctsiTables, generateZipFileStatus, requestObj.ConnectionString, requestObj.companyId);

                if (generateZipFileStatus == "Success")
                {
                    movedFileStatus = MoveZipFileToTargetLocation(requestObj, zipName);
                }

                if (FileCount > 0 && generateZipFileStatus == "Success" && movedFileStatus == "Success")
                {
                    UploadCtsiZipFilesToFtp(requestObj, zipName);
                    AppendLogMessage(" Chempoint Ctsi File Generator Ended For " + requestObj.Company + " - Processing Date: " + requestObj.processingDate.ToShortDateString());
                    AppendLogMessage("*************************************************************************************************************************************");
                }

                DownloadCtsiFilesFromFtp(requestObj.DownloadForNAandEU, requestObj);
                responseObj.Status = Response.Success;
                responseObj.LogMessage = logMessage.ToString().Trim();
            }
            catch (Exception ex)
            {
                responseObj.Status = Response.Error;
                responseObj.ErrorMessage = ex.Message.ToString().Trim();
            }
            return responseObj;
        }

        public void SaveAllUploadedDetails(string CTSIUserId, DataSet ctsiTables, string status, string ConnectionString, int companyId)
        {
            PayableManagementResponse responseObj = null;
            IPayableManagementRepository payableDataAccess = null;
            try
            {
                responseObj = new PayableManagementResponse();
                if (ctsiTables.Tables.Count > 0 && status == "Success")
                {
                    payableDataAccess = new ChemPoint.GP.PMDL.PMRepositoryDL(ConnectionString);
                    payableDataAccess.SaveAllUploadedDetails(ctsiTables, CTSIUserId, companyId);
                    AppendLogMessage(" Ctsi transaction details saved successfully into table");
                }
            }
            catch (Exception ex)
            {
                AppendLogMessage(ex.Message.ToString().Trim());
            }
        }

        public void DownloadCtsiFilesFromFtp(string company, PayableManagementRequest requestObj)
        {
            FTPRequest fTPRequest = null;
            FTPResponse fTPResponse = null;
            try
            {
                fTPRequest = new FTPRequest();
                fTPResponse = new FTPResponse();
                FtpBL ftpBL = new FtpBL();
                FTPEntity objFTPEntity = new FTPEntity();

                objFTPEntity.SourcePath = requestObj.TempFolderPath;
                objFTPEntity.FileName = requestObj.FileNameStarts;
                objFTPEntity.ClientDownloadPath = requestObj.FTPPath;
                objFTPEntity.FtpUploadPath = requestObj.FTPOutFolderPath;
                objFTPEntity.UploadFilePath = requestObj.ZipExtractedFilePath;
                objFTPEntity.FtpUserName = requestObj.FTPUserName;
                objFTPEntity.FtpPassword = requestObj.FTPPassword;
                fTPRequest.objFTPEntity = objFTPEntity;
                fTPResponse = ftpBL.DownloadCtsiZipFilesFromFtp(fTPRequest);
                if (fTPResponse.ErrorStatus == Configuration.FtpSuccess)
                {
                    AppendLogMessage(fTPResponse.logMessage.ToString());
                }
                else
                {
                    AppendLogMessage("Error while uploading CTSI files to FTP");
                }
            }
            catch (Exception ex)
            {
                AppendLogMessage(" Error while downloading files from FTP" + "\n" + "Error : " + ex.Message);
                AppendLogMessage(ex.StackTrace);
                AppendLogMessage(" Sending failure mail.");

                SendEmailRequest emailRequest = new SendEmailRequest();
                SendEmailResponse emailResponse = new SendEmailResponse();
                EMailInformation emailInformation = new EMailInformation();
                EmailBusiness emailBusiness = new EmailBusiness();

                emailRequest.EmailConfigID = requestObj.AppConfigID;
                emailRequest.ConnectionString = requestObj.ConnectionString;
                emailInformation.Subject = requestObj.GPToCtsiMailSubject+" - " + requestObj.Company;
                emailInformation.EmailFrom = requestObj.GPToCtsiMailFrom;
                emailInformation.Body = (" <b>Error occurred while downloading files from FTP</b>" + "<br><br>" + " <b>Error Description:</b>" + "<br>" + ex.Message.Trim() + "<br>" + " <b>Error Details:</b> " + "<br>" + ex.StackTrace);
                emailInformation.SmtpAddress = requestObj.SMTPServcer;
                emailInformation.Signature = requestObj.CTSISignature;
                emailInformation.IsDataTableBodyRequired = false;
                emailRequest.EmailInformation = emailInformation;
                emailResponse = emailBusiness.SendEmail(emailRequest);
                if (emailResponse.IsMailSent == true)
                {
                    AppendLogMessage(" Mail has been sent successfully");
                    AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                }
                else
                {
                    AppendLogMessage(" Mail is not sent successfully");
                    AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                }
            }
        }

        public void UploadCtsiZipFilesToFtp(PayableManagementRequest requestObj, string zipName)
        {
            FTPRequest fTPRequest = null;
            FTPResponse fTPResponse = null;
            try
            {
                fTPRequest = new FTPRequest();
                fTPResponse = new FTPResponse();
                FtpBL ftpBL = new FtpBL();
                FTPEntity objFTPEntity = new FTPEntity();

                objFTPEntity.SourcePath = requestObj.TempFolderPath;
                objFTPEntity.FileName = zipName;
                objFTPEntity.ClientUploadPath = requestObj.FTPPath;
                objFTPEntity.FtpUploadPath = requestObj.FTPInFolderPath;
                objFTPEntity.FtpUserName = requestObj.FTPUserName;
                objFTPEntity.FtpPassword = requestObj.FTPPassword;
                fTPRequest.objFTPEntity = objFTPEntity;

                fTPResponse = ftpBL.UploadCtsiZipFilesToFtp(fTPRequest);
                if (fTPResponse.ErrorStatus == Configuration.FtpSuccess)
                {
                    AppendLogMessage(fTPResponse.logMessage.ToString());
                }
                else
                {
                    AppendLogMessage("Error while uploading CTSI files to FTP");
                }
            }
            catch (Exception ex)
            {
                AppendLogMessage(" Error while uploading files to FTP " + "\n" + "Error : " + ex.Message);
                AppendLogMessage(ex.StackTrace);
                AppendLogMessage(" Sending failure mail.");

                SendEmailRequest emailRequest = new SendEmailRequest();
                SendEmailResponse emailResponse = new SendEmailResponse();
                EMailInformation emailInformation = new EMailInformation();
                EmailBusiness emailBusiness = new EmailBusiness();

                emailRequest.EmailConfigID = requestObj.AppConfigID;
                emailRequest.ConnectionString = requestObj.ConnectionString;
                emailInformation.Subject = requestObj.GPToCtsiMailSubject+ " - " + requestObj.Company;
                emailInformation.EmailFrom = requestObj.GPToCtsiMailFrom;
                emailInformation.Body = (" <b>Error occurred while uploading files to FTP for the date " + requestObj.processingDate.ToShortDateString() + "</b>" + "<br><br>" + " <b>Error Description:</b> " + "<br>" + ex.Message.Trim() + "<br>" + " <b>Error Details:</b> " + "<br>" + ex.StackTrace);
                emailInformation.SmtpAddress = requestObj.SMTPServcer;
                emailInformation.Signature = requestObj.CTSISignature;
                emailInformation.IsDataTableBodyRequired = false;
                emailRequest.EmailInformation = emailInformation;
                emailResponse = emailBusiness.SendEmail(emailRequest);
                if (emailResponse.IsMailSent == true)
                {
                    AppendLogMessage(" Mail has been sent successfully");
                    AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                }
                else
                {
                    AppendLogMessage(" Mail is not sent successfully");
                    AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                }
            }
        }

        public string MoveZipFileToTargetLocation(PayableManagementRequest requestObj, string zipName)
        {
            FTPRequest fTPRequest = null;
            FTPResponse fTPResponse = null;
            try
            {
                fTPRequest = new FTPRequest();
                fTPResponse = new FTPResponse();
                FtpBL ftpBL = new FtpBL();
                FTPEntity objFTPEntity = new FTPEntity();
                objFTPEntity.SourcePath = requestObj.TempFolderPath;
                objFTPEntity.DestinationPath = requestObj.OutputFilePath;
                objFTPEntity.FileName = zipName;
                fTPRequest.objFTPEntity = objFTPEntity;
                fTPResponse = ftpBL.MoveZipFileToTargetFolder(fTPRequest);
                if (fTPResponse.ErrorStatus == Configuration.FtpSuccess)
                {
                    AppendLogMessage(fTPResponse.logMessage.ToString());
                }
                else
                {
                    AppendLogMessage("Error in moving zip files to target location");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return fTPResponse.ErrorStatus;
        }

        /// <summary>
        /// GenerateText File
        /// </summary>
        /// <param name="company"></param>
        /// <param name="ctsiTables"></param>
        /// <param name="path"></param>
        /// <param name="fileCount"></param>
        /// <returns></returns>
        public bool GenerateTextFile(PayableManagementRequest objRequest,string company, DataSet transactionDetails, string path)
        {
            int fileCount = 0;
            StringBuilder inBoundFile = null;
            StringBuilder outBoundFile = null;
            StringBuilder custReturnFile = null;
            StringBuilder supReturnFile = null;
            StringBuilder invTransfersFile = null;


            if (transactionDetails.Tables.Count > 0)
            {
                //Inbound
                if (transactionDetails.Tables[0].Rows.Count > 0)
                {
                    inBoundFile = new StringBuilder();
                    inBoundFile.AppendLine(objRequest.InboundHeader);
                    foreach (DataRow row in transactionDetails.Tables[0].Rows)
                    {
                        string rowData = "";
                        foreach (DataColumn col in transactionDetails.Tables[0].Columns)
                        {
                            if ((col.ColumnName != "LineNumber"))
                                rowData += row[col.ColumnName].ToString().Trim() + "|";
                        }
                        inBoundFile.AppendLine(rowData.Substring(0, rowData.Length - 1));
                    }
                }

                //Outbound
                if (transactionDetails.Tables[1].Rows.Count > 0)
                {
                    outBoundFile = new StringBuilder();
                    outBoundFile.AppendLine(objRequest.OutboundHeader);
                    foreach (DataRow row in transactionDetails.Tables[1].Rows)
                    {
                        string rowData = "";
                        foreach (DataColumn col in transactionDetails.Tables[1].Columns)
                        {
                            if ((col.ColumnName != "LineNumber"))
                                rowData += row[col.ColumnName].ToString().Trim() + "|";
                        }
                        outBoundFile.AppendLine(rowData.Substring(0, rowData.Length - 1));
                    }
                }

                //CustomerReturn
                if (transactionDetails.Tables[2].Rows.Count > 0)
                {
                    custReturnFile = new StringBuilder();
                    custReturnFile.AppendLine(objRequest.CustomerReturnHeader);
                    foreach (DataRow row in transactionDetails.Tables[2].Rows)
                    {
                        string rowData = "";
                        foreach (DataColumn col in transactionDetails.Tables[2].Columns)
                        {
                            if ((col.ColumnName != "LineNumber"))
                                rowData += row[col.ColumnName].ToString().Trim() + "|";
                        }
                        custReturnFile.AppendLine(rowData.Substring(0, rowData.Length - 1));
                    }
                }

                if (company.ToUpper().Trim() == "CHMPT")
                {
                    //SupplierReturn
                    if (transactionDetails.Tables[3].Rows.Count > 0)
                    {
                        supReturnFile = new StringBuilder();
                        supReturnFile.AppendLine(objRequest.SupplierReturnHeader);

                        foreach (DataRow row in transactionDetails.Tables[3].Rows)
                        {
                            string rowData = "";
                            foreach (DataColumn col in transactionDetails.Tables[3].Columns)
                            {
                                if ((col.ColumnName != "LineNumber"))
                                    rowData += row[col.ColumnName].ToString().Trim() + "|";
                            }
                            supReturnFile.AppendLine(rowData.Substring(0, rowData.Length - 1));
                        }
                    }

                    //InvTransfers
                    if (transactionDetails.Tables[4].Rows.Count > 0)
                    {
                        invTransfersFile = new StringBuilder();
                        invTransfersFile.AppendLine(objRequest.TransferHeader);

                        foreach (DataRow row in transactionDetails.Tables[4].Rows)
                        {
                            string rowData = "";
                            foreach (DataColumn col in transactionDetails.Tables[4].Columns)
                            {
                                if ((col.ColumnName != "LineNumber"))
                                    rowData += row[col.ColumnName].ToString().Trim() + "|";
                            }
                            invTransfersFile.AppendLine(rowData.Substring(0, rowData.Length - 1));
                        }
                    }
                }

                //Saving to corresponding files
                AppendLogMessage(" Saving all the details into corresponding files.");
                if (inBoundFile != null && inBoundFile.Length > 0)
                {
                    //this.LogInformationToFile(path, inBoundFile.ToString(),
                    isSucessfullyRetured = LogInformationToFile(path, inBoundFile.ToString(),
                        (company.ToUpper().Trim() == "CHMPT") ? objRequest.NaInboundFileName.ToString().Trim() : objRequest.EuInboundFileName.ToString().Trim()
                        , 0, 1);
                    fileCount += 1;
                    if (isSucessfullyRetured == 0)
                        AppendLogMessage(" Inbound text file is generated successfully");
                    else
                    {
                        AppendLogMessage(" Inbound text file is not generated");
                        return false;
                    }
                }

                if (outBoundFile != null && outBoundFile.Length > 0)
                {
                    //this.LogInformationToFile(path, outBoundFile.ToString(),
                    isSucessfullyRetured = LogInformationToFile(path, outBoundFile.ToString(),
                        (company.ToUpper().Trim() == "CHMPT") ? objRequest.NaOutboundFileName : objRequest.EuOutboundFileName
                        , 0, 1);
                    fileCount += 1;
                    if (isSucessfullyRetured == 0)
                        AppendLogMessage(" Outbound text file is generated successfully");
                    else
                    {
                        AppendLogMessage(" Outbound text file is not generated");
                        return false;
                    }

                }

                if (custReturnFile != null && custReturnFile.Length > 0)
                {
                    //this.LogInformationToFile(path, custReturnFile.ToString()
                    isSucessfullyRetured = LogInformationToFile(path, custReturnFile.ToString()
                          , (company.ToUpper() == "CHMPT") ? objRequest.NaCustomerReturnFileName : objRequest.EuCustomerReturnFileName
                        , 0, 1);
                    fileCount += 1;
                    if (isSucessfullyRetured == 0)
                        AppendLogMessage(" Customer return text file is generated successfully");
                    else
                    {
                        AppendLogMessage(" Customer return text file is not generated");
                        return false;
                    }
                }

                if (supReturnFile != null && supReturnFile.Length > 0)
                {
                    //this.LogInformationToFile(path, supReturnFile.ToString(), objHelper.GetConfigurationValue("NaSupplierReturnFileName"), 1);
                    isSucessfullyRetured = LogInformationToFile(path, supReturnFile.ToString(), objRequest.NaSupplierReturnFileName, 0, 1);
                    fileCount += 1;
                    if (isSucessfullyRetured == 0)
                        AppendLogMessage(" Supplier return file is generated successfully");
                    else
                    {
                        AppendLogMessage(" Supplier return file is not generated successfully");
                        return false;
                    }

                }

                if (invTransfersFile != null && invTransfersFile.Length > 0)
                {
                    //this.LogInformationToFile(path, invTransfersFile.ToString(), objHelper.GetConfigurationValue("NaTransfersFileName"), 1);
                    isSucessfullyRetured = LogInformationToFile(path, invTransfersFile.ToString(), objRequest.NaTransfersFileName, 0, 1);
                    fileCount += 1;
                    if (isSucessfullyRetured == 0)
                        AppendLogMessage(" Inventory transfer file is generated successfully");
                    else
                    {
                        AppendLogMessage(" Inventory transfer file is not generated successfully");
                        return false;
                    }
                }
            }
            FileCount = fileCount;
            return true;
        }

        public string GenerateZipFile(PayableManagementRequest requestObj, out string zipName)
        {
            FTPRequest fTPRequest = null;
            FTPResponse fTPResponse = null;
            try
            {

                fTPRequest = new FTPRequest();
                FTPEntity objFTPEntity = new FTPEntity();
                fTPResponse = new FTPResponse();
                FtpBL ftpBL = new FtpBL();
                // gets the Zip file

                string outputPathAndFile = (requestObj.Company.ToUpper().Trim() == "CHMPT") ? requestObj.OutputNaFileName : requestObj.OutputEuFileName ;
                string processingDate = requestObj.processingDate.ToString("MMddyyyy");
                string time = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                processingDate = processingDate + time;

                zipName = outputPathAndFile + processingDate + ".zip";
                

                objFTPEntity.LoggingFilePath = requestObj.TempFolderPath;
                objFTPEntity.FileName = zipName;
                fTPRequest.objFTPEntity = objFTPEntity;
                fTPResponse = ftpBL.GenerateZipFile(fTPRequest);

                if (fTPResponse.ErrorStatus == Configuration.FtpSuccess)
                {
                    AppendLogMessage(fTPResponse.logMessage.ToString());
                }
                else
                {
                    AppendLogMessage(" Files are not zipped");
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return fTPResponse.ErrorStatus;
        }

        #endregion CTSI File Generator
    }
    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
