using System;
using System.Threading.Tasks;
using Adriva.Extensions.Reporting.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.SqlServer
{
    public class SqlServerDataSource : IDataSource, IDisposable
    {
        private readonly ILogger Logger;
        private SqlConnection Connection;
        private bool IsDisposed;

        public SqlServerDataSource(ILoggerFactory loggerFactory)
        {
            this.Logger = loggerFactory.CreateLogger<SqlServerDataSource>();
        }

        public Task OpenAsync(DataSourceDefinition dataSourceDefinition)
        {
            this.Connection = new SqlConnection(dataSourceDefinition.ConnectionString);
            return this.Connection.OpenAsync();
        }

        protected virtual async Task<DataSet> GetDataAsync(ReportCommand command, FieldDefinition[] fields, SqlServerReportOutputOptions outputOptions)
        {
            using (SqlCommand sqlCommand = new SqlCommand(command.Text, this.Connection))
            {
                if (null != command.Parameters)
                {
                    foreach (var parameter in command.Parameters)
                    {
                        SqlParameter sqlParameter = new SqlParameter();
                        sqlParameter.ParameterName = parameter.Name;
                        sqlParameter.Value = parameter.FilterValue.Value ?? DBNull.Value;

                        sqlCommand.Parameters.Add(sqlParameter);
                    }
                }

                this.Logger.LogInformation($"Executing Sql Command : {command.Text}");

                if (null != command.Parameters)
                {
                    foreach (var parameter in command.Parameters)
                    {
                        this.Logger.LogInformation($"{parameter.Name} = {parameter.FilterValue.Value}");
                    }
                }

                DataSet dataSet = null;

                int[] columnIndices = null;
                long recordCount = long.MinValue;

                using (var dataReader = await sqlCommand.ExecuteReaderAsync(System.Data.CommandBehavior.SingleResult))
                {
                    if (null == fields || 0 == fields.Length)
                    {
                        fields = new FieldDefinition[dataReader.FieldCount];
                        for (int loop = 0; loop < dataReader.FieldCount; loop++)
                        {
                            fields[loop] = new FieldDefinition()
                            {
                                Name = dataReader.GetName(loop)
                            };
                        }
                    }

                    if (null == dataSet)
                    {
                        dataSet = DataSet.FromFields(fields);
                    }

                    if (null == columnIndices)
                    {
                        int loop = 0;
                        columnIndices = new int[fields.Length];

                        foreach (var field in fields)
                        {
                            try
                            {
                                columnIndices[loop] = dataReader.GetOrdinal(field.Name);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                columnIndices[loop] = -1;
                                this.Logger.LogWarning($"Field '{field.Name}' doesn't exist in the data retrieved from the source. This field will be populated with empty values.");
                            }
                            ++loop;
                        }
                    }

                    while (await dataReader.ReadAsync())
                    {
                        if (long.MinValue == recordCount && !string.IsNullOrWhiteSpace(outputOptions.RowCountField))
                        {
                            int rowCountFieldIndex = -1;
                            try
                            {
                                rowCountFieldIndex = dataReader.GetOrdinal(outputOptions.RowCountField);
                                recordCount = dataReader.GetFieldValue<long>(rowCountFieldIndex);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                recordCount = 0;
                            }
                        }

                        var dataRow = dataSet.CreateRow();

                        foreach (var columnIndex in columnIndices)
                        {
                            if (-1 < columnIndex)
                            {
                                object value = dataReader.GetValue(columnIndex);
                                dataRow.AddData(DBNull.Value == value ? null : value);
                            }
                            else
                            {
                                dataRow.AddData(null);
                            }
                        }
                    }
                }

                if (0 >= recordCount)
                {
                    recordCount = dataSet.Rows.Count;
                }

                dataSet.Metadata.RecordCount = recordCount;

                return dataSet;
            }
        }

        public Task<DataSet> GetDataAsync(ReportCommand command, FieldDefinition[] fields, JToken outputOptions)
        {
            var concreteOptions = outputOptions?.ToObject<SqlServerReportOutputOptions>();
            return this.GetDataAsync(command, fields, concreteOptions);
        }

        public Task CloseAsync()
        {
            return this.Connection.CloseAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.Connection?.Dispose();
                }

                this.IsDisposed = true;
                this.Connection = null;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
