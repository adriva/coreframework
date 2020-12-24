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

            using (var stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(content);
                await writer.FlushAsync();
                stream.Seek(0, SeekOrigin.Begin);
                return await blobClient.UpsertAsync(name, stream, cacheDuration);
            }
        }

        public static async Task UpdateAsync(this IBlobClient blobClient, string name, string content, string etag)
        {
            content = content ?? string.Empty;
            using (var stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(content);
                await writer.FlushAsync();
                stream.Seek(0, SeekOrigin.Begin);
                await blobClient.UpdateAsync(name, stream, etag);
            }
        }
    }
}