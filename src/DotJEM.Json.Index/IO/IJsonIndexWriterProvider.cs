using DotJEM.Json.Index.Documents;

namespace DotJEM.Json.Index.IO
{
    public interface IJsonIndexWriterProvider
    {
        IJsonIndexWriter Get();
    }

    public class SyncJsonIndexWriterProvider : IJsonIndexWriterProvider
    {
        private SyncJsonIndexWriter cache;

        private IJsonIndex index;
        private ILuceneDocumentFactory factory;
        private IIndexWriterManager manager;

        public SyncJsonIndexWriterProvider(IJsonIndex index, ILuceneDocumentFactory factory, IIndexWriterManager manager)
        {
            this.index = index;
            this.factory = factory;
            this.manager = manager;
        }


        public SyncJsonIndexWriterProvider(SyncJsonIndexWriter cache)
        {
            this.cache = cache;
        }

        public IJsonIndexWriter Get() => cache ??= new SyncJsonIndexWriter(index, factory, manager);
    }

    public class AsyncJsonIndexWriterProvider : IJsonIndexWriterProvider
    {
        private AsyncJsonIndexWriter cache;

        private IJsonIndex index;
        private ILuceneDocumentFactory factory;
        private IIndexWriterManager manager;

        public AsyncJsonIndexWriterProvider(IJsonIndex index, ILuceneDocumentFactory factory, IIndexWriterManager manager)
        {
            this.index = index;
            this.factory = factory;
            this.manager = manager;
        }

        public IJsonIndexWriter Get() => cache ??= new AsyncJsonIndexWriter(index, factory, manager);
    }

}