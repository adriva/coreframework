namespace Adriva.Documents.Abstractions
{
    public interface IDocumentFactoryProvider
    {
        TFactory GetFactory<TFactory>() where TFactory : IDocumentFactory;

        IDocumentFactory<TDocument> GetFactoryForDocument<TDocument>() where TDocument : IDocument;
    }
}