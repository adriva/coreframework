using System.Collections.Generic;

namespace Adriva.Documents.Abstractions
{
    public abstract class Document<TPart> : Document, IDocumentPartsContainer<TPart> where TPart : class, IDocumentPart
    {
        public abstract TPart AddPart();

        public abstract IEnumerable<TPart> GetParts();
    }
}