using System.IO;
using System.Threading.Tasks;
using Adriva.Documents.Abstractions;

namespace Adriva.Documents.Excel
{
    public class SpreadsheetDocumentFactory : IDocumentFactory<SpreadsheetDocument>, IDocumentFactory
    {
        IDocument IDocumentFactory.Create() => ((IDocumentFactory<SpreadsheetDocument>)this).Create();

        async Task<IDocument> IDocumentFactory.LoadAsync(Stream stream) => await ((IDocumentFactory<SpreadsheetDocument>)this).LoadAsync(stream);

        public SpreadsheetDocument Create() => new SpreadsheetDocument();

        public Task<SpreadsheetDocument> LoadAsync(Stream stream)
        {
            var spreadsheetDocument = new SpreadsheetDocument(stream);
            return Task.FromResult(spreadsheetDocument);
        }
    }
}