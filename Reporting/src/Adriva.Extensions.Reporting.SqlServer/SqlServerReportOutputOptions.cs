using Newtonsoft.Json;

namespace Adriva.Extensions.Reporting.SqlServer;

[JsonObject]
public class SqlServerReportOutputOptions
{
    public string RowCountField { get; set; }

    public string PageNumberFilter { get; set; }

    public string PageSizeFilter { get; set; }
}
