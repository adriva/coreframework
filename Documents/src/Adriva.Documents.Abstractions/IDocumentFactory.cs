using System.IO;
using System.Threading.Tasks;

namespace Adriva.Documents.Abstractions
{
    public interface IDocumentFactory
    {
        IDocument Create();

        Task<IDocument> LoadAsync(Stream stream);
    }
}