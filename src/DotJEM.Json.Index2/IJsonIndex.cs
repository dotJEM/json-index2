using System;
using DotJEM.Json.Index.Configuration;
using DotJEM.Json.Index.IO;
using DotJEM.Json.Index.Searching;
using DotJEM.Json.Index.Storage;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index
{
    public interface IJsonIndexSearcherProvider
    {
        IJsonIndexSearcher CreateSearcher();
    }

    public interface IJsonIndex : IJsonIndexSearcherProvider
    {
        IInfoStream InfoStream { get; }
        IServiceResolver Services { get; }
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
        public IServiceResolver Services { get; }

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
            Configuration = configuration.AsReadOnly() ?? throw new ArgumentNullException(nameof(configuration));
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
            IJsonIndexWriterProvider provider = Services.Resolve<IJsonIndexWriterProvider>();
            return provider.Get();
        }

        public void Close()
        {
            WriterManager.Close();
            Storage.Close();
        }
    }

  
}