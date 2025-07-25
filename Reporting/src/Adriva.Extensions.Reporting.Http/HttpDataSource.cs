using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Reporting.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Http;

public abstract class HttpDataSource : IDataSource
{
    private readonly IHttpClientFactory HttpClientFactory;
    private HttpClient HttpClient;

    protected ILogger Logger { get; private set; }

    public abstract ValueTask PopulateDatasetAsync(string content, ReportCommand command, DataSet dataSet);

    public virtual async ValueTask DecorateRequestAsync(HttpRequestMessage request, ReportCommand command, FieldDefinition[] fields)
    {
        await Task.CompletedTask;
    }

    public virtual async ValueTask DecorateDatasetAsync(DataSet dataset, ReportCommand command, JToken outputOptions)
    {
        await Task.CompletedTask;
    }

    public virtual Task<string> ProcessResponseContentAsync(string content) => Task.FromResult(content);

    public HttpDataSource(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        this.Logger = loggerFactory.CreateLogger(this.GetType());
        this.HttpClientFactory = httpClientFactory;
    }

    public Task OpenAsync(DataSourceDefinition dataSourceDefinition)
    {
        if (!Uri.TryCreate(dataSourceDefinition.ConnectionString, UriKind.Absolute, out Uri baseUri) || !Utilities.IsValidHttpUri(baseUri))
        {
            throw new InvalidOperationException($"Provided http connection string '{dataSourceDefinition?.ConnectionString}' is not a valid absolute Http or Https Uri.");
        }

        this.HttpClient = this.HttpClientFactory.CreateClient($"{dataSourceDefinition.Type}_HttpClient");
        this.HttpClient.BaseAddress = baseUri;

        this.Logger.LogInformation($"Http data source created named client '{dataSourceDefinition.Type}_HttpClient' targeting '{baseUri}'.");

        return Task.CompletedTask;
    }

    public async Task<DataSet> GetDataAsync(ReportCommand command, FieldDefinition[] fields, JToken outputOptions)
    {
        HttpCommandOptions commandOptions = new();

        if (null != command.CommandDefinition.Options)
        {
            commandOptions = command.CommandDefinition.Options.ToObject<HttpCommandOptions>();
            this.Logger.LogTrace($"Http data source using options {Utilities.SafeSerialize(commandOptions)}");
        }

        StringBuilder commandBuffer = new(command.Text);

        foreach (var parameter in command.Parameters)
        {
            commandBuffer.Replace(parameter.Name, Convert.ToString(parameter.FilterValue.Value));
        }

        string contentText = null;

        using var request = new HttpRequestMessage(new HttpMethod(commandOptions.Method), commandBuffer.ToString());

        await this.DecorateRequestAsync(request, command, fields);

        using var response = await this.HttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

        if (!response.IsSuccessStatusCode)
        {
            if (commandOptions.ThrowDetailedHttpErrors)
            {
                contentText = await response.Content.ReadAsStringAsync();
            }

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception httpError)
            {
                throw new HttpRequestException(string.IsNullOrWhiteSpace(contentText) ? "Http error retrieving data." : contentText, httpError);
            }
        }
        else
        {
            contentText = await response.Content.ReadAsStringAsync();
        }


        contentText = await this.ProcessResponseContentAsync(contentText);

        var dataset = DataSet.FromFields(fields);

        await this.PopulateDatasetAsync(contentText, command, dataset);

        await this.DecorateDatasetAsync(dataset, command, outputOptions);

        return dataset;
    }

    public Task CloseAsync()
    {
        return Task.CompletedTask;
    }
}
