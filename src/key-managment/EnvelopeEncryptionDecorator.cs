using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;

namespace Kamus.KeyManagement
{
    public class EnvelopeEncryptionDecorator : IKeyManagement
    {
        private readonly IKeyManagement mMasterKeyManagement;
        private readonly int mMaximumDataLength;
        private readonly ILogger mLogger = Log.ForContext<EnvelopeEncryptionDecorator>();

        public EnvelopeEncryptionDecorator(IKeyManagement masterKeyManagement, int maximumDataLength)
        {
            mMasterKeyManagement = masterKeyManagement;
            mMaximumDataLength = maximumDataLength;
        }

        public async Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            if (data.Length <= mMaximumDataLength)
            {
                return await mMasterKeyManagement.Encrypt(data, serviceAccountId, createKeyIfMissing);
            }

            mLogger.Information("Encryption data too length, using envelope encryption");

            var dataKey = RijndaelUtils.GenerateKey(32);
            var (encryptedData, iv) = RijndaelUtils.Encrypt(dataKey, Encoding.UTF8.GetBytes(data));
            var encryptedDataKey = await mMasterKeyManagement.Encrypt(Convert.ToBase64String(dataKey), serviceAccountId, createKeyIfMissing);
            return $"env${encryptedDataKey}${Convert.ToBase64String(encryptedData)}${Convert.ToBase64String(iv)}";

        }

        public async Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            var regex = new Regex(@"(env)\$(.*)\$(.*)\$(.*)");
            var match = regex.Match(encryptedData);
            
            if (!match.Success)
                return await mMasterKeyManagement.Decrypt(encryptedData, serviceAccountId);

            mLogger.Information("Encrypted data mactched envelope encryption pattern");
            var encryptedDataKey = match.Groups[2].Value;
            var actualEncryptedData = Convert.FromBase64String(match.Groups[3].Value);
            var iv = Convert.FromBase64String(match.Groups[3].Value);

            var key = await mMasterKeyManagement.Decrypt(encryptedDataKey, serviceAccountId);

            var decrypted = RijndaelUtils.Decrypt(Encoding.UTF8.GetBytes(key), iv, actualEncryptedData);
            return Encoding.UTF8.GetString(decrypted);

        }
    }
}
