using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public interface IStorageClient
    {

    }

    public interface IAsyncInitializedStorageClient<in TOptions> where TOptions : class, new()
    {
        ValueTask InitializeAsync(TOptions options);
    }
}