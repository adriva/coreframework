using System.Linq;
using Xunit;

namespace Adriva.Common.Core.Test
{
    public class AutoStream_Test
    {
        [Fact]
        public void WriteToAutoStream()
        {
            using (var stream = new AutoStream(100))
            {
                var buffer = Enumerable.Range(1, 100).Select(x => (byte)x).ToArray();
                stream.Write(buffer, 0, buffer.Length);

                Assert.True(null == stream.FilePath);

                stream.Write(buffer, 0, 10);
                Assert.True(null != stream.FilePath);
            }
        }
    }
}