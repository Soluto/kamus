using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.Extensions.Configuration;

namespace Hamuste.KeyManagment
{
    public class SymmetricKeyManagment : IKeyManagment
    {
        private readonly byte[] mKey;
        private readonly int mMaximumDataLength;

        public SymmetricKeyManagment(IConfiguration configuration, string key = null)
        {
            if (key == null)
            {
                var rnd = RandomNumberGenerator.Create();
                var keyBuffer = new byte[50];
                rnd.GetBytes(keyBuffer);
                mKey = keyBuffer;
            }
            mKey = Convert.FromBase64String(key);
            
            if (!int.TryParse(configuration["KeyVault:KeyLength"], out mMaximumDataLength))
            {
                mMaximumDataLength = int.MaxValue;
            }
            
        }
        public Task<string> Decrypt(string encryptedData, string serviceAccountId = null)
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

        public int GetMaximumDataLength()
        {
            return mMaximumDataLength;
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

        public string GetEncryptionKey()
        {
            return Convert.ToBase64String(mKey);
        }
    }
}