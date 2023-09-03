using System;
using System.Collections.Concurrent;
using DotJEM.Json.Index;
using DotJEM.Json.Index.Configuration;
using DotJEM.Json.Index.Contexts.Searching;
using DotJEM.Json.Index.Contexts.Storage;
using DotJEM.Json.Index.Results;
using DotJEM.Json.Index.Searching;
using Lucene.Net.Search;

namespace DotJEM.Json.Index.Contexts
{
    public interface IJsonIndexContext : IJsonIndexSearcherProvider
    {
        IServiceResolver Services { get; }

        IJsonIndex Open(string name);
    }

    public class JsonIndexContext : IJsonIndexContext
    {
        private readonly ILuceneJsonIndexFactory factory;
        private readonly ConcurrentDictionary<string, ILuceneJsonIndex> indices = new ConcurrentDictionary<string, ILuceneJsonIndex>();
        public IServiceResolver Services { get; }
        public JsonIndexContext(IServiceCollection services = null)
            : this(new LuceneIndexContextBuilder(), services) { }

        public JsonIndexContext(string path, IServiceCollection services = null)
            : this(new LuceneIndexContextBuilder(path), services) { }

        public JsonIndexContext(ILuceneJsonIndexFactory factory, IServiceCollection services = null)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.Services = new ServiceResolver(services ?? ServiceCollection.CreateDefault());
        }
        public JsonIndexContext(ILuceneJsonIndexFactory factory, IServiceResolver resolver)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.Services = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public IJsonIndex Open(string name)
        {
            return indices.GetOrAdd(name, factory.Create);
        }

        public IJsonIndexSearcher CreateSearcher()
        {
            return new LuceneJsonMultiIndexSearcher(indices.Values);
        }
    }
    
    public interface ILuceneIndexContextBuilder
    {
        IServiceCollection Services { get; }
        ILuceneIndexContextBuilder Configure(string name, Action<ILuceneJsonIndexBuilder> config);
        IJsonIndexContext Build();
    }

    public interface ILuceneJsonIndexFactory
    {
        ILuceneJsonIndex Create(string name);
    }

    public class LuceneIndexContextBuilder : ILuceneIndexContextBuilder, ILuceneJsonIndexFactory
    {
        private readonly ConcurrentDictionary<string, ILuceneJsonIndexBuilder> builders = new ConcurrentDictionary<string, ILuceneJsonIndexBuilder>();

        public IServiceCollection Services { get; }

        private readonly ILuceneStorageFactoryProvider storage;
        
        public LuceneIndexContextBuilder()
            : this(new RamStorageFacility(), ServiceCollection.CreateDefault()) { }

        public LuceneIndexContextBuilder(string path)
            : this(new SimpleFileSystemRootStorageFacility(path), ServiceCollection.CreateDefault()) { }

        public LuceneIndexContextBuilder(ILuceneStorageFactoryProvider storage, IServiceCollection services)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IJsonIndexContext Build()
        {
            return new JsonIndexContext(this, Services);
        }

        public ILuceneIndexContextBuilder Configure(string name, Action<ILuceneJsonIndexBuilder> config)
        {
            return Configure(name, builder =>
            {
                config(builder);
                return builder;
            });
        }

        private ILuceneIndexContextBuilder Configure(string name, Func<ILuceneJsonIndexBuilder, ILuceneJsonIndexBuilder> config)
        {
            ILuceneJsonIndexBuilder builder = config(new ContextedLuceneJsonIndexBuilder(name, Services).AddFacility(storage.Create(name)));
            builders.AddOrUpdate(name, builder, (s, a) => builder);
            return this;
        }

        ILuceneJsonIndex ILuceneJsonIndexFactory.Create(string name)
        {
            if (builders.TryGetValue(name, out ILuceneJsonIndexBuilder builder))
                return builder.Build();

            builder = new ContextedLuceneJsonIndexBuilder(name, Services).AddFacility(storage.Create(name));
            return builder.Build();
        }
    }

}