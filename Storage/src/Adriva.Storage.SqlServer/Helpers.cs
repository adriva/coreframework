using System.Data;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileProviders;

namespace Adriva.Storage.SqlServer;

internal static class Helpers
{
    private static async Task<string> GetQueryFromResourceAsync(string scriptName, SqlServerBlobManagerConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(scriptName);
        ArgumentNullException.ThrowIfNull(configuration);

        var resourceFileProvider = new EmbeddedFileProvider(typeof(Helpers).Assembly);
        var fileInfo = resourceFileProvider.GetFileInfo(scriptName);

        if (!fileInfo.Exists)
        {
            throw new InvalidOperationException("!!!!");
        }

        using Stream stream = fileInfo.CreateReadStream();
        using StreamReader reader = new(stream, Encoding.UTF8);

        StringBuilder buffer = new(await reader.ReadToEndAsync());

        buffer
            .Replace("$SchemaName$", configuration.SchemaName)
            .Replace("$TableName$", configuration.TableName)
            .Replace("$FileGroupName$", configuration.FileGroupName)
            .Replace("$UpsertName$", configuration.UpsertProcedureName)
            ;

        return buffer.ToString();
    }

    internal static async Task RunDbCommandAsync(SqlServerBlobManagerConfiguration configuration, Func<SqlCommand, Task> callback)
    {
        await using var connection = new SqlConnection(configuration.ConnectionString);

        await connection.OpenAsync();

        await using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

        using (var command = new SqlCommand())
        {
            command.Transaction = transaction;
            command.Connection = connection;
            await callback(command);
        }

        await transaction.CommitAsync();
    }

    internal static async Task CreateTablesAsync(SqlServerBlobManagerConfiguration configuration)
    {
        string sql = await Helpers.GetQueryFromResourceAsync("create-file-table.sql", configuration);
        await Helpers.RunDbCommandAsync(configuration, async command =>
        {
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            await command.ExecuteNonQueryAsync();
        });
    }

    internal static async Task CreateStoredProceduresAsync(SqlServerBlobManagerConfiguration configuration)
    {
        string sql = await Helpers.GetQueryFromResourceAsync("create-sp-upsert.sql", configuration);
        await Helpers.RunDbCommandAsync(configuration, async command =>
        {
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            await command.ExecuteNonQueryAsync();
        });
    }

    internal static void CheckName(string name, bool isContainer)
    {
        string parameterName = isContainer ? nameof(SqlServerBlob.Container) : nameof(SqlServerBlob.Name);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"{parameterName} cannot be null or empty.");
        }

        char[] invalidChars = Path.GetInvalidPathChars();

        if (name.Any(c => invalidChars.Any(x => x == c)))
        {
            throw new ArgumentException($"Invalid {parameterName} specified. '{name}' {parameterName} cannot contain any of {string.Join(',', invalidChars)}.");
        }

        if (!isContainer)
        {
            invalidChars = [.. Path.GetInvalidFileNameChars(), Path.PathSeparator, Path.VolumeSeparatorChar, Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

            if (name.Any(c => invalidChars.Any(x => x == c)))
            {
                throw new ArgumentException($"Invalid {parameterName} specified. '{name}' {parameterName} cannot contain any of {string.Join(',', invalidChars)}.");
            }
        }
        else
        {
            try
            {
                PathString containerPath = new(name);
            }
            catch (Exception pathError)
            {
                throw new ArgumentException($"Invalid {parameterName} specified '{name}'. {pathError.Message}");
            }
        }
    }
}