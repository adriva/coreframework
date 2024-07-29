using System.Globalization;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Adriva.Storage.SqlServer
{
    internal static class DbHelpers
    {
        private static string DelimitIdentifier(string identifier)
        {
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        public static string GetQualifiedTableName(ISqlServerModelOptions options)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", DbHelpers.DelimitIdentifier(options.SchemaName), DbHelpers.DelimitIdentifier(options.TableName));
        }

        public static string GetQualifiedProcedureName(ISqlServerModelOptions options)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", DbHelpers.DelimitIdentifier(options.SchemaName), DbHelpers.DelimitIdentifier(options.RetrieveProcedureName));
        }

        public static async Task ExecuteSqlAsync(string sql, ISqlServerModelOptions options)
        {
            using (var connection = new SqlConnection(options.ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task ExecuteScriptAsync(ISqlServerModelOptions options, ILogger logger, params string[] scriptNames)
        {
            var resourceFileProvider = new EmbeddedFileProvider(typeof(DbHelpers).Assembly);

            using (var connection = new SqlConnection(options.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var scriptName in scriptNames)
                    {
                        var scriptFile = resourceFileProvider.GetFileInfo($"{scriptName}.sql");

                        string sql = await scriptFile.ReadAllTextAsync();

                        sql = sql
                                .Replace("{SCHEMA}", options.SchemaName)
                                .Replace("{TABLE}", options.TableName)
                                .Replace("{PROC_RETRIEVE}", options.RetrieveProcedureName);

                        logger.LogInformation($"Running script '{scriptName}'.");
                        logger.LogDebug(sql);

                        using (var command = new SqlCommand(sql, connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    await transaction.CommitAsync();
                }
            }
        }
    }
}
