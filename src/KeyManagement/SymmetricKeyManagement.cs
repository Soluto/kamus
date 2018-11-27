using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Hamuste.KeyManagement
{
    public class SymmetricKeyManagement : IDynamicKeyManagmenet
    {
        private byte[] mKey;
        
        public SymmetricKeyManagement(string key = null)
        {
            if (key == null)
            {
                using (var aes = new AesManaged())
                {
                    aes.GenerateKey();
                    mKey = aes.Key;
                }
            }
            else
            {
                mKey = Convert.FromBase64String(key);
            }
        }
        
        public Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            var splitted = encryptedData.Split(":");
            if (splitted.Length != 2) {
                throw new InvalidOperationException("Encrypted data format is invalid");
            }

            var iv = Convert.FromBase64String(splitted[0]);
            var buffer = Convert.FromBase64String(splitted[1]);

            byte[] result;
            using (var rijndael = new RijndaelManaged())
            {
                using (var decryptor = rijndael.CreateDecryptor(mKey, iv))
                using (var resultStream = new MemoryStream())
                {
                    using (var rijndaelStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(buffer))
                    {
                        plainStream.CopyTo(rijndaelStream);
                    }

                    result = resultStream.ToArray();
                }
            }

            return Task.FromResult(Encoding.UTF8.GetString(result));
        }

        public Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            var iv = GetRandomData(16);
            var buffer = Encoding.UTF8.GetBytes(data);

            byte[] result;
            using (var rijndael = new RijndaelManaged())
            {
                using (var encryptor = rijndael.CreateEncryptor(mKey, iv))
                using (var resultStream = new MemoryStream())
                {
                    using (var rijndaelStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(buffer))
                    {
                        plainStream.CopyTo(rijndaelStream);
                    }

                    result = resultStream.ToArray();
                }
            }

            return Task.FromResult(Convert.ToBase64String(iv) + ":" + Convert.ToBase64String(result));
        }

        private static byte[] GetRandomData(int size)
        {
            var provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[size];
            provider.GetBytes(byteArray);
            return byteArray;
        }

        public void SetEncryptionKey(string key)
        {
            mKey = Convert.FromBase64String(key);
        }

        public string GetEncryptionKey()
        {
            return Convert.ToBase64String(mKey);
        }
    }
}