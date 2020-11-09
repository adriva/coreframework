using System.IO;
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

    }
}