using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChemPoint.GP.DataContracts.Utilities;
using System.Data.SqlClient;
using System.Data;

namespace Chempoint.GP.CryptoDL
{
    public class CryptoDL : IcryptoDL
    {

        string strConnectionString = string.Empty;

        public CryptoDL(string strConnString)
        {
            strConnectionString = strConnString;
        }

        /// <summary>
        /// To update the password of Ftp based App name
        /// </summary>
        /// <param name="strAppName"></param>
        /// <param name="strEncryptedPwd"></param>
        /// <param name="lngModifiedBy"></param>
        /// <returns></returns>
        public bool UpdateFtpPassoword(string strAppName, string strEncryptedPwd, long lngModifiedBy)
        {
            int intRowsAffected = 0;
            bool bRowsAffected = false;
            try
            {

                using (SqlConnection con = new SqlConnection(strConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("GPTrace.chmpt.UpdateFtpPassword", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@AppName", strAppName);
                        cmd.Parameters.AddWithValue("@FtpPassword", strEncryptedPwd);
                        cmd.Parameters.AddWithValue("@ModifiedBy", lngModifiedBy);

                        intRowsAffected = cmd.ExecuteNonQuery();

                        if (intRowsAffected > 0)
                            bRowsAffected = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return bRowsAffected;
        }
    }
}


