using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Adriva.Common.Core
{
    public static class FileProviderExtensions
    {

        public static string GetDirectoryName(this IFileInfo fileInfo, string relativeTo = null)
        {
            string directoryName = Path.GetDirectoryName(fileInfo.PhysicalPath);
            if (string.IsNullOrWhiteSpace(relativeTo))
            {
                return directoryName;
            }
            else
            {
                return Path.GetRelativePath(relativeTo, directoryName);
            }
        }

        public static async Task<string> ReadAllTextAsync(this IFileInfo fileInfo, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;

            using (var stream = fileInfo.CreateReadStream())
            using (var reader = new StreamReader(stream, encoding))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}