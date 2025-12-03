using System;
using System.Net.Http;
using Adriva.Extensions.Reporting.Abstractions;
using Adriva.Extensions.Reporting.Http;

namespace Microsoft.Extensions.DependencyInjection;

public static class ReportingBuilderHttpExtensions
{
    public static IReportingBuilder UseHttpJson(this IReportingBuilder builder, string dataSourceTypeName = "HttpJson", Action<HttpClient> configure = null)
    {
        if (string.IsNullOrWhiteSpace(dataSourceTypeName))
        {
            throw new ArgumentException("The data source type name cannot be empty or null.", nameof(dataSourceTypeName));
        }

        configure ??= (_ => { });

        builder.Services.AddHttpClient($"{dataSourceTypeName}_HttpClient", configure);
        return builder.UseDataSource<HttpJsonDataSource>(dataSourceTypeName);
    }
}