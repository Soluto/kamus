using System;
using System.Threading.Tasks;

namespace Hamuste.KeyManagement
{
    public interface IKeyManagement 
    {
        Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true);
        Task<string> Decrypt(string encryptedData, string serviceAccountId);
    }

    public interface IDynamicKeyManagmenet : IKeyManagement
    {
        void SetEncryptionKey(string key = null);
        string GetEncryptionKey();
    }

    public class DecryptionFailureException : Exception
    {
        public DecryptionFailureException(string reason, Exception innerException) : base(reason, innerException)
        {
            
        }
    }
}