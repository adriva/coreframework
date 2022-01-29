using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Reporting.Abstractions;

namespace Adriva.Extensions.Reporting.Http
{
    public abstract class HttpDataSource : IDataSource
    {
        private readonly IHttpClientFactory HttpClientFactory;
        private HttpClient HttpClient;

        public abstract void PopulateDataset(string content, HttpCommandOptions commandOptions, DataSet dataSet);

        public HttpDataSource(IHttpClientFactory httpClientFactory)
        {
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
            return Task.CompletedTask;
        }

        public async Task<DataSet> GetDataAsync(ReportCommand command, FieldDefinition[] fields)
        {
            HttpCommandOptions commandOptions = new HttpCommandOptions();

            if (null != command.CommandDefinition.Options)
            {
                commandOptions = command.CommandDefinition.Options.ToObject<HttpCommandOptions>();
            }

            StringBuilder commandBuffer = new StringBuilder(command.Text);

            foreach (var parameter in command.Parameters)
            {
                commandBuffer.Replace(parameter.Name, Convert.ToString(parameter.FilterValue.Value));
            }

            string contentText = null;

            using (var request = new HttpRequestMessage(new HttpMethod(commandOptions.Method), commandBuffer.ToString()))
            {
                using (var response = await this.HttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                {
                    response.EnsureSuccessStatusCode();

                    contentText = await response.Content.ReadAsStringAsync();
                }
            }


            var dataset = DataSet.FromFields(fields);

            this.PopulateDataset(contentText, commandOptions, dataset);

            return dataset;
        }

        public Task CloseAsync()
        {
            this.HttpClient.DefaultRequestHeaders.Clear();
            return Task.CompletedTask;
        }
    }
}