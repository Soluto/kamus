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
            var kms = new SymmetricKeyManagement(Key, false);
            var expected = "hello";
            var encrypted = await kms.Encrypt(expected, "sa");
            var decrypted = await kms.Decrypt(encrypted, "sa");

            Assert.Equal(expected, decrypted);
        }

        [Fact]
        public async Task Get_ReturnsCorrectValuesWhenDeriviationEnabled()
        {
            var kms = new SymmetricKeyManagement(Key, true);
            var expected = "hello";
            var encrypted = await kms.Encrypt(expected, "sa");
            var decrypted = await kms.Decrypt(encrypted, "sa");

            Assert.Equal(expected, decrypted);
        }

        [Fact]
        public async Task Get_ReturnsCorrectValuesForDifferentServiceAccounts()
        {
            var kms = new SymmetricKeyManagement(Key, false);
            var expected = "hello";
            var encryptedSa1 = await kms.Encrypt(expected, "sa1");
            var encryptedSa2 = await kms.Encrypt(expected, "sa2");
            var decryptedSa1 = await kms.Decrypt(encryptedSa2, "sa1");
            var decryptedSa2 = await kms.Decrypt(encryptedSa1, "sa2");

            Assert.Equal(expected, decryptedSa1);
            Assert.Equal(expected, decryptedSa2);
        }

        [Fact]
        public async Task Get_ReturnsInvalidValuesForDifferentServiceAccountsWhenDeriviationEnabled()
        {
            var kms = new SymmetricKeyManagement(Key, true);
            var expected = "hello";
            var encryptedSa1 = await kms.Encrypt(expected, "sa1");
            var encryptedSa2 = await kms.Encrypt(expected, "sa2");
            await Assert.ThrowsAsync<CryptographicException>(async () => await kms.Decrypt(encryptedSa2, "sa1"));
            await Assert.ThrowsAsync<CryptographicException>(async () => await kms.Decrypt(encryptedSa1, "sa2"));
        }

        [Fact]
        public async Task RegressionTest()
        {
            var kms = new SymmetricKeyManagement(Key, false);
            var expected = "hello";
            var encrypted = "C4gChhspnTa5yVqYmSitrg==:tr0Ke6OGUaUa8KZgMJg14g==";
            var decrypted = await kms.Decrypt(encrypted, "sa");

            Assert.Equal(expected, decrypted);
        }

        [Fact]
        public async Task RegressionTestWithDeriviation()
        {
            var kms = new SymmetricKeyManagement(Key, true);
            var expected = "hello";
            var encrypted = "VnZkifeNdxI7NWMbjr/MZg==:qskLf4Z57DC9HBTe6+IEkA==";
            var decrypted = await kms.Decrypt(encrypted, "sa");

            Assert.Equal(expected, decrypted);
        }

    }
}
