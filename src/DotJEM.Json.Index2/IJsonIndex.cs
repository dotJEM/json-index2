using System;
using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Searching;
using DotJEM.Json.Index2.Storage;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2
{
    public interface IJsonIndexSearcherProvider
    {
        IJsonIndexSearcher CreateSearcher();
    }

    public interface IJsonIndex : IJsonIndexSearcherProvider
    {
        IInfoStream InfoStream { get; }
        IJsonIndexStorageManager Storage { get; }
        IJsonIndexConfiguration Configuration { get; }
        IIndexWriterManager WriterManager { get; }
        IIndexSearcherManager SearcherManager { get; }
        IJsonIndexWriter CreateWriter();

        void Close();
    }

    public class JsonIndex : IJsonIndex
    {
        public IInfoStream InfoStream { get; } = new InfoStream<JsonIndex>();
        public IJsonIndexStorageManager Storage { get; }
        public IJsonIndexConfiguration Configuration { get; }
        public IIndexWriterManager WriterManager => Storage.WriterManager;
        public IIndexSearcherManager SearcherManager => Storage.SearcherManager;

        public JsonIndex()
            : this(new RamJsonIndexStorage(), new JsonIndexConfiguration())
        {
        }

        public JsonIndex(string path)
            : this(new SimpleFsJsonIndexStorage(path), new JsonIndexConfiguration())
        {
        }

        public JsonIndex(IJsonIndexStorage storage, IJsonIndexConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Storage = new JsonIndexStorageManager(this, storage);
        }

        //public JsonIndex(ILuceneStorageFactory storage, IJsonIndexConfiguration configuration, IServiceResolver services)
        //{
        //    Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        //    Services = services ?? throw new ArgumentNullException(nameof(services));

        //    Storage = storage.Create(this, configuration.Version);
        //}

        public IJsonIndexSearcher CreateSearcher()
        {
            return new JsonIndexSearcher(this);
        }

        public IJsonIndexWriter CreateWriter()
        {
            IJsonIndexWriterProvider provider = Configuration.Resolve<IJsonIndexWriterProvider>();
            return provider.Get();
        }

        public void Close()
        {
            WriterManager.Close();
            Storage.Close();
        }
    }

  
}