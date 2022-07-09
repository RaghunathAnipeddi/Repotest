using Chempoint.GP.Infrastructure.Email;
using Chempoint.GP.Infrastructure.Logging;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Model.Interactions.Email;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.BusinessContracts.Sales;
using ChemPoint.GP.DataContracts.Sales;
using ChemPoint.GP.Email;
using ChemPoint.GP.Entities.BaseEntities;
using Microsoft.Dynamics.GP.eConnect;
using ReceivablesDL.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;

namespace Chempoint.GP.ReceivablesBusiness
{
    public class ReceivablesBusiness : IReceivablesBusiness
    {
        StringBuilder logMessage = new StringBuilder();
        public ReceivablesBusiness()
        {
        }

        #region Bank_summary_and_CTX_import

        /// <summary>
        /// Convert CSV File to DataTable
        /// Filter the Datatable
        /// Send the Converted and filetered datatable to API 
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse ImportEFTBankSummaryReport(ReceivablesRequest receivablesRequest)
        {
            ReceivablesResponse objReceivablesResponse = null;
            IReceivablesRepository receivalblesDataAccess = null;
            DataTable dtForMail = null;
            List<EFTPayment> objEFListForMail = null, objOriginalEFTList = null, objFilteredList = null;
            EFTPayment objEFT = null;
            string FileName = string.Empty, strCSVFile = string.Empty, status = string.Empty;

            try
            {
                logMessage.Append(receivablesRequest.LogMessage);

                logMessage.Append(DateTime.Now + " - ImportEFTSummaryReport method in Chempoint.GP.ReceivablesBusiness.ReceivablesBusiness class is started");
                FileName = Path.GetFileName(receivablesRequest.EFTPaymentFilePath);
                receivablesRequest.EFTPaymentFileName = FileName;
                receivalblesDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(receivablesRequest.ConnectionString);
                objReceivablesResponse = new ReceivablesResponse();

                status = IsEftFileAlreadyProcessed(receivablesRequest);
                if (status == "0")
                {
                    //Convert CSV file to datatable
                    DataTable dt = GetDataTableFromCsv(receivablesRequest.EFTPaymentFilePath, true);

                    logMessage.Append(DateTime.Now + " - " + receivablesRequest.EFTPaymentFilePath + "is converted into datatable");

                    if (dt != null)
                    {
                        objOriginalEFTList = new List<EFTPayment>();

                        foreach (DataRow dr in dt.Rows)
                        {
                            objEFT = new EFTPayment();

                            if (dr["As Of"] != DBNull.Value && dr["As Of"] != "")
                                objEFT.AsOf = Convert.ToDateTime(dr["As Of"]);

                            objEFT.Currency = dr["Currency"].ToString();
                            objEFT.BankIDType = dr["BankID Type"].ToString();
                            objEFT.BankID = dr["BankID"].ToString();
                            objEFT.Account = dr["Account"].ToString();
                            objEFT.DataType = dr["Data Type"].ToString();

                            if (dr["BAI Code"] != DBNull.Value && dr["BAI Code"] != "")
                                objEFT.BAICode = Convert.ToInt32(dr["BAI Code"]);

                            objEFT.Description = dr["Description"].ToString();

                            if (dr["Amount"] != DBNull.Value && dr["Amount"] != "")
                                objEFT.Amount = Convert.ToDecimal(dr["Amount"]);


                            objEFT.BalanceOrValueDate = dr["Balance/Value Date"].ToString();
                            objEFT.CustomerReference = dr["Customer Reference"].ToString();

                            if (dr["Immediate Availability"] != DBNull.Value && dr["Immediate Availability"] != "")
                                objEFT.ImmediateAvailability = Convert.ToInt32(dr["Immediate Availability"]);

                            if (dr["1 Day Float"] != DBNull.Value && dr["1 Day Float"] != "")
                                objEFT.OneDayFloat = Convert.ToInt32(dr["1 Day Float"]);

                            if (dr["2+ DayFloat"] != DBNull.Value && dr["2+ DayFloat"] != "")
                                objEFT.TwoPlusDayFloat = Convert.ToInt32(dr["2+ DayFloat"]);

                            objEFT.BankReference = dr["Bank Reference"].ToString();

                            if (dr["# of Items"] != DBNull.Value && dr["# of Items"] != "")
                                objEFT.NoOfItems = Convert.ToInt32(dr["# of Items"]);

                           
                            objEFT.Text = dr["Text"].ToString();
                            objEFT.CTXId = Convert.ToInt32(dr["RowID"].ToString());

                            objOriginalEFTList.Add(objEFT);
                        }

                        receivablesRequest.EFTOriginalList = objOriginalEFTList;
                        //Filtering only credit data types rows

                        DataView CreditTypeView = new DataView(dt);
                        CreditTypeView.RowFilter = "[Data Type]='Credits'";

                        DataTable creditsDT = CreditTypeView.ToTable();

                        if (creditsDT.Rows.Count > 0)
                        {
                            objFilteredList = new List<EFTPayment>();
                            objEFListForMail = new List<EFTPayment>();
                            int inc = 1;
                            string strReferenceNumber = string.Empty;
                            foreach (DataRow row in creditsDT.Rows)
                            {
                                if (!row["Description"].ToString().ToLower().Contains("lockbox") &&
                                    !row["Text"].ToString().ToLower().Contains("american express") &&
                                    !row["Text"].ToString().ToLower().Contains("bofa merch") &&
                                    !row["Text"].ToString().ToLower().Contains("canada lockbox") &&
                                    !row["Text"].ToString().ToLower().Contains("zba transfer") &&
                                    !row["BankID"].ToString().ToLower().Contains("bofanlnx") &&
                                    !row["Text"].ToString().ToLower().Contains("funds repatriation") &&
                                    !row["Text"].ToString().ToLower().Contains("corptrad"))
                                {
                                    objEFT = new EFTPayment();

                                    objEFT.AsOf = Convert.ToDateTime(row["As Of"].ToString());
                                    objEFT.DateReceived = Convert.ToDateTime(row["As Of"].ToString());
                                    objEFT.Account = (decimal.Parse(row["Account"].ToString(), NumberStyles.Float)).ToString();
                                    objEFT.Amount = Convert.ToDecimal(row["Amount"].ToString());
                                    objEFT.Text = row["Text"].ToString();

                                    if (row["Text"].ToString().ToLower().Contains("co id:"))
                                        strReferenceNumber = row["Text"].ToString().Substring(row["Text"].ToString().IndexOf("CO ID:") + 6, row["Text"].ToString().IndexOf(" ", row["Text"].ToString().IndexOf("CO ID:") + 6) - row["Text"].ToString().IndexOf("CO ID:") - 6);
                                    else if (row["Text"].ToString().ToLower().Contains("wire type"))
                                        strReferenceNumber = row["Text"].ToString().Substring(row["Text"].ToString().IndexOf("SNDR REF:") + 9, row["Text"].ToString().IndexOf(" ", row["Text"].ToString().IndexOf("SNDR REF:") + 9) - row["Text"].ToString().IndexOf("SNDR REF:") - 9);
                                    else
                                        strReferenceNumber = row["Bank Reference"].ToString();

                                    var IsExists = objFilteredList.Where(r => r.ReferenceNumber == strReferenceNumber).FirstOrDefault();
                                    if (IsExists != null)
                                        strReferenceNumber = strReferenceNumber.Trim() + "-" + inc++;

                                    if (!string.IsNullOrEmpty(strReferenceNumber))
                                        objEFT.ReferenceNumber = strReferenceNumber;

                                    //Datatable to send the mail to Account Receivables
                                    if (row["Text"].ToString().ToLower().StartsWith("wire type"))
                                    {
                                        EFTPayment objEmailEntry = new EFTPayment();

                                        objEmailEntry.AsOf = Convert.ToDateTime(row["As Of"].ToString());
                                        objEmailEntry.Account = (decimal.Parse(row["Account"].ToString(), NumberStyles.Float)).ToString();
                                        objEmailEntry.Amount = Convert.ToDecimal(row["Amount"].ToString());
                                        objEmailEntry.Text = row["Text"].ToString();

                                        objEFListForMail.Add(objEmailEntry);
                                    }

                                    if (Convert.ToDecimal(row["Amount"].ToString()) > 0)
                                    {
                                        objEFT.PaymentAmount = Convert.ToDecimal(row["Amount"].ToString());
                                        objEFT.ItemAmount = Convert.ToDecimal(row["Amount"].ToString());
                                    }

                                    objEFT.CustomerID = string.Empty;
                                    objEFT.ReceivedOnCTS = "Yes";
                                    objEFT.Source = "CTX";
                                    objEFT.BankOriginatingID = row["BankID"].ToString();
                                    objEFT.CTXId = Convert.ToInt32(row["RowID"].ToString());
                                    
                                    objEFT.CurrencyID = row["Currency"].ToString();
                                    objEFT.PaymentNumber = string.Empty;
                                    objEFT.ItemReference = string.Empty;

                                    if (row["Text"] != null && row["Text"] != "" && row["Text"].ToString().Length > 2000)
                                        objEFT.Notes = row["Text"].ToString().Substring(1, 1990);
                                    else
                                        objEFT.Notes = row["Text"].ToString();


                                    objEFT.EftStatusId = 0;

                                    objFilteredList.Add(objEFT);
                                }
                            }


                            if (objFilteredList != null && objFilteredList.Count > 0)
                            {
                                logMessage.Append(DateTime.Now + " - All filters are processed");
                                receivablesRequest.EFTFilteredPaymentList = objFilteredList;
                                
                                logMessage.Append(DateTime.Now + " - ImportEFTSummaryReport method in ChemPoint.GP.ReceivablesDL.ReceivablesDL is invoked");
                                string result = (string)receivalblesDataAccess.SaveImportedEFTBankSummaryDetail(receivablesRequest);

                                if (result == "1" && objEFListForMail != null && objEFListForMail.Count > 0)
                                {
                                    logMessage.Append(DateTime.Now + " - Bank Summary details imported successfully");
                                    receivablesRequest.EFTMailList = objEFListForMail;
                                    dtForMail = DataTableMapper.EFTMailReport(receivablesRequest,
                                                DataColumnMapping.ImportEFTMailSummary);
                                    if (dtForMail != null && dtForMail.Rows.Count > 0)
                                    {
                                        SendMailToAccountReceivables(dtForMail);
                                    }
                                    objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                                }
                                else if (result == "0")
                                {
                                    logMessage.Append(DateTime.Now + " - File contains duplicate data completly");
                                    objReceivablesResponse.ErrorMessage = "File contains duplicate data completly";
                                    objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                                }

                            }
                            else
                            {
                                logMessage.Append(DateTime.Now + " - No data is generated as per filtered conditions");
                                objReceivablesResponse.ErrorMessage = "No data is generated as per filtered conditions";
                                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                            }
                        }
                        else
                        {
                            logMessage.Append(DateTime.Now + " - No data is generated as per filtered conditions");
                            objReceivablesResponse.ErrorMessage = "No data is generated as per filtered conditions";
                            objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                        }
                    }
                    else
                    {
                        logMessage.Append(DateTime.Now + " - File is empty");
                        objReceivablesResponse.ErrorMessage = "File is empty";
                        objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                    }
                }
                else if (status == "1")
                {
                    logMessage.Append(DateTime.Now + " - Batch Id and File name is already exists");
                    objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                    objReceivablesResponse.ErrorMessage = "Batch Id and File name is already exists";
                }
                else if (status == "2")
                {
                    logMessage.Append(DateTime.Now + " - File name is already exists");
                    objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                    objReceivablesResponse.ErrorMessage = "File name is already exists";
                }
                else if (status == "3")
                {
                    logMessage.Append(DateTime.Now + " - Batch Id is already exists");
                    objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                    objReceivablesResponse.ErrorMessage = "Batch Id is already exists";
                }
            }
            catch (Exception ex)
            {

                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                objReceivablesResponse.ErrorMessage = ex.Message.ToString().Trim();
                logMessage.Append(DateTime.Now + " Exception Message- " + ex.Message);
                logMessage.Append(DateTime.Now + " Exception Stack Trace- " + ex.StackTrace);
            }
            finally
            {
                logMessage.Append(DateTime.Now + " - ImportEFTSummaryReport method in Chempoint.GP.ReceivablesBusiness.ReceivablesBusiness class is ended");
                objReceivablesResponse.LogMessage = logMessage.ToString();
            }
            return objReceivablesResponse;
        }

