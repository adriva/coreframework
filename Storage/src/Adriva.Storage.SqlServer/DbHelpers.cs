using System.Threading.Tasks;
using Adriva.Common.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Adriva.Storage.SqlServer
{
    internal static class DbHelpers
    {
        public static async Task ExecuteScriptAsync(DatabaseFacade database, ISqlServerModelOptions options, ILogger logger, params string[] scriptNames)
        {
            var resourceFileProvider = new EmbeddedFileProvider(typeof(DbHelpers).Assembly);

            using (var transaction = await database.BeginTransactionAsync())
            {
                foreach (var scriptName in scriptNames)
                {
                    var scriptFile = resourceFileProvider.GetFileInfo($"{scriptName}.sql");

                    string sql = await scriptFile.ReadAllTextAsync();

                    sql = sql
                            .Replace("{SCHEMA}", options.SchemaName)
                            .Replace("{TABLE}", options.TableName)
                            .Replace("{PROC_RETRIEVE}", options.RetrieveProcedureName);

                    if (options is SqlServerBlobOptions blobOptions)
                    {
                        sql = sql
                                .Replace("{PROC_UPSERT}", blobOptions.UpsertProcedureName)
                                .Replace("{PROC_UPDATE}", blobOptions.UpdateProcedureName)
                                ;
                    }

                    logger.LogInformation($"Running script '{scriptName}'.");
                    logger.LogInformation(sql);

                    await database.ExecuteSqlRawAsync(sql);
                }

                await transaction.CommitAsync();
            }
        }
    }
}
