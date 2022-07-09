using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.DataContracts.Utilities
{
    public interface IcryptoDL
    {
        bool UpdateFtpPassoword(string strAppName, string strEncryptedPwd, long lngModifiedBy);
    }
}
