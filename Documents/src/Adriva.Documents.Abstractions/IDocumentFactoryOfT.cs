using System.IO;
using System.Threading.Tasks;

namespace Adriva.Documents.Abstractions
{
    public interface IDocumentFactory<TDocument> where TDocument : IDocument
    {
        TDocument Create();

        Task<TDocument> LoadAsync(Stream stream);
    }
}