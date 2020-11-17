using System;
using System.Collections.Generic;
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

        public async Task<DataSet> GetDataAsync(ReportCommand command, IEnumerable<FieldDefinition> fields)
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

                using (var dataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    int[] columnIndices = fields.Select(f => dataReader.GetOrdinal(f.Name)).ToArray();

                    while (await dataReader.ReadAsync())
                    {
                        object[] rowData = new object[columnIndices.Length];

                        for (int loop = 0; loop < columnIndices.Length; loop++)
                        {
                            rowData[loop] = dataReader.GetValue(loop);
                        }

                        DataRow dataRow = new DataRow(rowData);
                    }
                }
            }

            return null;
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
