using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Kamus.KeyManagement;
using Xunit;

namespace unit.KeyManagement
{
    public class SymmetricKeyManagementTests
    {
        private static readonly byte[] Key = Convert.FromBase64String("tWG4dk8ARsETnFL3jCf1xtMVe05imlx9vimER7iky2s =");

        [Fact]
        public async Task Get_ReturnsCorrectValues()
        {
            var kms = new SymmetricKeyManagement(Key);
            var expected = "hello";
            var encrypted = await kms.Encrypt(expected, "sa");
            var decrypted = await kms.Decrypt(encrypted, "sa");

            Assert.Equal(expected, decrypted);
        }

        [Fact]
        public async Task RegressionTest()
        {
            var kms = new SymmetricKeyManagement(Key);
            var expected = "hello";
            var encrypted = "C4gChhspnTa5yVqYmSitrg==:tr0Ke6OGUaUa8KZgMJg14g==";
            var decrypted = await kms.Decrypt(encrypted, "sa");

            Assert.Equal(expected, decrypted);
        }

        private static byte[] GetRandomData(int size)
        {
            var provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[size];
            provider.GetBytes(byteArray);
            return byteArray;
        }

    }
}
