using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kamus.KeyManagement
{
    public class SymmetricKeyManagement : IKeyManagement
    {
        private readonly byte[] mKey;
        
        public SymmetricKeyManagement(byte[] key)
        {
            mKey = key;
        }
        
        public Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            var splitted = encryptedData.Split(':');
            if (splitted.Length != 2) {
                throw new InvalidOperationException("Encrypted data format is invalid");
            }

            var iv = Convert.FromBase64String(splitted[0]);
            var data = Convert.FromBase64String(splitted[1]);

            var result = RijndaelUtils.Decrypt(mKey, iv, data);

            return Task.FromResult(Encoding.UTF8.GetString(result));
        }

        public Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            var (decryptedData, iv) = RijndaelUtils.Encrypt(mKey, Encoding.UTF8.GetBytes(data));

            return Task.FromResult(Convert.ToBase64String(iv) + ":" + Convert.ToBase64String(decryptedData));
        }
    }
}