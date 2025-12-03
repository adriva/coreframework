using System;
using System.IO;
using System.Threading.Tasks;

namespace Adriva.Documents.Abstractions
{
    public interface IDocument : IDocumentPartsContainer, IDisposable
    {
        bool CanSave { get; }

        Task SaveAsync(Stream stream);
    }
}