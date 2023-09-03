using DotJEM.Json.Index.Documents;

namespace DotJEM.Json.Index.IO
{
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


}