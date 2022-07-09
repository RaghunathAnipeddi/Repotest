using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chempoint.GP.Model.Interactions.Cryptography;
using ChemPoint.GP.BusinessContracts.Utilities.Cryptography;
namespace ChemPoint.Cryptography
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    // Name         : Encrypt Password
    // Description  : This class is used to encrypt and update the passowrd of FTP
    //                
    //
    // Created by   : RosiReddy
    // Date         : 07-Sept-2016
    //--------------------------------------------------------------------------------------------------------------------------------------------- 
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("***************************************************************");
            Console.WriteLine("**************FTP Password Reset Utility****************");
            Console.WriteLine("***************************************************************");
            Console.WriteLine("Enter the application name : ");
            string strAppName = Console.ReadLine().Trim();
            Console.WriteLine("Enter the password of FTP  : ");
            string strFtpPasword = Console.ReadLine().Trim();
            if (!String.IsNullOrEmpty(strFtpPasword))
            {
                IcryptoBL cryptoBL = new Chempoint.GP.CryptoBL.CryptoBL();
                CryptoRequest objCryptoRequest = new CryptoRequest();
                objCryptoRequest.objFTPEntity = new ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP.FTPEntity();
                objCryptoRequest.objFTPEntity.AppName = strAppName;
                objCryptoRequest.objFTPEntity.FtpPassword = strFtpPasword;
                objCryptoRequest.objFTPEntity.CreatedBy = Chempoint.GP.Infrastructure.Config.Configuration.FtpAdminId;
                var obj = cryptoBL.EncryptAndUpdatePassword(objCryptoRequest);
                Console.WriteLine("Encrypting and updating the encrypted password in table");

                if (String.Equals(obj.ErrorStatus, Chempoint.GP.Infrastructure.Config.Configuration.CryptoSuccess))
                    Console.WriteLine("Ftp password is updated successfully for " + strAppName);

                Console.WriteLine("***************************************************************");
            }
            Console.ReadLine();
        }
    }
}
