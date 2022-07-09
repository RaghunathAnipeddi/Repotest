using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Microsoft.Dexterity.Applications;
using Microsoft.Dexterity.Shell;
using Microsoft.Dexterity.Applications.DynamicsDictionary;
using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities;
using Chempoint.GP.Model.Interactions.Sales;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using Chempoint.GP.Infrastructure.Logging;
using System.Text;
using Chempoint.GP.SO.Properties;

namespace Chempoint.GP.SO
{
    /// <summary>
    /// This form displays the Sales line detail's lot number information. The form is a read-only.
    /// </summary>
    public partial class GP_Lot_Inquiry : DexUIForm
    {
        // Sop Inquiry Item Detail Form object
        SopInquiryItemDetailForm sopInquiryItemDetailForm = Dynamics.Forms.SopInquiryItemDetail;

        static string gpServiceConfigurationUrl = null;
        bool isSoLoggingEnabled = false;
        string soLogFileName = null;
        string soLogFilePath = null;

        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileStringW",
            SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnString, int nSize, string lpFilename);

        /// <summary>
        /// Constructor
        /// </summary>
        public GP_Lot_Inquiry()
        {
            InitializeComponent();
            string getCurrentDirectoryPath = @Directory.GetCurrentDirectory();
            string iniFilePath = getCurrentDirectoryPath + "\\Data\\Dex.ini";
            List<string> categories = GetCategories(iniFilePath);
            foreach (string category in categories)
            {
                //Get the key values
                gpServiceConfigurationUrl = GetIniFileString(iniFilePath, category, "GPSERVICE", "");
                isSoLoggingEnabled = Convert.ToBoolean(GetIniFileString(iniFilePath, category, "ISSOLOGENABLED", ""));
                soLogFileName = GetIniFileString(iniFilePath, category, "SOLOGFILENAME", "");
                soLogFilePath = GetIniFileString(iniFilePath, category, "SOLOGFILEPATH", "");
                //isSoLoggingEnabled = false;
                //soLogFileName = "";
                //soLogFilePath = "";
            }
        }

        /// <summary>
        /// Method to read the data from dex.ini file
        /// </summary>
        /// <param name="iniFile"></param>
        /// <param name="category"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static string GetIniFileString(string iniFile, string category, string key, string defaultValue)
        {
            string returnString = new string(' ', 1024);
            GetPrivateProfileString(category, key, defaultValue, returnString, 1024, iniFile);
            return returnString.Split('\0')[0];
        }

        /// <summary>
        /// Method to get the catagory for the ini file.
        /// </summary>
        /// <param name="iniFile"></param>
        /// <returns></returns>
        private static List<string> GetCategories(string iniFile)
        {
            string returnString = new string(' ', 65536);
            GetPrivateProfileString(null, null, null, returnString, 65536, iniFile);
            List<string> result = new List<string>(returnString.Split('\0'));
            result.RemoveRange(result.Count - 2, 2);
            return result;
        }

        /// <summary>
        /// form load event. This will loads the grid with the respective data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GP_Lot_Inquiry_Load(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                SalesLineItem salesLineItem = new SalesLineItem();
                SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
                SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
                SalesOrderEntity salesOrderEntity = new SalesOrderEntity();
                SalesOrderRequest salesOrderRequest = new SalesOrderRequest();

                TextBoxSopNumber.Text = sopInquiryItemDetailForm.SopInquiryItemDetail.SopNumber.Value.ToString();
                TextBoxSopType.Text = sopInquiryItemDetailForm.SopInquiryItemDetail.SopType.Value.ToString();
                TextBoxLineSeq.Text = sopInquiryItemDetailForm.SopInquiryItemDetail.LineItemSequence.Value.ToString();
                TextBoxItemNmbr.Text = sopInquiryItemDetailForm.SopInquiryItemDetail.ItemNumber.Value.ToString();

                //// maps the soptype with DB values against form values
                //int sopTypeDataBase = 0;
                //if (TextBoxSopType.Text == "3")//fo
                //    sopTypeDataBase = 6;
                //else if (TextBoxSopType.Text == "4")//inv
                //    sopTypeDataBase = 3;
                //else if (TextBoxSopType.Text == "6")//bo
                //    sopTypeDataBase = 5;
                //else if (TextBoxSopType.Text == "5")//ret
                //    sopTypeDataBase = 4;
                //else
                //    sopTypeDataBase = sopInquiryItemDetailForm.SopInquiryItemDetail.SopType.Value;

