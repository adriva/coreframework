using System;
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