        /// <summary>
        /// Extracting bank remittance values from uploaded notepad file
        /// Insert/Update EFT Transaction details
        /// Insert/Update EFT transaction map details
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse ImportEFTRemittanceReport(ReceivablesRequest receivablesRequest)
        {
            
            string PaymentRowID = string.Empty, MatchedreferenceNumber = string.Empty;
            //bool bIterate = false;
            ReceivablesResponse objReceivablesResponse = null;
            IReceivablesRepository receivalblesDataAccess = null;
            string FileName = string.Empty, status = string.Empty;
            try
            {
                logMessage.Append(receivablesRequest.LogMessage);
                logMessage.Append(DateTime.Now + " - ImportEFTRemittanceReport method in Chempoint.GP.ReceivablesBusiness.ReceivablesBusiness class is started");
                FileName = Path.GetFileName(receivablesRequest.EFTPaymentFilePath);
                receivablesRequest.EFTPaymentFileName = FileName;
                objReceivablesResponse = new ReceivablesResponse();

                status = IsEftFileAlreadyProcessed(receivablesRequest);
                if (status == "0")
                {
                    #region Extracting bank remittance values from uploaded notepad file

                    receivalblesDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(receivablesRequest.ConnectionString);

                    DataTable dtEFTRemit = new DataTable();
                    dtEFTRemit.Columns.Add("Originator", typeof(string));
                    dtEFTRemit.Columns.Add("TraceNumber", typeof(string));
                    dtEFTRemit.Columns.Add("ACHTraceNumber", typeof(string));
                    dtEFTRemit.Columns.Add("AccountName", typeof(string));
                    dtEFTRemit.Columns.Add("BankAccountNumber", typeof(string));
                    dtEFTRemit.Columns.Add("BankRoutingNumber", typeof(string));
                    dtEFTRemit.Columns.Add("Source", typeof(string));
                    dtEFTRemit.Columns.Add("CID", typeof(string));
                    dtEFTRemit.Columns.Add("CheckAmount", typeof(decimal));
                    dtEFTRemit.Columns.Add("EffectiveDate", typeof(DateTime));
                    dtEFTRemit.Columns.Add("IsMatched", typeof(bool));
                    dtEFTRemit.Columns.Add("PaymentRowId", typeof(int));

                    DataTable dtEFTNotePadItem = new DataTable();
                    dtEFTNotePadItem.Columns.Add("PaymentRowId", typeof(string));
                    dtEFTNotePadItem.Columns.Add("ApplyRowId", typeof(string));
                    dtEFTNotePadItem.Columns.Add("InvoiceNumber", typeof(string));
                    dtEFTNotePadItem.Columns.Add("InvoiceAmount", typeof(decimal));


                    // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(receivablesRequest.EFTPaymentFilePath))
                    {
                        string line;
                        DataRow dr = null;
                        DataRow drApply = null;
                        int rowCount = 1;
                        int rowCountApply = 1;
                        string tempInvNumber = "";

                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.Contains("============================  PAYMENT INFORMATION  ==========================="))
                            {
                                if (dr != null)
                                    dtEFTRemit.Rows.Add(dr);
                                if (drApply != null)
                                {
                                    dtEFTNotePadItem.Rows.Add(drApply);
                                    drApply = null;
                                    tempInvNumber = string.Empty;
                                }

                                dr = dtEFTRemit.NewRow();
                                dr["PaymentRowId"] = rowCount;
                                rowCount++;
                            }

                            if (line.Contains("CR AMT:"))
                            {
                                dr["Originator"] = line.Substring(line.IndexOf("ORIGINATOR:") + 11, 10).Trim();
                                dr["AccountName"] = line.Substring(line.IndexOf("ORIGINATOR:") + 27, 20).Trim();
                                dr["CheckAmount"] = Convert.ToDecimal(line.Substring(line.IndexOf("$") + 1, line.IndexOf("ORIGINATOR:") - line.IndexOf("$") - 1));

                            }

                            if (line.Contains("|ORIG. BANK:"))
                            {
                                dr["BankRoutingNumber"] = line.Substring(line.IndexOf("BANK:") + 10, 9).Trim();

                                dr["BankAccountNumber"] = (line.Substring(line.IndexOf("BANK:") + 32, line.IndexOf("EFFECT") - line.IndexOf("BANK:") - 32).Length > 24 ? line.Substring(line.IndexOf("BANK:") + 32, line.IndexOf("EFFECT") - line.IndexOf("BANK:") - 32).Substring(0, 24).Trim() : line.Substring(line.IndexOf("BANK:") + 32, line.IndexOf("EFFECT") - line.IndexOf("BANK:") - 32).Trim());
                                dr["EffectiveDate"] = line.Substring(line.IndexOf("EFFECT DATE:") + 12, 9).Trim();
                            }

                            if (line.Contains("ACH TRACE NUMBER:"))
                            {
                                dr["ACHTraceNumber"] = line.Substring(line.IndexOf("ACH TRACE NUMBER:") + 17, 24).Trim();
                            }

                            if (line.Contains("| TRACE NUMBER:"))
                            {
                                dr["TraceNumber"] = line.Substring(line.IndexOf("| TRACE NUMBER:") + 17, 24).Trim();
                            }

                            if (line.Contains("DISC AMT  |  IV:"))
                            {
                                tempInvNumber = line.Substring(line.IndexOf("DISC AMT  |  IV:") + 16, line.LastIndexOf("|") - line.IndexOf("DISC AMT  |  IV:") - 16).Trim();
                            }
                            if (line.Contains("DISC AMT  |  ZZ:"))
                            {
                                tempInvNumber = line.Substring(line.IndexOf("DISC AMT  |  ZZ:") + 16, line.LastIndexOf("|") - line.IndexOf("DISC AMT  |  ZZ:") - 16).Trim();
                            }

                            if ((line.Contains("CM:") || line.Contains("NA0") || line.Contains("CR0") || line.Contains("RTN0")) && !line.Contains("PD AMT"))
                            {
                                if (drApply != null)
                                    dtEFTNotePadItem.Rows.Add(drApply);

                                drApply = dtEFTNotePadItem.NewRow();
                                drApply["PaymentRowId"] = dr["PaymentRowId"];
                                drApply["ApplyRowId"] = rowCountApply;
                                drApply["InvoiceAmount"] = 0;
                                tempInvNumber = string.Empty;
                                rowCountApply++;

                                int leastIndexOfInvoice = 0;
                                if (line.IndexOf("CM:") > 0)
                                    leastIndexOfInvoice = line.IndexOf("CM:");
                                if (line.IndexOf("NA0") > 0)
                                {
                                    if (leastIndexOfInvoice == 0)
                                        leastIndexOfInvoice = line.IndexOf("NA0");
                                    else if (line.IndexOf("NA0") < leastIndexOfInvoice)
                                        leastIndexOfInvoice = line.IndexOf("NA0");
                                }
                                if (line.IndexOf("CR0") > 0)
                                {
                                    if (leastIndexOfInvoice == 0)
                                        leastIndexOfInvoice = line.IndexOf("CR0");
                                    else if (line.IndexOf("CR0") < leastIndexOfInvoice)
                                        leastIndexOfInvoice = line.IndexOf("CR0");
                                }

                                if (line.IndexOf("RTN0") > 0)
                                {
                                    if (leastIndexOfInvoice == 0)
                                        leastIndexOfInvoice = line.IndexOf("RTN0");
                                    else if (line.IndexOf("RTN0") < leastIndexOfInvoice)
                                        leastIndexOfInvoice = line.IndexOf("RTN0");
                                }

                                drApply["InvoiceNumber"] = line.Substring(leastIndexOfInvoice, line.LastIndexOf("|") - leastIndexOfInvoice).Trim();
                            }

                            if (line.Contains("PD AMT"))
                            {
                                if (drApply != null)
                                {
                                    if (Convert.ToDecimal(drApply["InvoiceAmount"]) > 0 && drApply["InvoiceNumber"] != tempInvNumber)
                                    {
                                        dtEFTNotePadItem.Rows.Add(drApply);
                                        drApply = null;
                                    }
                                }

                                if (drApply == null)
                                {
                                    drApply = dtEFTNotePadItem.NewRow();
                                    drApply["PaymentRowId"] = dr["PaymentRowId"];
                                    drApply["InvoiceAmount"] = 0;
                                    drApply["ApplyRowId"] = rowCountApply;
                                    drApply["InvoiceNumber"] = tempInvNumber;
                                    rowCountApply++;
                                }
                                drApply["InvoiceAmount"] = Convert.ToDecimal(line.Substring(line.IndexOf("$") + 1, line.IndexOf("PD AMT") - line.IndexOf("$") - 1));
                            }
                        }


                        if (dr != null)
                        {
                            dtEFTRemit.Rows.Add(dr);
                        }

                        if (drApply != null)
                            dtEFTNotePadItem.Rows.Add(drApply);

                        logMessage.Append(DateTime.Now + " - " + receivablesRequest.EFTPaymentFilePath + "contents are converted into datatables");
                        receivalblesDataAccess.SaveRemittanceFileDetails(dtEFTRemit, dtEFTNotePadItem, receivablesRequest.UserName, Path.GetFileName(receivablesRequest.EFTPaymentFilePath), receivablesRequest.CompanyId);
                        logMessage.Append(DateTime.Now + " - Original contents of " + receivablesRequest.EFTPaymentFilePath + " file are saved");
                    }

                    #endregion


                }
                else
                {
                    objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                    objReceivablesResponse.ErrorMessage = "File name is already exists.please select another file.";
                    logMessage.Append(DateTime.Now + " - File name is already exists");
                }
            }
            catch (Exception ex)
            {
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                objReceivablesResponse.ErrorMessage = ex.Message.ToString().Trim();
                logMessage.Append(DateTime.Now + " Exception Message- " + ex.Message);
                logMessage.Append(DateTime.Now + " Exception Stack Trace- " + ex.StackTrace);
            }
            finally
            {
                logMessage.Append(DateTime.Now + " - ImportEFTRemittanceReport method in Chempoint.GP.ReceivablesBusiness.ReceivablesBusiness class is ended");
                objReceivablesResponse.LogMessage = logMessage.ToString();
            }
            return objReceivablesResponse;
        }

        /**************Private methods*******************/
        public string IsEftFileAlreadyProcessed(ReceivablesRequest receivablesRequest)
        {
            string status = string.Empty;
            IReceivablesRepository receivalblesDataAccess = null;
            try
            {
                receivalblesDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(receivablesRequest.ConnectionString);
                status = receivalblesDataAccess.IsEftFileAlreadyProcessed(receivablesRequest.EFTPaymentFileName, receivablesRequest.BatchId, receivablesRequest.CompanyId);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return status;
        }

        /// <summary>
        /// To get Open EFT transaction details
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <returns></returns>
        public DataSet GetEftOpenTransactionDetail(ReceivablesRequest receivablesRequest)
        {

            IReceivablesRepository receivalblesDataAccess = null;
            DataSet ds = new DataSet();
            try
            {
                receivalblesDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(receivablesRequest.ConnectionString);
                ds = (DataSet)receivalblesDataAccess.GetEftOpenTransactions(receivablesRequest.CompanyId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds;
        }

        #endregion Bank_summary_and_CTX_import


        /// <summary>
        /// To get all Eft transaction details
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <returns></returns>
        public DataSet GetEftTransactionDetails(ReceivablesRequest receivablesRequest)
        {

            IReceivablesRepository receivalblesDataAccess = null;
            DataSet ds = new DataSet();
            try
            {
                receivalblesDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(receivablesRequest.ConnectionString);
                ds = (DataSet)receivalblesDataAccess.GetEftTransactionDetails(receivablesRequest.CompanyId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds;
        }

        /// <summary>
        /// Convert List<T> to datatable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        DataTable ToDataTable<T>(List<T> items)
        {

            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        /// <summary>
        /// Convert CSV file to datatable
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isFirstRowHeader"></param>
        /// <returns></returns>
        DataTable GetDataTableFromCsv(string path, bool isFirstRowHeader)
        {

            DataTable dataTable = new DataTable();

            int rowId = 0;

            logMessage.Append(DateTime.Now + " - GetDataTableFromCsv method from Chempoint.GP.ReceivablesBusiness.ReceivablesBusiness class is started");
            StreamReader reader = new StreamReader(path);

            while (!reader.EndOfStream)
            {
                string inputLine = reader.ReadLine();
                string outputLine = inputLine.Replace('\t', ',');
                string[] columns = inputLine.Split('\t');
                if (rowId++ == 0)
                {
                    foreach (string column in columns)
                    {
                        dataTable.Columns.Add(column);
                    }
                    dataTable.Columns.Add("RowID");
                }
                else
                {
                    int columnId = 0;
                    DataRow dr = dataTable.NewRow();
                    foreach (string column in columns)
                    {
                        dr[columnId] = column;
                        columnId++;
                    }
                    dr["RowID"] = rowId.ToString();
                    dataTable.Rows.Add(dr);
                }

            }

            reader.Close();

            logMessage.Append(DateTime.Now + " - GetDataTableFromCsv method from Chempoint.GP.ReceivablesBusiness.ReceivablesBusiness class is completed");

            return dataTable;
        }


        /// <summary>
        /// Convert DataTable to String
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static string ConvertDataTableToString(DataTable dataTable)
        {
            string data = string.Empty;

            try
            {
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }


        /// <summary>
        /// To send mail to Account Receivables
        /// </summary>
        /// <param name="dtMailBody"></param>
        /// <returns></returns>
        bool SendMailToAccountReceivables(DataTable dtMailBody)
        {
            bool bIsMailSent = false;
            try
            {

                logMessage.Append(DateTime.Now + " - SendMailToAccountReceivables method from Chempoint.GP.ReceivablesBusiness.ReceivablesBusiness class is started");

                SendEmailRequest objSendEmailRequest = new SendEmailRequest();
                objSendEmailRequest.EmailInformation = new EMailInformation();
                objSendEmailRequest.FileName = ConfigurationManager.AppSettings["EFTReportFileName"];
                objSendEmailRequest.Report = ConvertDataTableToString(dtMailBody);
                objSendEmailRequest.EmailConfigID = Convert.ToInt32(ConfigurationManager.AppSettings["EFTEmailConfigID"]);
                objSendEmailRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
                objSendEmailRequest.EmailInformation.EmailFrom = ConfigurationManager.AppSettings["EFTEMailFrom"];
                objSendEmailRequest.EmailInformation.SmtpAddress = ConfigurationManager.AppSettings["EFTSMTPServer"];
                objSendEmailRequest.EmailInformation.Signature = ConfigurationManager.AppSettings["EFTEmailSignature"];
                objSendEmailRequest.EmailInformation.Body = "ETF payment Details";
                objSendEmailRequest.EmailInformation.IsDataTableBodyRequired = true;
                var reponse = new EmailBusiness().SendEmail(objSendEmailRequest);

                logMessage.Append(DateTime.Now + " -Text starts with WireType details mailed to Account Receivables");
                logMessage.Append(DateTime.Now + " - SendMailToAccountReceivables method from Chempoint.GP.ReceivablesBusiness.ReceivablesBusiness class is completed");

            }

            catch (Exception ex)
            {
                throw ex;
            }
            return bIsMailSent;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse ValidateEFTLine(ReceivablesRequest receivablesRequest)
        {
            ReceivablesResponse objReceivablesResponse = null;
            IReceivablesRepository receivalblesDataAccess = null;
            DataTable dtSummaryLineRef = null, dtSummaryLineItemRef = null;
            List<EFTCashReceipt> paymentRemit = new List<EFTCashReceipt>();

            try
            {
                objReceivablesResponse = new ReceivablesResponse();
                dtSummaryLineRef = new DataTable();
                dtSummaryLineItemRef = new DataTable();
                dtSummaryLineRef = DataTableMapper.EFTMailReport(receivablesRequest, DataColumnMapping.EFTTransactionDetails);
                dtSummaryLineItemRef = DataTableMapper.EFTMailReport(receivablesRequest, DataColumnMapping.EFTTransactionMapDetails);
                receivalblesDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(receivablesRequest.ConnectionString);
                objReceivablesResponse = receivalblesDataAccess.ValidateEFTLine(dtSummaryLineRef, dtSummaryLineItemRef, receivablesRequest.CompanyId);
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;

            }
            catch (Exception ex)
            {
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                objReceivablesResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return objReceivablesResponse;

        }


        public void ConverteTablimitedFileToCSV(string strTabLimitedFilePath, string strOutputFile)
        {
            try
            {

                logMessage.Append(DateTime.Now + " - ConverteTablimitedFileToCSV method from Chempoint.GP.ReceivablesBusiness.ReceivablesBusiness class is started");
                StreamReader reader = new StreamReader(strTabLimitedFilePath);
                StreamWriter writer = new StreamWriter(strOutputFile);

                while (!reader.EndOfStream)
                {
                    string inputLine = reader.ReadLine();
                    string outputLine = inputLine.Replace('\t', ',');

                    writer.WriteLine(outputLine);
                }

                reader.Close();
                writer.Close();

                logMessage.Append(DateTime.Now + " - ConverteTablimitedFileToCSV method from Chempoint.GP.ReceivablesBusiness.ReceivablesBusiness class is completed");

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        #region Remittance

        public ReceivablesResponse ValidatePaymentRemittance(ReceivablesRequest receivablesRequest)
        {
            ReceivablesResponse objReceivablesResponse = null;
            IReceivablesRepository receivalblesDataAccess = null;
            DataSet paymentRemitResponseDT = new DataSet();
            List<EFTCustomerPayment> paymentRemit = new List<EFTCustomerPayment>();

            try
            {
                objReceivablesResponse = new ReceivablesResponse();

                receivalblesDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(receivablesRequest.ConnectionString);
                paymentRemitResponseDT = receivalblesDataAccess.ValidatePaymentRemittance(receivablesRequest);
                paymentRemit = ConvertToList<EFTCustomerPayment>(paymentRemitResponseDT.Tables[0]);
                objReceivablesResponse.EFTCustomerPaymentList = paymentRemit;
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;

            }
            catch (Exception ex)
            {
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                objReceivablesResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return objReceivablesResponse;
        }

        /// <summary>
        /// Convert Datatable to List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<T> ConvertToList<T>(DataTable dt)
        {

            var columnNames = dt.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name))
                    {
                        
                        if (row[pro.Name] != DBNull.Value)
                            pro.SetValue(objT, row[pro.Name]);
                    }
                }
                return objT;
            }).ToList();
        }

        /// <summary>
        /// Push To GP Business logic
        /// </summary>
        /// <param name="salesRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse PushEftTransactionsToGP(ReceivablesRequest eftRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            eftRequest.ThrowIfNull("eftRequest");
            ReceivablesResponse eftResponse = null;
            IReceivablesRepository eftDataAccess = null;
            bool isPaymentCreated = false;
            bool isPaymentError = false;
            bool isApplyCreated = false;
            bool isApplyError = false;
            try
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Action Type " + eftRequest.Actiontype.ToString().Trim());
                if (eftRequest.Actiontype == 3)
                {
                    StringBuilder multiCurrencyMailMessage = new StringBuilder();
                    try
                    {
                        int Eftid;
                        DataTable inputDataTable = null;
                        DataSet ApplyPaymentDetail = new DataSet();
                        List<string> UnlockedOrders = new List<string>();

                        multiCurrencyMailMessage.Append("<html><h3>Please apply the following payments to corresponding posted invoices manually</h3><br>");
                        multiCurrencyMailMessage.Append("<table border=1><tr><th>CID Number</th><th>Customer Name</th><th>Invoice Number</th><th>Applied Amount</th><th>Document Number</th></tr>");
                        DataTable applyTable = new DataTable();
                        DataTable inputPaymentDataTable = new DataTable();
                        DataTable inputApplyDataTable = new DataTable();


                        eftResponse = new ReceivablesResponse();
                        eftDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(eftRequest.ConnectionString);
                        eftResponse = eftDataAccess.GetPushToGP(eftRequest, 3);
                        inputDataTable = new DataTable();
                        inputDataTable = eftResponse.ApplyPaymentDetail.Tables[0].Clone();

                        ApplyPaymentDetail = eftResponse.ApplyPaymentDetail.Copy();
                        if (eftResponse.ApplyPaymentDetail == null || eftResponse.ApplyPaymentDetail.Tables.Count == 0)
                        {
                            eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.NoRecord;
                        }
                        else
                        {
                            //Multi currency order
                            foreach (DataRow theMulticurrencyRow in ApplyPaymentDetail.Tables[1].Rows)
                            {
                                multiCurrencyMailMessage.AppendLine("<tr><td>" + theMulticurrencyRow["CustomerNumber"].ToString().Trim() +
                                                                      "</td><td>" + theMulticurrencyRow["CustomerName"].ToString().Trim() +
                                                                      "</td><td>" + theMulticurrencyRow["ApplyToDocumentNumber"].ToString().Trim() +
                                                                      "</td><td>" + theMulticurrencyRow["CurrencySymbol"].ToString().Trim() + theMulticurrencyRow["ApplyAmount"].ToString().Trim() +
                                                                      "</td><td>" + theMulticurrencyRow["DocumentNumber"].ToString().Trim() +
                                                                      "</td></tr>");
                            }
                            DataView paymentView = new DataView(ApplyPaymentDetail.Tables[0]);

                            DataTable specificPaymentDT = paymentView.ToTable(true, "ReferenceNumber", "EftId", "DocumentDate", "BachNumber", "GLPostedDate", "CurrencyId", "CustomerNumber", "CheckBookNumber", "ORTRXAMT", "CheckbookID");

                            //Valid payment process one by one..
                            foreach (DataRow theRow in specificPaymentDT.Rows)
                            {
                                inputPaymentDataTable = specificPaymentDT.Clone();
                                inputPaymentDataTable.Rows.Add(theRow.ItemArray);

                                Eftid = Convert.ToInt32(theRow["EftId"]);

                                string referenceNumber = theRow["ReferenceNumber"].ToString().Trim();

                                // Get the next avialable Payment number
                                string nextAvailablePaymentNumber = eftDataAccess.GetNextPaymentNumber(9, eftRequest.AuditInformation.CompanyId);

                                string batchNumber = eftRequest.BatchId;

                                string inputPaymentXml = SerializeToString(inputPaymentDataTable);
                                inputPaymentDataTable.Rows.Clear();
                                inputPaymentXml = inputPaymentXml.Replace("<DocumentElement>", "<Payment>");
                                inputPaymentXml = inputPaymentXml.Replace("</DocumentElement>", "</Payment>");
                                inputPaymentXml = inputPaymentXml.Replace("<Table>", "<PaymentLine>");
                                inputPaymentXml = inputPaymentXml.Replace("</Table>", "</PaymentLine>");
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Input Payment XML: " + inputPaymentXml);

                                logMessage.AppendLine(DateTime.Now.ToString() + " - Started applying cash to open orders: " + referenceNumber);

                                // View aplly documetn from specific Reference number
                                DataView specificOrderDV = new DataView(ApplyPaymentDetail.Tables[0]);
                                specificOrderDV.RowFilter = "ReferenceNumber='" + referenceNumber.ToString().Trim() + "'";
                                inputApplyDataTable = specificOrderDV.ToTable();

                                DataTable specificApplyDT = specificOrderDV.ToTable(false, "EftId", "DocumentNumber", "DocumentType", "CustomerNumber", "CustomerName", "ApplyToDocumentNumber", "ApplyToDocumentType", "ApplyAmount", "ApplyDate", "ApplyPostingDate", "Discount", "WriteOffAmount", "ErrorId", "ErrorDescription", "CurrencyID", "CurrencySymbol");

                                string inputApplyXml = SerializeToString(specificApplyDT);

                                inputApplyXml = inputApplyXml.Replace("<DocumentElement>", "<Order>");
                                inputApplyXml = inputApplyXml.Replace("</DocumentElement>", "</Order>");
                                inputApplyXml = inputApplyXml.Replace("<Table>", "<Details>");
                                inputApplyXml = inputApplyXml.Replace("</Table>", "</Details>");
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Input Apply XML: " + inputApplyXml);
                                inputApplyXml = "<EFT>" + inputPaymentXml + inputApplyXml + "</EFT>";
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Input XML: " + inputApplyXml);
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Creating eConnect XML");

                                string eConnectXml = TransformForEft(inputApplyXml, eftRequest.AuditInformation.CompanyId == 1 ? "CHMPT" : "CPEUR", eftRequest, nextAvailablePaymentNumber);

                                logMessage.AppendLine(DateTime.Now.ToString() + " - Econnect XML : " + eConnectXml);
                                if (eConnectXml != string.Empty)
                                {
                                    eConnectMethods eConObj = null;
                                    bool result = false;
                                    try
                                    {
                                        eConObj = new eConnectMethods();
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - Initializing Econnect Object");

                                        //string tranResult = eConObj.CreateTransactionEntity(soRequest.NAEconnectConnectionString, eConnectXml);  
                                        result = eConObj.eConnect_EntryPoint(eftRequest.AuditInformation.CompanyId == 1 ? eftRequest.NAEconnectConnectionString : eftRequest.EUEconnectConnectionString, Microsoft.Dynamics.GP.eConnect.EnumTypes.ConnectionStringType.SqlClient, eConnectXml, Microsoft.Dynamics.GP.eConnect.EnumTypes.SchemaValidationType.XSD, "");

                                        if (result == true)
                                        {
                                            isPaymentCreated = true;
                                            isApplyCreated = true;
                                            eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Reference Number " + referenceNumber);
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Payment Number " + nextAvailablePaymentNumber);
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Payment has been successfully Created ");
                                            logMessage.AppendLine(DateTime.Now.ToString() + " --------------------------------------.");
                                        }
                                        else
                                        {
                                            isPaymentError = true;
                                            isApplyError = true;
                                            eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Payment/Apply Failed to Created");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        isPaymentError = true;
                                        isApplyError = true;
                                        eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                                        logMessage.AppendLine(DateTime.Now.ToString() + "Error Message " + ex.Message);
                                        logMessage.AppendLine(DateTime.Now.ToString() + "Inner Exception " + ex.InnerException);
                                        logMessage.AppendLine(DateTime.Now.ToString() + "Error - Payment/Apply Creation Failed ");
                                        logMessage.AppendLine(DateTime.Now.ToString() + " --------------------------------------.");
                                    }
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                        logMessage.AppendLine(DateTime.Now.ToString() + "Error Message " + ex.Message);
                    }
                    finally
                    {
                        if (multiCurrencyMailMessage.ToString().Contains("<tr><td>") || multiCurrencyMailMessage.ToString().Contains("<tr><td colspan=3>"))
                        {
                            multiCurrencyMailMessage.Append("</table>");
                            eftRequest.SalesPriorityOrdersEmail.Body = multiCurrencyMailMessage.ToString();
                            eftRequest.SalesPriorityOrdersEmail.Subject = eftRequest.SalesPriorityOrdersEmail.Subject + " - " + eftRequest.Source.ToString().Trim();

                            if (new EmailHelper().SendMail(eftRequest.SalesPriorityOrdersEmail))
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Multi-Currency mail has been successfully sent");
                            else
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Multi-Currency mail failed to send.");
                        }
                    }
                }
                else if (eftRequest.Actiontype == 2)
                {
                    int Eftid;
                    DataTable inputDataTable = null;
                    eftResponse = new ReceivablesResponse();
                    eftDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(eftRequest.ConnectionString);
                    eftResponse = eftDataAccess.GetPushToGP(eftRequest, 2);

                    if (eftResponse.ApplyPaymentDetail == null || eftResponse.ApplyPaymentDetail.Tables.Count == 0)
                    {
                        eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.NoRecord;
                    }
                    else
                    {
                        inputDataTable = new DataTable();
                        inputDataTable = eftResponse.ApplyPaymentDetail.Tables[0].Clone();
                        foreach (DataRow theRow in eftResponse.ApplyPaymentDetail.Tables[0].Rows)
                        {

                            Eftid = Convert.ToInt32(theRow["EftId"]);
                            inputDataTable.Rows.Add(theRow.ItemArray);

                            string referenceNumber = theRow["ReferenceNumber"].ToString().Trim();
                            // Get the next avialable Payment number
                            string nextAvailablePaymentNumber = eftDataAccess.GetNextPaymentNumber(9, eftRequest.AuditInformation.CompanyId);

                            string batchNumber = eftRequest.BatchId;

                            string inputXml = SerializeToString(inputDataTable);

                            inputXml = inputXml.Replace("<DocumentElement>", "<Payment>");
                            inputXml = inputXml.Replace("</DocumentElement>", "</Payment>");
                            inputXml = inputXml.Replace("<Table>", "<PaymentLine>");
                            inputXml = inputXml.Replace("</Table>", "</PaymentLine>");
                            inputXml = "<EFT>" + inputXml + "</EFT>";
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Input XML: " + inputXml);
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Creating eConnect XML");

                            string eConnectXml = TransformForEft(inputXml, eftRequest.AuditInformation.CompanyId == 1 ? "CHMPT" : "CPEUR", eftRequest, nextAvailablePaymentNumber);

                            logMessage.AppendLine(DateTime.Now.ToString() + " - Econnect XML : " + eConnectXml);

                            inputDataTable.Rows.Clear();
                            // Create FO in GP
                            if (eConnectXml != string.Empty)
                            {
                                eConnectMethods eConObj = null;
                                bool result = false;
                                try
                                {
                                    eConObj = new eConnectMethods();
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Initializing Econnect Object");

                                    //string tranResult = eConObj.CreateTransactionEntity(soRequest.NAEconnectConnectionString, eConnectXml);  
                                    result = eConObj.eConnect_EntryPoint(eftRequest.AuditInformation.CompanyId == 1 ? eftRequest.NAEconnectConnectionString : eftRequest.EUEconnectConnectionString, Microsoft.Dynamics.GP.eConnect.EnumTypes.ConnectionStringType.SqlClient, eConnectXml, Microsoft.Dynamics.GP.eConnect.EnumTypes.SchemaValidationType.XSD, "");

                                    if (result == true)
                                    {
                                        isPaymentCreated = true;
                                        eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - Reference Number " + referenceNumber);
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - Payment Number " + nextAvailablePaymentNumber);
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - Payment has been successfully Created ");
                                        logMessage.AppendLine(DateTime.Now.ToString() + " --------------------------------------.");
                                    }
                                    else
                                    {
                                        isPaymentError = true;
                                        eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - Payment Failed to Created");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    isPaymentError = true;
                                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                                    logMessage.AppendLine(DateTime.Now.ToString() + "Error Message " + ex.Message);
                                    logMessage.AppendLine(DateTime.Now.ToString() + "Inner Exception " + ex.InnerException);
                                    logMessage.AppendLine(DateTime.Now.ToString() + "Error - Payment Creation Failed ");
                                    logMessage.AppendLine(DateTime.Now.ToString() + " --------------------------------------.");
                                }
                            }
                        }
                    }

                }
                else if (eftRequest.Actiontype == 1)
                {
                    int Eftid = 0;
                    eftResponse = new ReceivablesResponse();
                    StringBuilder failureMailMessage = new StringBuilder();
                    failureMailMessage.Append("<html><h3>Following are the error(s) occured while applying cash to open orders through cash application process engine</h3><br>");
                    failureMailMessage.Append("<table border=1><tr><th>CID Number</th><th>Customer Name</th><th>Invoice Number</th><th>Applied Amount</th><th>Document Number</th><th>Error Description</th></tr>");

                    StringBuilder multiCurrencyMailMessage = new StringBuilder();
                    multiCurrencyMailMessage.Append("<html><h3>Please apply the following payments to corresponding posted invoices manually</h3><br>");
                    multiCurrencyMailMessage.Append("<table border=1><tr><th>CID Number</th><th>Customer Name</th><th>Invoice Number</th><th>Applied Amount</th><th>Document Number</th></tr>");

                    try
                    {
                        eftDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(eftRequest.ConnectionString);

                        //Fetching Record from SP
                        eftResponse = eftDataAccess.GetPushToGP(eftRequest, 1);
                        List<string> UnlockedOrders = new List<string>();
                        if (eftResponse.ApplyPaymentDetail == null || eftResponse.ApplyPaymentDetail.Tables.Count == 0)
                        {
                            eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.NoRecord;
                        }
                        else
                        {
                            DataSet ApplyPaymentDetail = new DataSet();
                            ApplyPaymentDetail = eftResponse.ApplyPaymentDetail.Copy();

                            foreach (DataRow theRow in ApplyPaymentDetail.Tables[0].Rows)
                            {

                                if (!UnlockedOrders.Contains(theRow["DocumentNumber"].ToString().Trim()))
                                {
                                    UnlockedOrders.Add(theRow["DocumentNumber"].ToString().Trim());
                                }
                            }

                            foreach (DataRow theRow in ApplyPaymentDetail.Tables[1].Rows)
                            {
                                multiCurrencyMailMessage.AppendLine("<tr><td>" + theRow["CustomerNumber"].ToString().Trim() +
                                                                      "</td><td>" + theRow["CustomerName"].ToString().Trim() +
                                                                      "</td><td>" + theRow["ApplyToDocumentNumber"].ToString().Trim() +
                                                                      "</td><td>" + theRow["CurrencySymbol"].ToString().Trim() + theRow["ApplyAmount"].ToString().Trim() +
                                                                      "</td><td>" + theRow["DocumentNumber"].ToString().Trim() +
                                                                      "</td></tr>");
                            }

                            if (UnlockedOrders.Count > 0)
                            {
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Document Numbers : " + String.Join(",", (string[])UnlockedOrders.ToArray()));
                            }

                            //for every order, transfer to FO
                            if (UnlockedOrders.Count > 0)
                            {
                                foreach (string documentNumber in UnlockedOrders)
                                {
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - --------------------------------------------------");
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Started applying cash to open orders: " + documentNumber);

                                    // Get specific order details to view
                                    DataView specificOrderDV = new DataView(ApplyPaymentDetail.Tables[0]);
                                    specificOrderDV.RowFilter = "DocumentNumber='" + documentNumber + "'";
                                    DataTable inputDataTable = specificOrderDV.ToTable();


                                    //prepare Econnect XML
                                    string inputXml = SerializeToString(inputDataTable);
                                    inputXml = inputXml.Replace("<DocumentElement>", "<Order>");
                                    inputXml = inputXml.Replace("</DocumentElement>", "</Order>");
                                    inputXml = inputXml.Replace("<Table>", "<Details>");
                                    inputXml = inputXml.Replace("</Table>", "</Details>");

                                    inputXml = "<EFT>" + inputXml + "</EFT>";
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Input XML: " + inputXml);
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Creating eConnect XML");

                                    string eConnectXml = TransformForCash(inputXml, eftRequest.AuditInformation.CompanyId == 1 ? "CHMPT" : "CPEUR", eftRequest.EFTPaymentAndApplyStyleSheetPath, eftRequest.Actiontype.ToString().Trim());
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Econnect XML : " + eConnectXml);

                                    // Create FO in GP
                                    if (eConnectXml != string.Empty)
                                    {
                                        eConnectMethods eConObj = null;
                                        bool result = false;
                                        try
                                        {
                                            eConObj = new eConnectMethods();
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Initializing Econnect Object");

                                            //string tranResult = eConObj.CreateTransactionEntity(soRequest.NAEconnectConnectionString, eConnectXml);  
                                            result = eConObj.eConnect_EntryPoint(eftRequest.AuditInformation.CompanyId == 1 ? eftRequest.NAEconnectConnectionString : eftRequest.EUEconnectConnectionString, Microsoft.Dynamics.GP.eConnect.EnumTypes.ConnectionStringType.SqlClient, eConnectXml, Microsoft.Dynamics.GP.eConnect.EnumTypes.SchemaValidationType.XSD, "");

                                            if (result == true)
                                            {
                                                isApplyCreated = true;
                                                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                                                UpdateStatusForCash(ApplyPaymentDetail.Tables[0], "cash has been successfully applied to document number", "0", documentNumber, ref failureMailMessage);

                                                logMessage.AppendLine(DateTime.Now.ToString() + " - cash has been successfully applied to document number : " + documentNumber);
                                            }
                                            else
                                            {
                                                isApplyError = true;
                                                UpdateStatusForCash(ApplyPaymentDetail.Tables[0], "Error : Cash is not linked to document number.", "3", documentNumber, ref failureMailMessage);
                                                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                                                logMessage.AppendLine(DateTime.Now.ToString() + " - Error : Cash is not linked to document number.");
                                                logMessage.AppendLine(DateTime.Now.ToString() + " --------------------------------------.");
                                            }
                                        }
                                        catch (eConnectException econEx)
                                        {
                                            isApplyError = true;
                                            eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                                            string errorCode = string.Empty;
                                            if (econEx.Message.Contains("Error Number = "))
                                            {
                                                logMessage.AppendLine(DateTime.Now.ToString() + " - Status : Error while applying cash to open orders into GP");

                                                errorCode = econEx.Message.Substring(econEx.Message.IndexOf("Error Number = ") + 15,
                                                    (econEx.Message.IndexOf(" ", econEx.Message.IndexOf("Error Number = ") + 15) - (econEx.Message.IndexOf("Error Number = ") + 15)));

                                                logMessage.AppendLine(DateTime.Now.ToString() + " - Error Code: " + errorCode);
                                            }

                                            UpdateStatusForCash(ApplyPaymentDetail.Tables[0], econEx.Message.Trim(), errorCode, documentNumber, ref failureMailMessage);

                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Econnect Error: " + econEx.Message);
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - -------------------------------------.");
                                        }
                                        catch (System.Data.SqlClient.SqlException sqlEx)
                                        {
                                            isApplyError = true;
                                            eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                                            UpdateStatusForCash(ApplyPaymentDetail.Tables[0], sqlEx.Message.Trim(), "2", documentNumber, ref failureMailMessage);

                                            logMessage.AppendLine(sqlEx.Message);
                                            logMessage.AppendLine(sqlEx.StackTrace);
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - -------------------------------------.");
                                        }
                                        catch (Exception ex)
                                        {
                                            isApplyError = true;
                                            eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                                            UpdateStatusForCash(ApplyPaymentDetail.Tables[0], ex.Message.Trim(), "1", documentNumber, ref failureMailMessage);

                                            logMessage.AppendLine(ex.Message);
                                            logMessage.AppendLine(ex.StackTrace);
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - -------------------------------------.");
                                        }
                                        finally
                                        {
                                            if (eConObj != null)
                                            {
                                                eConObj.Dispose();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        isApplyError = true;
                                        eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - Econnect XML is not generated.");
                                    }
                                }

                            }
                            else
                            {
                                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.NoRecord;
                                logMessage.AppendLine(DateTime.Now.ToString() + " - There is no order to process for cash application.");
                                logMessage.AppendLine(DateTime.Now.ToString() + " ----------------------------------------------.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        isApplyError = true;
                        eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message);
                        logMessage.AppendLine(DateTime.Now.ToString() + " - " + ex.StackTrace);
                        logMessage.AppendLine(DateTime.Now.ToString() + " - " + "-------------------------------------.");
                    }
                    finally
                    {
                        //Send out email to tech team for failures
                        if (failureMailMessage.ToString().Contains("<tr><td>") || failureMailMessage.ToString().Contains("<tr><td colspan=3>"))
                        {
                            failureMailMessage.Append("</table>");
                            eftRequest.SalesOrderFailureEmail.Body = failureMailMessage.ToString();

                            if (new EmailHelper().SendMail(eftRequest.SalesOrderFailureEmail))
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Failure mail has been successfully sent");
                            else
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Failure mail failed to send.");
                        }

                        //Send out email to tech team for failures
                        if (multiCurrencyMailMessage.ToString().Contains("<tr><td>") || multiCurrencyMailMessage.ToString().Contains("<tr><td colspan=3>"))
                        {
                            multiCurrencyMailMessage.Append("</table>");
                            eftRequest.SalesPriorityOrdersEmail.Body = multiCurrencyMailMessage.ToString();
                            eftRequest.SalesPriorityOrdersEmail.Subject = eftRequest.SalesPriorityOrdersEmail.Subject + " - " + eftRequest.Source.ToString().Trim();

                            if (new EmailHelper().SendMail(eftRequest.SalesPriorityOrdersEmail))
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Multi-Currency mail has been successfully sent");
                            else
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Multi-Currency mail failed to send.");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                isApplyError = true;
                isPaymentError = true;
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            finally
            {
                //log the message
                new TextLogger().LogInformationIntoFile(logMessage.ToString(), eftRequest.LoggingPath, eftRequest.LoggingFileName);
            }
            if (eftResponse.Status == Chempoint.GP.Model.Interactions.Sales.ResponseStatus.NoRecord)
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.NoRecord;
            else if (!isPaymentCreated && !isApplyCreated)
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
            else if ((isPaymentCreated && isPaymentError) || (isApplyCreated && isApplyError))
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Partial;
            else if (isPaymentCreated || isApplyCreated)
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            else
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Unknown;
            return eftResponse;
        }

        /// <summary>
        /// Method to transform object into xml string
        /// </summary>
        /// <param name="inputTable"> input data </param>
        /// <returns> input xml </returns>
        private string SerializeToString(DataTable inputTable)
        {
            //// creates the serializer object
            using (StringWriter writer = new StringWriter())
            {
                inputTable.WriteXml(writer);
                return writer.ToString();
            }

        }

        private static string TransformForCash(string tableXml, string companyName, string styleSheetPath, string ActionType)
        {
            // Local variables.
            string transformedXml = string.Empty;
            //Create object for XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            //Create object for XslTransform 
            XslCompiledTransform xslTrans = new XslCompiledTransform();
            //Creating Argument List Object
            XsltArgumentList xsltArgsCash = new XsltArgumentList();

            //Loading ChemMsgEcomOrder XML
            xmlDoc.LoadXml(tableXml);
            xsltArgsCash.AddParam("CompanyID", string.Empty, companyName.ToUpper());
            xsltArgsCash.AddParam("PaymentNumber", string.Empty, "");
            xsltArgsCash.AddParam("BatchNumber", string.Empty, "");
            xsltArgsCash.AddParam("BatchTransaction", string.Empty, "");
            xsltArgsCash.AddParam("BatchTotal", string.Empty, "");
            xsltArgsCash.AddParam("PaymentNumberNumeric", string.Empty, "");
            xsltArgsCash.AddParam("ActionType", string.Empty, ActionType);
            xsltArgsCash.AddParam("UserName", string.Empty, "");
            xslTrans.Load(styleSheetPath);

            //Creating StringWriter Object
            StringWriter strWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);
            //Peroforming transformation
            xslTrans.Transform(xmlDoc, xsltArgsCash, strWriter);
            // Set the transformed xml.
            transformedXml = strWriter.ToString().Trim();
            // Dispose the objects.                                                
            strWriter.Dispose();

            // Return the transformed xml to the caller
            return transformedXml;
        }

        /// <summary>
        /// Transform payment
        /// </summary>
        /// <param name="tableXml"></param>
        /// <param name="companyName"></param>
        /// <param name="styleSheetPath"></param>
        /// <returns></returns>
        private static string TransformForEft(string tableXml, string companyName, ReceivablesRequest eftRequest, string nextPaymentnumber)
        {
            // Local variables.
            string transformedXml = string.Empty;
            //Create object for XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            //Create object for XslTransform 
            XslCompiledTransform xslTrans = new XslCompiledTransform();
            //Creating Argument List Object
            XsltArgumentList xsltArgsCash = new XsltArgumentList();

            //Loading ChemMsgEcomOrder XML
            xmlDoc.LoadXml(tableXml);
            xsltArgsCash.AddParam("CompanyID", string.Empty, companyName.ToUpper());
            xsltArgsCash.AddParam("PaymentNumber", string.Empty, nextPaymentnumber);
            xsltArgsCash.AddParam("BatchNumber", string.Empty, eftRequest.BatchId);
            xsltArgsCash.AddParam("BatchTransaction", string.Empty, eftRequest.PaymentCount);
            xsltArgsCash.AddParam("BatchTotal", string.Empty, eftRequest.ControlAmount);
            xsltArgsCash.AddParam("PaymentNumberNumeric", string.Empty, nextPaymentnumber.Substring(3));
            xsltArgsCash.AddParam("ActionType", string.Empty, eftRequest.Actiontype);
            xsltArgsCash.AddParam("UserName", string.Empty, eftRequest.AuditInformation.CreatedBy);

            xslTrans.Load(eftRequest.EFTPaymentAndApplyStyleSheetPath);

            //Creating StringWriter Object
            StringWriter strWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);
            //Peroforming transformation
            xslTrans.Transform(xmlDoc, xsltArgsCash, strWriter);
            // Set the transformed xml.
            transformedXml = strWriter.ToString().Trim();
            // Dispose the objects.                                                
            strWriter.Dispose();

            // Return the transformed xml to the caller
            return transformedXml;
        }

        /// <summary>
        /// Log the mail content
        /// </summary>
        /// <param name="documentRowId"></param>
        /// <param name="documentNumber"></param>
        /// <param name="vendorId"></param>
        /// <param name="errorMessage"></param>
        private void UpdateStatusForCash(DataTable paymentDv, string errorMessage, string errorId, string documentNumber, ref StringBuilder failureMailMessage)
        {
            try
            {
                IEnumerable<DataRow> rows =
                            from DataRow row in paymentDv.Rows
                            where row.Field<string>("DocumentNumber").ToString().Trim() == documentNumber.ToString()
                            select row;

                if (rows != null && rows.Count() > 0)
                {
                    foreach (var rowitem in rows)
                    {
                        rowitem["ErrorId"] = errorId;
                        rowitem["ErrorDescription"] = string.IsNullOrEmpty(errorMessage) ? string.Empty : errorMessage.Trim();

                        if (errorId != "0")
                        {
                            failureMailMessage.AppendLine("<tr><td>" + rowitem["CustomerNumber"].ToString().Trim() +
                                                         "</td><td>" + rowitem["CustomerName"].ToString().Trim() +
                                                         "</td><td>" + rowitem["ApplyToDocumentNumber"].ToString().Trim() +
                                                         "</td><td>" + rowitem["CurrencySymbol"].ToString().Trim() + rowitem["ApplyAmount"].ToString().Trim() +
                                                         "</td><td>" + documentNumber +
                                                         "</td><td>" + errorMessage.Trim() + "</td></tr>");
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Email Reference Lookup 
        /// </summary>
        /// <param name="receivableRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchEmailReferenceLookup(ReceivablesRequest receivableRequest)
        {
            ReceivablesResponse objReceivablesResponse = null;
            IReceivablesRepository receivalblesDataAccess = null;

            try
            {
                objReceivablesResponse = new ReceivablesResponse();
                receivalblesDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(receivableRequest.ConnectionString);
                objReceivablesResponse = receivalblesDataAccess.FetchEmailReferenceLookup(receivableRequest);
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;

            }
            catch (Exception ex)
            {
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                objReceivablesResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return objReceivablesResponse;
        }

        /// <summary>
        /// Email Reference Line Details  
        /// </summary>
        /// <param name="receivableRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchEmailReferenceScroll(ReceivablesRequest receivableRequest)
        {
            ReceivablesResponse objReceivablesResponse = null;
            IReceivablesRepository receivalblesDataAccess = null;

            try
            {
                objReceivablesResponse = new ReceivablesResponse();
                receivalblesDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(receivableRequest.ConnectionString);
                objReceivablesResponse = receivalblesDataAccess.FetchEmailReferenceScroll(receivableRequest);
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;

            }
            catch (Exception ex)
            {
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                objReceivablesResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return objReceivablesResponse;
        }

        /// <summary>
        /// Delete EFT Email Reference 
        /// </summary>
        /// <param name="receivableRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse DeleteEFTEmailRemittance(ReceivablesRequest receivableRequest)
        {
            ReceivablesResponse objReceivablesResponse = null;
            IReceivablesRepository receivalblesDataAccess = null;

            try
            {
                objReceivablesResponse = new ReceivablesResponse();
                receivalblesDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(receivableRequest.ConnectionString);
                objReceivablesResponse = receivalblesDataAccess.DeleteEFTEmailRemittance(receivableRequest);
            }
            catch (Exception ex)
            {
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                objReceivablesResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return objReceivablesResponse;
        }

        /// <summary>
        /// Save EFT Email Reference 
        /// </summary>
        /// <param name="receivableRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse SaveEmailReferences(ReceivablesRequest receivableRequest)
        {
            ReceivablesResponse objReceivablesResponse = null;
            IReceivablesRepository receivalblesDataAccess = null;

            try
            {
                objReceivablesResponse = new ReceivablesResponse();
                receivalblesDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(receivableRequest.ConnectionString);
                objReceivablesResponse = receivalblesDataAccess.SaveEmailReferences(receivableRequest);
            }
            catch (Exception ex)
            {
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                objReceivablesResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return objReceivablesResponse;
        }
        #endregion Remittance
 /// <summary>
        /// DeleteBankEntryItemReference ...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse DeleteBankEntryEFTTransaction(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            IReceivablesRepository eftDataAccess = null;

            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(eftRequest.ConnectionString);

                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;

                int returnval = eftDataAccess.DeleteBankEntryEFTTransaction(eftRequest);

                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;

            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }

        /// <summary>
        /// DeleteBankEntryItemReference ...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse MapBankEntryToEmailRemittance(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            IReceivablesRepository eftDataAccess = null;
            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.ReceivablesDL.ReceivablesDL(eftRequest.ConnectionString);

                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;

                eftResponse = eftDataAccess.MapBankEntryToEmailRemittance(eftRequest);
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }
    }
}
