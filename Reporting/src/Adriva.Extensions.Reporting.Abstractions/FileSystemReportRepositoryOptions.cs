using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class FileSystemReportRepositoryOptions
    {
        public string RootPath { get; set; } = Environment.CurrentDirectory;
    }
}