using System;
using System.Threading.Tasks;

namespace Hamuste.KeyManagment
{
    public interface IKeyManagment 
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