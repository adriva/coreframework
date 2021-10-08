using System.Threading.Tasks;
using Adriva.Common.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.FileProviders;

namespace Adriva.Storage.SqlServer
{
    internal static class DbHelpers
    {
        public static async Task ExecuteScriptAsync(DatabaseFacade database, ISqlServerModelOptions options, params string[] scriptNames)
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

                    await database.ExecuteSqlRawAsync(sql);
                }

                await transaction.CommitAsync();
            }
        }
    }
}
