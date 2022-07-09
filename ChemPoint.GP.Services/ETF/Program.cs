using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.ETF
{

    class Program
    {

        static DataTable ToDataTable<T>(List<T> items)
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

        static DataTable GetDataTableFromCsv(string path, bool isFirstRowHeader)
        {
            DataTable dataTable = new DataTable();
            string header = isFirstRowHeader ? "Yes" : "No";

            string pathOnly = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            string sql = @"SELECT * FROM [" + fileName + "]";

            using (OleDbConnection connection = new OleDbConnection(
                      @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly +
                      ";Extended Properties=\"Text;HDR=" + header + "\""))
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
            {

                dataTable.Locale = CultureInfo.CurrentCulture;
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        static void Main(string[] args)
        {
            int i = 1;

            DataTable dtOriginal = null;
            DataTable dtForMail = null;
            List<EFT> objEFListForMail = null;
            List<FilTeredEFT> objFilteredList = null;
            List<EFT> objEFTList = null;


            DataTable dt = GetDataTableFromCsv(@"C:\Users\arosired\Desktop\CTX Extracted by Tab delimited.csv", true);

            if (dt != null)
            {
                objEFTList = new List<EFT>();

                foreach (DataRow dr in dt.Rows)
                {
                    EFT objEFT = new EFT();

                    if (dr["As Of"] != DBNull.Value)
                        objEFT.AsOf = Convert.ToDateTime(dr["As Of"]);

                    objEFT.Currency = dr["Currency"].ToString();
                    objEFT.BankIDType = dr["BankID Type"].ToString();
                    objEFT.BankID = dr["BankID"].ToString();
                    objEFT.Account = dr["Account"].ToString();
                    objEFT.DataType = dr["Data Type"].ToString();

                    if (dr["BAI Code"] != DBNull.Value)
                        objEFT.BAICode = Convert.ToInt32(dr["BAI Code"]);

                    objEFT.Description = dr["Description"].ToString();

                    if (dr["Amount"] != DBNull.Value)
                        objEFT.Amount = Convert.ToInt32(dr["Amount"]);

                    objEFT.BalanceOrValueDate = dr["Balance/Value Date"].ToString();
                    objEFT.CustomerReference = dr["Customer Reference"].ToString();

                    if (dr["Immediate Availability"] != DBNull.Value)
                        objEFT.ImmediateAvailability = Convert.ToInt32(dr["Immediate Availability"]);

                    if (dr["1 Day Float"] != DBNull.Value)
                        objEFT.OneDayFloat = Convert.ToInt32(dr["1 Day Float"]);

                    if (dr["1 Day Float"] != DBNull.Value)
                        objEFT.OneDayFloat = Convert.ToInt32(dr["1 Day Float"]);

                    if (dr["2+ DayFloat"] != DBNull.Value)
                        objEFT.TwoPlusDayFloat = Convert.ToInt32(dr["2+ DayFloat"]);

                    objEFT.BankReference = dr["Bank Reference"].ToString();

                    if (dr["# of Items"] != DBNull.Value)
                        objEFT.NoOfItems = Convert.ToInt32(dr["# of Items"]);

                    objEFT.Text = dr["Text"].ToString();
                    objEFT.EftFileReferenceId = i;

                    i++;
                    objEFTList.Add(objEFT);
                }



                if (objEFTList.Count > 0)
                {

                    dtForMail = new DataTable();
                    objFilteredList = new List<FilTeredEFT>();
                    objEFListForMail = new List<EFT>();

                    dtForMail.Columns.Add("Date A Of");
                    dtForMail.Columns.Add("Account");
                    dtForMail.Columns.Add("Amount");
                    dtForMail.Columns.Add("Text");

                    dtOriginal = ToDataTable(objEFTList);

                    foreach (var obj in objEFTList)
                    {

                        if (!obj.Description.Contains("Lockbox Deposit Credit") && !obj.Description.Contains("Lockbox Receipt") &&
                            !obj.Text.Contains("AMERICAN EXPRESS") && !obj.Text.Contains("BOFA MERCH") && !obj.Text.Contains("CANADA LOCKBOX") &&
                            !obj.DataType.Contains("Summary") && !obj.DataType.Contains("BOFA MERCH"))
                        {

                            if (obj.Text.StartsWith("WIRE TYPE"))
                            {
                                DataRow dr = dtForMail.NewRow();

                                dr["Date A Of"] = obj.AsOf;
                                dr["Account"] = obj.Account;
                                dr["Amount"] = obj.Amount;
                                dr["Text"] = obj.Text;

                                dtForMail.Rows.Add(dr);
                            }

                            FilTeredEFT objFilTeredEFT = new FilTeredEFT();
                            if (obj.Text.Contains("DES"))
                            {

                                string strRef = obj.Text.Split(':')[0];
                                strRef = strRef.Remove(strRef.Length - 4, 4);
                                objFilTeredEFT.ReferenceNumber = strRef;
                            }

                            if (obj.AsOf != null)
                                objFilTeredEFT.DateRecived = obj.AsOf;

                            if (obj.Amount != null)
                            {
                                objFilTeredEFT.PaymentAmount = obj.Amount;
                                objFilTeredEFT.ItemAmount = obj.Amount;
                            }

                            if (obj.CustomerReference != null)
                                objFilTeredEFT.CustomerID = obj.CustomerReference;

                            objFilTeredEFT.ReceivedOnCTS = "Yes";
                            objFilTeredEFT.Source = "CTX";
                            objFilTeredEFT.Status = 0;

                            if (obj.BankID != null)
                                objFilTeredEFT.BankOriginatingID = obj.BankID;

                            objFilTeredEFT.EftFileReferenceId = obj.EftFileReferenceId;

                            if (obj.Account != null)
                                objFilTeredEFT.AccountNo = obj.Account;

                            objFilteredList.Add(objFilTeredEFT);
                        }
                    }

                    DataTable dtFiltered = new DataTable();
                    if (objFilteredList.Count > 0)
                        dtFiltered = ToDataTable(objFilteredList);
                }
            }
        }
    }
}
