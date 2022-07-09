using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChemPoint.GP.BusinessContracts.Utilities.Cryptography;
using Chempoint.GP.Model.Interactions.Cryptography;
using System.Configuration;
using ChemPoint.GP.DataContracts.Utilities;
using Chempoint.GP.CryptoDL;
using System.Security.Cryptography;
using System.IO;
using System.Data.SqlClient;
using Chempoint.GP.Infrastructure.Logging;
namespace Chempoint.GP.CryptoBL
{
    public class CryptoBL : IcryptoBL
    {
        /// <summary>
        /// To Encrypt the password and updated the password based on App name
        /// </summary>
        /// <param name="objCryptoRequest"></param>
        /// <returns></returns>
        public CryptoResponse EncryptAndUpdatePassword(CryptoRequest objCryptoRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            CryptoResponse objCryptoResponse = new CryptoResponse();
            string strCipherText = string.Empty;
            IcryptoDL cryptoDL = new Chempoint.GP.CryptoDL.CryptoDL(ConfigurationManager.ConnectionStrings["FtpConnString"].ToString());
            bool bRestPassword = false;
            try
            {
                logMessage.AppendLine("****************************************************************");
                logMessage.AppendLine(DateTime.Now.ToString() + " - EncryptAndUpdatePassword method in Chempoint.GP.CryptoBL.CryptoBL is started");
                strCipherText = Encrypt(objCryptoRequest.objFTPEntity.FtpPassword);
                if (!string.IsNullOrEmpty(strCipherText))
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Started the files upload to the FTP");
                    bRestPassword = cryptoDL.UpdateFtpPassoword(objCryptoRequest.objFTPEntity.AppName, strCipherText, objCryptoRequest.objFTPEntity.CreatedBy);
                    if (bRestPassword)
                        objCryptoResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.CryptoSuccess;

                    logMessage.AppendLine(DateTime.Now.ToString() + " - Started the files upload to the FTP");
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objCryptoResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.CryptoFailure;
                objCryptoResponse.ErrorMessage = ex.Message;
            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - EncryptAndUpdatePassword method in Chempoint.GP.CryptoBL.CryptoBL ended.");
                logMessage.AppendLine("****************************************************************");

            }

            return objCryptoResponse;
        }

        /// <summary>
        /// To decrypt the Ftp password
        /// </summary>
        /// <param name="objCryptoRequest"></param>
        /// <returns></returns>
        public CryptoResponse DecryptPassword(CryptoRequest objCryptoRequest)
        {

            CryptoResponse objCryptoResponse = new CryptoResponse();
            string strPlainText = string.Empty;
            try
            {
                objCryptoRequest.objFTPEntity = new ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP.FTPEntity();
                objCryptoRequest.objFTPEntity.LoggingFilePath = ConfigurationManager.AppSettings["FtpLogPath"].ToString();
                objCryptoRequest.objFTPEntity.LoggingFileName = ConfigurationManager.AppSettings["FtpLogFileName"].ToString();
                objCryptoRequest.logMessage.AppendLine(DateTime.Now.ToString() + " - DecryptPassword method in Chempoint.GP.CryptoBL.CryptoBL is started");
                objCryptoRequest.logMessage.AppendLine(DateTime.Now.ToString() + " - Started the decrypting the password");
                strPlainText = Decrypt(objCryptoRequest.CipherText);
                if (!string.IsNullOrEmpty(strPlainText))
                {
                    objCryptoResponse.DecryptedText = strPlainText;
                    objCryptoResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.CryptoSuccess;
                }

            }
            catch (Exception ex)
            {
                objCryptoRequest.logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                objCryptoRequest.logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objCryptoResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.CryptoFailure;
                objCryptoResponse.ErrorMessage = ex.Message;
                objCryptoResponse.logMessage = objCryptoResponse.logMessage;
            }
            finally
            {

                objCryptoRequest.logMessage.AppendLine(DateTime.Now.ToString() + " - DecryptPassword method in Chempoint.GP.CryptoBL.CryptoBL ended.");
                objCryptoResponse.logMessage = objCryptoRequest.logMessage;
            }
            return objCryptoResponse;
        }

        /// <summary>
        /// To encrypt string
        /// </summary>
        /// <param name="clearText">Input string</param>
        /// <returns>Encrypted string</returns>
        private static string Encrypt(string clearText)
        {
            try
            {
                //Encryption using AES
                string EncryptionKey = "Chempoint@123";
                byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        clearText = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception exceptionObj)
            {
                throw exceptionObj;
            }
            return clearText;
        }

        /// <summary>
        /// Decrypt the encrypted password
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        private string Decrypt(string cipherText)
        {
            try
            {
                string EncryptionKey = "Chempoint@123";
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            catch
            {
                throw;
            }
            return cipherText;
        }

    }
}