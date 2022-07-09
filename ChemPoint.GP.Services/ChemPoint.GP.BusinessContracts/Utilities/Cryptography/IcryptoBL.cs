using Chempoint.GP.Model.Interactions.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.BusinessContracts.Utilities.Cryptography
{
    public interface IcryptoBL
    {
        CryptoResponse EncryptAndUpdatePassword(CryptoRequest objCryptoRequest);
        CryptoResponse DecryptPassword(CryptoRequest objCryptoRequest);
    }
}
