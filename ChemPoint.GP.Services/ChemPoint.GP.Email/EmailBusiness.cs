using Chempoint.GP.Infrastructure.Config;
using Chempoint.GP.Model.Interactions.Email;
using ChemPoint.GP.BusinessContracts.Email;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using ChemPoint.GP.Entities.BaseEntities;
using Excel = Microsoft.Office.Interop.Excel;
using ChemPoint.GP.DataContracts.GPUtilities;
using ChemPoint.GP.EmailDL;
using System.IO;

namespace ChemPoint.GP.Email
{
    public class EmailBusiness : ISendEmail
    {
        string bodyMsg = "";
        string mailTo = "";
        string mailCc = "";
        string mailFrom = "";
        string mailSubject = "";
        string smtpServer = "";
        bool attachFile = false;
        string excelFileName = "";

        public SendEmailResponse SendEmail(SendEmailRequest request)
        {
            SendEmailResponse response = new SendEmailResponse();
            StringBuilder logMessage = new StringBuilder();

            DataTable exportDT = new DataTable();
            try
            {
                response.LogMessage += "Action Started for sending Email" + "\n";
                // Getting Configuration Details from DataCentral
                ISendEmailRepository mailConfig = new SendEmailDL(request.ConnectionString);
                response = mailConfig.GetMailConfiguration(request);
                if (response.Status == ResponseStatusForEmail.Success)
                {
                    mailTo = response.EmailInformation.EmailTo;
                    mailCc = response.EmailInformation.EmailCc;
                    mailFrom = request.EmailInformation.EmailFrom;

                    if (!string.IsNullOrEmpty(request.EmailInformation.Subject))
                        mailSubject = request.EmailInformation.Subject;
                    else
                        mailSubject = response.EmailInformation.Subject;

                    smtpServer = request.EmailInformation.SmtpAddress;
                    attachFile = response.EmailInformation.HasAttachment;

                    //Making Tabular format for Data Table values                    
                    bodyMsg += response.EmailInformation.Body.Trim();

                    bodyMsg += Configuration.BREAK_LINE.Trim();
                    bodyMsg += request.EmailInformation.Body;
                    string FileName = request.FileName;

                    //converting Data Tale value into string
                    if (!string.IsNullOrEmpty(request.Report))
                        exportDT = ConvertStringToDataTable(request.Report);

                    if (attachFile)
                    {
                        if (request.FileType == ".xls")
                        {
                            string fileName = ExportDataTableToExcelFile(exportDT, FileName, request);
                            string fileType = request.FileType;
                            excelFileName = fileName + fileType;
                        }
                        else if (request.FileType == ".csv")
                        {
                            string fileName = ExportDataTableToCSV(exportDT, FileName, request);
                            excelFileName = fileName;
                        }
                    }
                    if (request.EmailInformation.IsDataTableBodyRequired)
                    {

                        bodyMsg += Configuration.TABLE_BORDER.Trim();
                        bodyMsg += Configuration.TABLEROW_START.Trim();
                        bodyMsg += "<bold>";
                        foreach (DataColumn columnHeader in exportDT.Columns)
                        {
                            bodyMsg += Configuration.TABLECOLH_START.Trim();
                            bodyMsg += columnHeader.ColumnName;
                            bodyMsg += Configuration.TABLECOLH_END.Trim();
                        }
                        bodyMsg += "</bold>";
                        bodyMsg += Configuration.TABLEROW_END.Trim();

                        foreach (DataRow value in exportDT.Rows)
                        {
                            bodyMsg += Configuration.TABLEROW_START.Trim();
                            foreach (var item in value.ItemArray)
                            {
                                bodyMsg += Configuration.TABLECOL_START.Trim();
                                bodyMsg += item;
                                bodyMsg += Configuration.TABLECOL_END.Trim();
                            }
                            bodyMsg += Configuration.TABLEROW_END.Trim();

                        }

                        bodyMsg += Configuration.TABLE_END.Trim();
                    }
                    bodyMsg += request.EmailInformation.Signature.Trim();
                    bodyMsg += Configuration.BREAK_LINE.Trim();
                    bodyMsg += Configuration.HTMLEND.Trim();
                    response.LogMessage += System.DateTime.Now.ToString() + "--" + "Data Fetched Successfully" + "\n";
                    response.LogMessage += System.DateTime.Now.ToString() + "--" + "Mail Ready to send" + "\n";

                    #region Code Block to send email
                    MailMessage mail = new MailMessage();
                    try
                    {
                        SmtpClient smtpClient = new SmtpClient(smtpServer.Trim());
                        mail.From = new MailAddress(mailFrom.Trim());

                        mail.Subject = mailSubject.Trim();
                        AddAddress(mail.To, mailTo.Trim());
                        AddAddress(mail.CC, mailCc.Trim());


                        mail.IsBodyHtml = true;
                        mail.Body = bodyMsg;

                        if (attachFile == true)
                        {
                            if (excelFileName != null)
                            {
                                System.Net.Mail.Attachment attachment;
                                attachment = new System.Net.Mail.Attachment(excelFileName, MediaTypeNames.Application.Octet);
                                mail.Attachments.Add(attachment);
                            }
                        }
                        smtpClient.Send(mail);

                        response.IsMailSent = true;
                        response.LogMessage += System.DateTime.Now.ToString() + "--" + "Mail Sent Successfully" + "\n";
                        response.LogMessage += System.DateTime.Now.ToString() + "--" + "Mail Process Completed" + "\n";
                    }
                    catch (Exception exc)
                    {
                        response.LogMessage += System.DateTime.Now.ToString() + "--" + exc.Message + "\n";
                        response.IsMailSent = true;
                    }


                    finally
                    {
                        mail.Dispose();
                    }
                    #endregion
                }

            }
            catch (Exception ex)
            {
                response.LogMessage += System.DateTime.Now.ToString() + "--" + ex.Message + "\n";
                response.LogMessage += System.DateTime.Now.ToString() + "--" + ex.StackTrace + "\n";
            }
            finally
            {
                if (attachFile == true && System.IO.File.Exists(excelFileName))
                    System.IO.File.Delete(excelFileName);
            }
            return response;
        }


