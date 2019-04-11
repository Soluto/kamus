using System;
using System.Text.RegularExpressions;

namespace Kamus.KeyManagement
{
    public static class EnvelopeEncryptionUtils
    {
        private static readonly Regex WrappedDataRegex = new Regex("env\\$(?<encryptedDataKey>.*)\\$(?<iv>.*):(?<encryptedData>.*)");

        public static string Wrap(string encryptedDataKey, byte[] iv, byte[] encryptedData)
        {
            return $"env${encryptedDataKey}${Convert.ToBase64String(iv)}:{Convert.ToBase64String(encryptedData)}";
        }

        public static (string encryptedDataKey, byte[] iv, byte[] encryptedData) Unwrap(string wrappedData)
        {
            var match = WrappedDataRegex.Match(wrappedData);

            if (!match.Success)
            {
                throw new InvalidOperationException("Invalid encrypted data format");
            }

            var encryptedDataKey = match.Groups["encryptedDataKey"].Value;
            var actualEncryptedData = Convert.FromBase64String(match.Groups["encryptedData"].Value);
            var iv = Convert.FromBase64String(match.Groups["iv"].Value);

            return (encryptedDataKey, iv, actualEncryptedData);
        }
    }
}
