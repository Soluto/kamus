using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Hamuste.KeyManagment;
using Xunit;

namespace unit.KeyManagment
{
    public class SymmetricKeyManagementTests
    {
        [Fact]
        public async Task Get_ReturnsCorrectValues()
        {
            var key = Convert.ToBase64String(GetRandomData(32));
            var kms = new SymmetricKeyManagement(key);
            var expected = "hello";
            var encrypted = await kms.Encrypt(expected, "sa");
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