                salesOrderEntity.SopNumber = sopInquiryItemDetailForm.SopInquiryItemDetail.SopNumber.Value.ToString();
                salesOrderEntity.SopType = sopInquiryItemDetailForm.SopInquiryItemDetail.SopType.Value;
                salesLineItem.ItemNumber = sopInquiryItemDetailForm.SopInquiryItemDetail.ItemNumber.Value.ToString();
                salesLineItem.OrderLineId = sopInquiryItemDetailForm.SopInquiryItemDetail.LineItemSequence.Value;
                salesOrderDetails.LineItemDetails = salesLineItem;
                salesOrderInformation.SalesOrderDetails = salesOrderDetails;
                salesOrderEntity.SalesOrderDetails = salesOrderInformation;
                salesOrderRequest.SalesOrderEntity = salesOrderEntity;

                if (!string.IsNullOrEmpty(salesOrderRequest.SalesOrderEntity.SopNumber))
                {
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/GetLotDetail", salesOrderRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            SalesOrderResponse salesOrderResponse = response.Result.Content.ReadAsAsync<SalesOrderResponse>().Result;
                            if (salesOrderResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In GP_Lot_Inquiry_Load Method (GetLotDetail-Inquiry): " + salesOrderResponse.ErrorMessage.ToString());
                                MessageBox.Show("Data does not fetch due to error: " + salesOrderResponse.ErrorMessage.ToString(), Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                TextBoxSopNumber.Text = sopInquiryItemDetailForm.SopInquiryItemDetail.SopNumber.Value.ToString();
                                TextBoxSopType.Text = sopInquiryItemDetailForm.SopInquiryItemDetail.SopType.Value.ToString();
                                TextBoxLineSeq.Text = sopInquiryItemDetailForm.SopInquiryItemDetail.LineItemSequence.Value.ToString();
                                TextBoxItemNmbr.Text = sopInquiryItemDetailForm.SopInquiryItemDetail.ItemNumber.Value.ToString();

                                List<LotInformation> lotInformationList;

                                lotInformationList = salesOrderResponse.SalesOrderDetails.SalesOrderDetails.SalesOrderDetails.LineItemDetails.LotsList;
                                DataTable convertDT = new DataTable();
                                convertDT = ListToDataTable(lotInformationList);
                                LotsDetailGrid.DataSource = convertDT.DefaultView;
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not fetch for GP Lot Form Inquiry");
                            MessageBox.Show("Data does not fetch", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }

                // formatting the grid 
                LotsDetailGrid.Columns[0].Width = 166;
                LotsDetailGrid.Columns[1].Width = 84;
                LotsDetailGrid.Columns[2].Width = 88;
            }
            catch (FormatException ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In GP_Lot_Info_Load Method (GP_Lot_Inquiry_Load): " + ex.Message.ToString());
                MessageBox.Show("Data does not fetch due to error: " + ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In GP_Lot_Info_Load Method (GP_Lot_Inquiry_Load): " + ex.Message.ToString());
                MessageBox.Show("Data does not fetch due to error: " + ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "SalesDetails");
                logMessage = null;
            }
        }

        private static DataTable ListToDataTable(List<LotInformation> items)
        {
            DataTable dataTable = new DataTable(typeof(LotInformation).Name);

            //Get all the properties
            PropertyInfo[] props = typeof(LotInformation).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (LotInformation item in items)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        /// <summary>
        /// close button event. This will close the current form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        /// <summary>
        ///Method to move the create  the log file and for todays date and write to log file.
        /// </summary>
        /// <param name="message">log message</param>
        /// <param name="logFileName">Name of the log file</param>
        /// <param name="logFilePath">Path for the log file</param>
        private void LogPostProcessDetailsToFile(string message, string fileType)
        {
            switch (fileType)
            {
                case "SalesDetails":
                    if (isSoLoggingEnabled && message != "")
                        new TextLogger().LogInformationIntoFile(message, soLogFilePath, soLogFileName);
                    break;
                default:
                    break;
            }
        }
    }
}