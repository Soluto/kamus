using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;

namespace Hamuste.KeyManagment
{
    // inspired by: https://stackoverflow.com/a/43936866/4792970
    public class AESKeyManagment : IKeyManagment
    {
        private string mKey;
        private IDataProtector mDataProtector;

        public AESKeyManagment(string key, IDataProtectionProvider dataProtectionProvider)
        {
            mDataProtector = dataProtectionProvider.CreateProtector(key);
        }
        public Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            var protector = mDataProtector.CreateProtector(serviceAccountId);
            return Task.FromResult(
                Encoding.UTF8.GetString(protector.Unprotect(Convert.FromBase64String(encryptedData))));
        }

        public Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            var protector = mDataProtector.CreateProtector(serviceAccountId);
            return Task.FromResult(
                Convert.ToBase64String(protector.Protect(Encoding.UTF8.GetBytes(data))));
        }
    }
}