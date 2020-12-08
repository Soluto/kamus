using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kamus.KeyManagement;

namespace CustomResourceDescriptorController.utils
{
    public static class KeyManagementExtensions
    {
        public async static Task<Dictionary<string, T>> DecryptItems<T>(
            this IKeyManagement keyManagement,
            Dictionary<string, string> source,
            string serviceAccountId,
            Action<Exception, string> errorHandler,
            Func<string, T> mapper)
        {
            var result = new Dictionary<string, T>();

            if (source == null)
            {
                return result;
            }

            foreach (var (key, value) in source)
            {
                try
                {
                    var decrypted = await keyManagement.Decrypt(value, serviceAccountId);

                    result.Add(key, mapper(decrypted));
                }
                catch (Exception e)
                {
                    errorHandler(e, key);
                }
            }

            return result;
        }

    }
}
