using Optional;
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

        public static Option<Tuple<string, byte[], byte[]>> Unwrap(string wrappedData)
        {
            var match = WrappedDataRegex.Match(wrappedData);

            if (!match.Success)
            {
                Option.None<Tuple<string, byte[], byte[]>>();
            }

            var encryptedDataKey = match.Groups["encryptedDataKey"].Value;
            var actualEncryptedData = Convert.FromBase64String(match.Groups["encryptedData"].Value);
            var iv = Convert.FromBase64String(match.Groups["iv"].Value);

            return Option.Some(Tuple.Create(encryptedDataKey, iv, actualEncryptedData));
        }
    }
}
