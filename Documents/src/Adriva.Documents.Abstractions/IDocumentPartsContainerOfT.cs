using System.Collections.Generic;

namespace Adriva.Documents.Abstractions
{
    public interface IDocumentPartsContainer<out T> : IDocumentPartsContainer where T : class, IDocumentPart
    {
        IEnumerable<T> GetParts();

        T AddPart();
    }
}