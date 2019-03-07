using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Kamus.KeyManagement
{
    public static class KeyIdCreator
    {
        public static string Create(string s)
        {
            return
                WebEncoders.Base64UrlEncode(
                        SHA256.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(s)))
                    .Replace("_", "-");
        }
    }
}