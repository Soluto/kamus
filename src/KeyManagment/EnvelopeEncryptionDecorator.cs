using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;

namespace Hamuste.KeyManagment
{
    public class EnvelopeEncryptionDecorator : IKeyManagement
    {
        private readonly IKeyManagement mMasterKeyManagement;
        private readonly IDynamicKeyManagmenet mDataKeyManagement;
        private readonly int mMaximumDataLength;
        private readonly ILogger mLogger = Log.ForContext<EnvelopeEncryptionDecorator>();

        public EnvelopeEncryptionDecorator(IKeyManagement masterKeyManagement, IDynamicKeyManagmenet dataKeyManagement, int maximumDataLength)
        {
            mMasterKeyManagement = masterKeyManagement;
            mDataKeyManagement = dataKeyManagement;
            mMaximumDataLength = maximumDataLength;
        }


        public async Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            if (data.Length <= mMaximumDataLength)
            {
                return await mMasterKeyManagement.Encrypt(data, serviceAccountId, createKeyIfMissing);
            }

            mLogger.Information($"Envelope encryption is starting, data to encrypt was {data.Length} characters");
            var encryptedData = await mDataKeyManagement.Encrypt(data, serviceAccountId, createKeyIfMissing);
            var encryptedKey = await mMasterKeyManagement.Encrypt(mDataKeyManagement.GetEncryptionKey(), serviceAccountId, createKeyIfMissing);
            return "env" + "$" + encryptedKey + "$" + encryptedData;

        }

        public async Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            var regex = new Regex(@"(env)\$(.*)\$(.*)");
            var match = regex.Match(encryptedData);
            
            if (!match.Success)
                return await mMasterKeyManagement.Decrypt(encryptedData, serviceAccountId);

            mLogger.Information("Found envelope encrypted value, decrypting...");
            var encryptedKey = match.Groups[2].Value;
            var extractedEncryptedData = match.Groups[3].Value;

            var key = await mMasterKeyManagement.Decrypt(encryptedKey, serviceAccountId);
            
            mDataKeyManagement.SetEncryptionKey(key);
            return await mDataKeyManagement.Decrypt(extractedEncryptedData, serviceAccountId);

        }
    }
}