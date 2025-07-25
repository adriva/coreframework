using System.Collections.Generic;

namespace Adriva.Documents.Abstractions
{
    public interface IDocumentPartsContainer
    {
        IEnumerable<T> GetParts<T>() where T : IDocumentPart;

        T AddPart<T>() where T : class, IDocumentPart;
    }
}