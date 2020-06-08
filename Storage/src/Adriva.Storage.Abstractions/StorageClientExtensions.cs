using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public static class StorageClientExtensions
    {
        public static async Task<string> UpsertAsync(this IBlobClient blobClient, string name, string content, int cacheDuration = 0)
        {
            content = content ?? string.Empty;

            using (Stream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(content);
                stream.Seek(0, SeekOrigin.Begin);
                return await blobClient.UpsertAsync(name, stream, cacheDuration);
            }
        }
    }
}