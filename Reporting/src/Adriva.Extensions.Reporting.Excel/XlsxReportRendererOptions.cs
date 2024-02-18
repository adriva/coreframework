using Newtonsoft.Json;

namespace Adriva.Extensions.Reporting.Excel
{
    public class XlsxReportRendererOptions
    {
        [JsonIgnore]
        public const string KeyName = "XlsxRenderer";

        public bool CreateTable { get; set; } = true;

        public string TemplatePath { get; set; }

        public string TargetTableName { get; set; }
    }
}
