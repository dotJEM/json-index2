using DotJEM.Json.Index2.Documents;

namespace DotJEM.Json.Index2.IO;

public interface IJsonIndexWriterProvider
{
    IJsonIndexWriter Get();
}

public class SyncJsonIndexWriterProvider : IJsonIndexWriterProvider
{
    private JsonIndexWriter cache;

    private readonly IJsonIndex index;
    private readonly ILuceneDocumentFactory factory;
    private readonly IIndexWriterManager manager;

    public SyncJsonIndexWriterProvider(IJsonIndex index, ILuceneDocumentFactory factory, IIndexWriterManager manager)
    {
        this.index = index;
        this.factory = factory;
        this.manager = manager;
    }


    public IJsonIndexWriter Get() => cache ??= new JsonIndexWriter(index, factory, manager);
}