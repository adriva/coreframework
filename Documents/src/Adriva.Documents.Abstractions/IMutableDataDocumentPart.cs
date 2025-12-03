namespace Adriva.Documents.Abstractions
{
    public interface IMutableDataDocumentPart<in TData, in TOptions> : IDocumentPart where TOptions : struct
    {
        void AppendData(TData data, TOptions options);

        void DeleteData(TOptions options, params TOptions[] argv);

        void UpsertData(TData data, TOptions options);
    }
}