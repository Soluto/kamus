using System;
using System.IO;
using System.Security.Cryptography;

namespace Kamus.KeyManagement
{
    public static class RijndaelUtils
    {
        public static byte[] GenerateKey(int keySize = 256)
        {
            using (var aes = new AesManaged())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                return aes.Key;
            }
        }

        public static (byte[] encryptedData, byte[] iv) Encrypt(byte[] key, byte[] data)
        {
            byte[] iv = GetRandomData(16);
            byte[] result;
            using (var rijndael = new RijndaelManaged())
            {
                using (var encryptor = rijndael.CreateEncryptor(key, iv))
                using (var resultStream = new MemoryStream())
                {
                    using (var rijndaelStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(data))
                    {
                        plainStream.CopyTo(rijndaelStream);
                    }

                    result = resultStream.ToArray();
                }
            }

            return (result, iv);
        }

        public static byte[] Decrypt(byte[] key, byte[] iv, byte[] encryptedData)
        {
            byte[] result;
            try
            {
                using (var rijndael = new RijndaelManaged())
                {
                    using (var decryptor = rijndael.CreateDecryptor(key, iv))
                    using (var resultStream = new MemoryStream())
                    {
                        using (var rijndaelStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write))
                        using (var cipherStream = new MemoryStream(encryptedData))
                        {
                            cipherStream.CopyTo(rijndaelStream);
                        }

                        result = resultStream.ToArray();
                    }
                }
            }catch(ArgumentException e)
            {
                throw new Exception($"on no {key.Length}", e);
            }

            return result;

        }

        private static byte[] GetRandomData(int size)
        {
            var provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[size];
            provider.GetBytes(byteArray);
            return byteArray;
        }
    }
}
