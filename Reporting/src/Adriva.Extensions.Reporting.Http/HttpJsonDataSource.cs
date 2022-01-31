using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using Adriva.Extensions.Reporting.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Http
{
    public class HttpJsonDataSource : HttpDataSource
    {
        private readonly ArrayPool<object> ArrayPool;

        public HttpJsonDataSource(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory) : base(httpClientFactory, loggerFactory)
        {
            this.ArrayPool = ArrayPool<object>.Shared;
        }

        public override void PopulateDataset(string content, HttpCommandOptions commandOptions, DataSet dataSet)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            JToken jToken = JToken.Parse(content);

            if (!string.IsNullOrWhiteSpace(commandOptions.DataElement))
            {
                jToken = jToken[commandOptions.DataElement];
            }

            if (null == jToken)
            {
                return;
            }

            Dictionary<DataColumn, Func<JToken, object>> columnPopulators = new Dictionary<DataColumn, Func<JToken, object>>();

            foreach (var column in dataSet.Columns)
            {
                columnPopulators[column] = this.BuildColumnMapping(column);
            }

            if (JTokenType.Array == jToken.Type)
            {
                var jArray = (JArray)jToken;

                foreach (var jrowToken in jArray)
                {
                    object[] rowData = this.ArrayPool.Rent(dataSet.Columns.Count);

                    try
                    {
                        this.PopulateDataRow(rowData, jrowToken as JObject, dataSet.Columns, columnPopulators);

                        DataRow row = dataSet.CreateRow();
                        for (int loop = 0; loop < dataSet.Columns.Count; loop++)
                        {
                            row.AddData(rowData[loop]);
                        }
                    }
                    finally
                    {
                        this.ArrayPool.Return(rowData);
                    }
                }
            }
            else if (JTokenType.Object == jToken.Type)
            {
                object[] rowData = this.ArrayPool.Rent(dataSet.Columns.Count);
                try
                {
                    this.PopulateDataRow(rowData, jToken as JObject, dataSet.Columns, columnPopulators);
                    DataRow row = dataSet.CreateRow();
                    for (int loop = 0; loop < dataSet.Columns.Count; loop++)
                    {
                        row.AddData(rowData[loop]);
                    }
                }
                finally
                {
                    this.ArrayPool.Return(rowData);
                }
            }
            else
            {
                throw new NotSupportedException($"JToken type '{jToken.Type}' is not supported by the Http Json data source.");
            }
        }

        protected virtual Func<JToken, object> BuildColumnMapping(DataColumn column)
        {
            string[] fieldNames = column.Name.Split('.', StringSplitOptions.RemoveEmptyEntries);

            return jtoken =>
            {
                JToken leafToken = jtoken;
                foreach (var fieldName in fieldNames)
                {
                    if (null != leafToken.SelectToken(fieldName))
                    {
                        leafToken = leafToken[fieldName];
                    }
                    else
                    {
                        return null;
                    }
                }

                object jValueCandidate = leafToken.Value<object>();

                if (jValueCandidate is JValue jvalue)
                {
                    return jvalue.Value;
                }
                else
                {
                    return null;
                }
            };
        }

        protected virtual void PopulateDataRow(object[] rowData, JObject jObject, ReadOnlyCollection<DataColumn> columns, IDictionary<DataColumn, Func<JToken, object>> columnPopulators)
        {
            for (int loop = 0; loop < columns.Count; loop++)
            {
                DataColumn column = columns[loop];
                rowData[loop] = columnPopulators[column](jObject);
            }
        }
    }
}