        /// <summary>
        /// Methoed to convert string to datatable
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Method used to convert datatable to excel file
        /// </summary>
        /// <param name="skuDT"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string ExportDataTableToExcelFile(DataTable skuDT, string fileName, SendEmailRequest request)
        {
            Excel.Worksheet worksheet = null;
            Excel.Application excelApp = null;
            Excel.Workbook xlWorkBook = null;
            Excel.Range cellRange;

            excelApp = new Excel.Application();
            xlWorkBook = excelApp.Workbooks.Add(true);
            worksheet = (Excel.Worksheet)xlWorkBook.ActiveSheet;

            DataTable table = skuDT;

            int ColumnIndex = 0;
            foreach (DataColumn col in table.Columns)
            {
                ColumnIndex++;
                worksheet.Cells[1, ColumnIndex] = col.ColumnName;
                cellRange = worksheet.Cells.get_Range((object)worksheet.Cells[1, ColumnIndex], (object)worksheet.Cells[1, ColumnIndex]);

                cellRange.Font.Bold = true;
                cellRange.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlThin, Excel.XlColorIndex.xlColorIndexAutomatic, Excel.XlColorIndex.xlColorIndexAutomatic);

                cellRange.Columns.AutoFit(); // Columns to fit according to size of header
                if (ColumnIndex == 6)
                    cellRange.ColumnWidth = 65; // Column values greater than header so specified the width
            }
            // fills the excel row
            int rowIndex = 0;
            foreach (DataRow row in table.Rows)
            {
                rowIndex++;
                ColumnIndex = 0;
                foreach (DataColumn col in table.Columns)
                {
                    ColumnIndex++;
                    excelApp.Cells[rowIndex + 1, ColumnIndex] = row[col.ColumnName].ToString().Trim();
                    cellRange = worksheet.Cells.get_Range((object)worksheet.Cells[rowIndex + 1, ColumnIndex], (object)worksheet.Cells[rowIndex + 1, ColumnIndex]);
                    cellRange.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlThin, Excel.XlColorIndex.xlColorIndexAutomatic, Excel.XlColorIndex.xlColorIndexAutomatic);
                    if (ColumnIndex == 3 || ColumnIndex == 5)    // Added for numbers
                        cellRange.NumberFormat = "@";
                }
            }

            worksheet = (Excel.Worksheet)excelApp.ActiveSheet;
            string filePath = request.FilePath;
            fileName = filePath + fileName + "_" + "_" + DateTime.Now.ToString("yyyyMMdd", CultureInfo.CurrentCulture).Trim();

            string fileType = request.FileType;

            if (System.IO.File.Exists(fileName + fileType))
                System.IO.File.Delete(fileName + fileType);

            worksheet.SaveAs(fileName, Excel.XlFileFormat.xlExcel8, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing);

            xlWorkBook.Close(false, fileName, Type.Missing);
            releaseObject(excelApp);
            releaseObject(worksheet);

            return fileName;
        }

        /// <summary>
        /// method to release the objects
        /// </summary>
        /// <param name="obj"></param>
        private static void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (ObjectDisposedException)
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objMailAddressCollection"></param>
        /// <param name="strAddress"></param>
        private static void AddAddress(MailAddressCollection mailAddressCollection, string addresses)
        {
            string[] strarrMailAddress = addresses.Split(';');
            foreach (string strAddressPart in strarrMailAddress)
            {
                string strAddressPartTemp = strAddressPart;
                strAddressPartTemp = strAddressPartTemp.Trim();
                if (!string.IsNullOrWhiteSpace(strAddressPartTemp))
                    mailAddressCollection.Add(strAddressPartTemp);
            }
        }

        private static void AddAttachment(MailMessage objMailMessage, String strAttachmentPath)
        {
            objMailMessage.Attachments.Add(new Attachment(strAttachmentPath));
        }

        private static string ExportDataTableToCSV(DataTable dt, string fileName, SendEmailRequest request)
        {
            string filePath = string.Empty;
            try
            {
                filePath = request.FilePath;
                filePath = filePath + fileName + "_" + "_" + DateTime.Now.ToString("yyyyMMdd", CultureInfo.CurrentCulture).Trim() + ".csv";


                if (!File.Exists(filePath))
                {
                    var myFile = File.Create(filePath);
                    myFile.Close();
                }

                //open file
                StreamWriter wr = new StreamWriter(filePath);

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    wr.Write(dt.Columns[i].ToString().ToUpper() + ",");
                }

                wr.WriteLine();

                //write rows to excel file
                for (int i = 0; i < (dt.Rows.Count); i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        wr.Write(dt.Rows[i][j].ToString().Trim() + ",");
                    }
                    //go to next line
                    wr.WriteLine();
                }
                //close file
                wr.Close();
                wr.Dispose();



            }
            catch (Exception ex)
            {
                throw ex;

            }
            finally
            {

                // logMessage.AppendLine(DateTime.Now.ToString() + " - WriteToExcelSpreadsheet method in Chempoint.ReportUploader ended.");

            }
            return filePath;
        }


    }
}
