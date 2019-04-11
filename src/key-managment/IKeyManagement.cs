using System;
using System.Threading.Tasks;

namespace Kamus.KeyManagement
{
    public interface IKeyManagement 
    {
        Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true);
        Task<string> Decrypt(string encryptedData, string serviceAccountId);
    }

    public class DecryptionFailureException : Exception
    {
        public DecryptionFailureException(string reason, Exception innerException) : base(reason, innerException)
        {
            
        }
    }
}