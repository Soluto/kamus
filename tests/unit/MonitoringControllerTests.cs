using Hamuste.Controllers;
using Xunit;

namespace unit
{
    public class UnitTest1
    {
        [Fact]
        public void Get_ReturnsCorrectValues()
        {
            var res = new MonitoringController().IsAlive();

            Assert.True(res);
        }
    }
}
