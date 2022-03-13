using System;
using System.Threading.Tasks;
using Adriva.Extensions.Reporting.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

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

        public async Task<DataSet> GetDataAsync(ReportCommand command, FieldDefinition[] fields)
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

                DataSet dataSet = DataSet.FromFields(fields);
                int[] columnIndices = null;

                this.Logger.LogInformation($"Executing Sql Command : {command.ToString()}");

                if (null != command.Parameters)
                {
                    foreach (var parameter in command.Parameters)
                    {
                        this.Logger.LogInformation($"{parameter.Name} = {parameter.FilterValue.Value}");
                    }
                }

                using (var dataReader = await sqlCommand.ExecuteReaderAsync(System.Data.CommandBehavior.SingleResult))
                {
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
                return dataSet;
            }
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
