using Newtonsoft.Json;

namespace Adriva.Extensions.Reporting.SqlServer
{
    [JsonObject]
    public class SqlServerReportOutputOptions
    {
        public string RowCountField { get; set; }
    }
}
