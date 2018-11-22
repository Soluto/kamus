using System.Threading.Tasks;

namespace Hamuste.KeyManagment
{
    public class EnvelopeDecorator : IKeyManagement
    {
        private readonly IKeyManagement mMasterKeyManagement;
        private readonly IDynamicKeyManagmenet mEnvelopeKeyManagement;
        private readonly int mMaximumDataLength;

        public EnvelopeDecorator(IKeyManagement masterKeyManagement, IDynamicKeyManagmenet envelopeKeyManagement, int maximumDataLength)
        {
            mMasterKeyManagement = masterKeyManagement;
            mEnvelopeKeyManagement = envelopeKeyManagement;
            mMaximumDataLength = maximumDataLength;
        }


        public async Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            if (data.Length <= mMaximumDataLength)
            {
                return await mMasterKeyManagement.Encrypt(data, serviceAccountId, createKeyIfMissing);
            }

            var encryptedData = await mEnvelopeKeyManagement.Encrypt(data, serviceAccountId, createKeyIfMissing);
            var encryptedKey = await mMasterKeyManagement.Encrypt(mEnvelopeKeyManagement.GetEncryptionKey(), serviceAccountId, createKeyIfMissing);
            return "env" + "$" + encryptedKey + "$" + encryptedData;

        }

        public async Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            if (!encryptedData.StartsWith("env:"))
                return await mMasterKeyManagement.Decrypt(encryptedData, serviceAccountId);
            
            var encryptedKey = encryptedData.Split("$")[1];
            var extractedEncryptedData = encryptedData.Split("$")[2];

            var key = await mMasterKeyManagement.Decrypt(encryptedKey, serviceAccountId);
            return await mEnvelopeKeyManagement.Decrypt(extractedEncryptedData, serviceAccountId);

        }
    }
}