using System;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace Kamus.KeyManagement
{
    public class SymmetricKeyManagement : IKeyManagement
    {
        private readonly byte[] mKey;
        private readonly bool mUseKeyDerivation;
        private const int derivedKeySize = 32;
        
        public SymmetricKeyManagement(byte[] key, bool useKeyDerivation)
        {
            mKey = key;
            mUseKeyDerivation = useKeyDerivation;
        }
        
        public Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            var splitted = encryptedData.Split(':');
            if (splitted.Length != 2) {
                throw new InvalidOperationException("Encrypted data format is invalid");
            }

            var iv = Convert.FromBase64String(splitted[0]);
            var data = Convert.FromBase64String(splitted[1]);

            var key = mUseKeyDerivation ? DeriveKey(serviceAccountId) : mKey;

            var result = RijndaelUtils.Decrypt(key, iv, data);

            return Task.FromResult(Encoding.UTF8.GetString(result));
        }

        public Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            var key = mUseKeyDerivation ? DeriveKey(serviceAccountId) : mKey;

            var (decryptedData, iv) = RijndaelUtils.Encrypt(key, Encoding.UTF8.GetBytes(data));

            return Task.FromResult(Convert.ToBase64String(iv) + ":" + Convert.ToBase64String(decryptedData));
        }

        private byte[] DeriveKey(string serviceAccountId)
        {
            var generator = new HkdfBytesGenerator(new Sha256Digest());

            generator.Init(HkdfParameters.SkipExtractParameters(mKey, Encoding.UTF8.GetBytes(serviceAccountId)));

            var derivedKey = new byte[derivedKeySize];

            generator.GenerateBytes(derivedKey, 0, derivedKeySize);

            return derivedKey;
        }
    }
}