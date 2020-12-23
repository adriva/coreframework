using System.Text;
using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public static class StorageClientExtensions
    {
        public static async Task<string> UpsertAsync(this IBlobClient blobClient, string name, string content, int cacheDuration = 0)
        {
            content = content ?? string.Empty;
            var buffer = Encoding.UTF8.GetBytes(content);
            return await blobClient.UpsertAsync(name, buffer, cacheDuration);
        }
    }
}