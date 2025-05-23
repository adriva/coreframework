namespace Adriva.Documents.Abstractions
{
    public interface INativeDocumentElement<T>
    {
        T Element { get; }
    }
}