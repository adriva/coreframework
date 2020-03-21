using Xunit;

namespace Adriva.Common.Core.Test
{
    public class Utilities_Test
    {
        [Fact]
        public void ExtractMoney()
        {
            decimal output = Utilities.ExtractDecimal("1234.56");
            Assert.True(output == 1234.56M);
            output = Utilities.ExtractDecimal("1,234.56");
            Assert.True(output == 1234.56M);
            output = Utilities.ExtractDecimal("1234,56");
            Assert.True(output == 1234.56M);
            output = Utilities.ExtractDecimal("1.234,56");
            Assert.True(output == 1234.56M);

        }
    }
}