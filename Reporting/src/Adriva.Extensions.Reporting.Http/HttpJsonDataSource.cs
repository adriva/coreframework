using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Adriva.Extensions.Reporting.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Http
{
    public class HttpJsonDataSource : HttpDataSource
    {
        public HttpJsonDataSource(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory) : base(httpClientFactory, loggerFactory)
        {

        }

        protected virtual Task<JToken> DecorateJsonResponseAsync(JToken responseJsonToken) => Task.FromResult(responseJsonToken);

        protected virtual async ValueTask DecorateJsonDatasetAsync(DataSet dataset, ReportCommand command, JToken responseJsonToken)
        {
            await Task.CompletedTask;
        }

        protected virtual IDictionary<string, object> ParseRowContainer(JContainer jContainer)
        {
            if (null == jContainer)
            {
                return null;
            }

            return jContainer
                        .Descendants()
                        .OfType<JValue>()
                        .ToDictionary(x => x.Path, x => x.Value, StringComparer.OrdinalIgnoreCase);

        }

        public override async ValueTask PopulateDatasetAsync(string content, ReportCommand command, DataSet dataSet)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            JToken jToken = JToken.Parse(content);

            jToken = await this.DecorateJsonResponseAsync(jToken);

            if (null != command.CommandDefinition.Options)
            {
                var commandOptions = command.CommandDefinition.Options.ToObject<HttpCommandOptions>();

                if (!string.IsNullOrWhiteSpace(commandOptions.DataElement))
                {
                    jToken = jToken[commandOptions.DataElement];
                }
            }

            if (null == jToken)
            {
                return;
            }

            JArray jarray;

            if (JTokenType.Object == jToken.Type)
            {
                jarray = new JArray();
                jarray.Add(jToken);
            }
            else if (JTokenType.Array == jToken.Type)
            {
                jarray = (JArray)jToken;
            }
            else
            {
                throw new NotSupportedException($"Only json array and object is supported as root. Current json element is of type '{jToken.Type}'.");
            }

            foreach (var jArrayItem in jarray.Children<JContainer>())
            {
                var jRowContainer = (JContainer)jArrayItem.DeepClone();
                var dictionary = this.ParseRowContainer(jRowContainer);

                if (null != dictionary)
                {
                    var dataRow = dataSet.CreateRow();

                    foreach (var dataColumn in dataSet.Columns)
                    {
                        if (dictionary.ContainsKey(dataColumn.Name))
                        {
                            dataRow.AddData(dictionary[dataColumn.Name]);
                        }
                        else
                        {
                            dataRow.AddData(null);
                        }
                    }
                }
            }

            await this.DecorateJsonDatasetAsync(dataSet, command, jToken);
        }
    }
}