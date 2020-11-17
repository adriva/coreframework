using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Adriva.Extensions.Reporting.Abstractions;
using Microsoft.Data.SqlClient;

namespace Adriva.Extensions.Reporting.SqlServer
{
    public class SqlServerDataSource : IDataSource, IDisposable
    {
        private SqlConnection Connection;
        private bool IsDisposed;

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
                using (var dataReader = await sqlCommand.ExecuteReaderAsync(System.Data.CommandBehavior.SingleResult))
                {
                    if (null == columnIndices)
                    {
                        try
                        {
                            columnIndices = fields.Select(f => dataReader.GetOrdinal(f.Name)).ToArray();
                        }
                        catch (IndexOutOfRangeException indexOutOfRangeException)
                        {
                            throw new IndexOutOfRangeException($"Field '{indexOutOfRangeException.Message}' could not be found in the retrieved dataset.", indexOutOfRangeException);
                        }
                    }

                    while (await dataReader.ReadAsync())
                    {
                        var dataRow = dataSet.CreateRow();

                        foreach (var columnIndex in columnIndices)
                        {
                            object value = dataReader.GetValue(columnIndex);
                            dataRow.AddData(DBNull.Value == value ? null : value);
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
