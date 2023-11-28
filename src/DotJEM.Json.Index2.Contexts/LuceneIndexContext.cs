using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Contexts.Searching;
using DotJEM.Json.Index2.Contexts.Storage;
using DotJEM.Json.Index2.Searching;
using DotJEM.Json.Index2.Storage;
using Lucene.Net.Util;

namespace DotJEM.Json.Index2.Contexts;

public interface IJsonIndexContext 
{
    IJsonIndex Open(string index);
    IJsonIndex Open(string group, string index);
    IJsonIndexSearcher CreateSearcher();
    IJsonIndexSearcher CreateSearcher(string group);
}

public class JsonIndexContext : IJsonIndexContext
{
    private readonly IJsonIndexFactory factory;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, IJsonIndex>> indices = new ();

    public JsonIndexContext(IJsonIndexFactory factory)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public IJsonIndex Open(string index)
        => Open("*", index);

    public IJsonIndex Open(string group, string index)
    {
        ConcurrentDictionary<string, IJsonIndex> map = indices.GetOrAdd(group, s => new ConcurrentDictionary<string, IJsonIndex>());
        return map.GetOrAdd(index, factory.Create);
    }

    public IJsonIndexSearcher CreateSearcher()
        => CreateSearcher("*");

    public IJsonIndexSearcher CreateSearcher(string group)
    {
        ConcurrentDictionary<string, IJsonIndex> map = indices.GetOrAdd(group, s => new ConcurrentDictionary<string, IJsonIndex>());
        return new LuceneJsonMultiIndexSearcher(map.Values);
    }
}

public interface IJsonIndexContextBuilder
{
    IJsonIndexContextBuilder ByDefault(Action<IJsonIndexBuilder> configure);
    IJsonIndexContextBuilder For(string group, Action<IJsonIndexBuilder> configure);
    IJsonIndexContext Build();
}

public class JsonIndexContextBuilder : IJsonIndexContextBuilder
{
    private readonly ConcurrentDictionary<string, Action<IJsonIndexBuilder>> configurators = new();

    public IJsonIndexContextBuilder ByDefault(Action<IJsonIndexBuilder> configure)
    {
        configurators.AddOrUpdate("*", s => configure, (s, func) => configure);
        return this;
    }

    public IJsonIndexContextBuilder For(string group, Action<IJsonIndexBuilder> configure)
    {
        if (group == null) throw new ArgumentNullException(nameof(group));
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        if (group is "*" or "") throw new ArgumentException("Invalid name for an index.", nameof(group));

        configurators.AddOrUpdate(group, s => configure, (s, func) => configure);
        return this;
    }

    public IJsonIndexContext Build()
    {
        return new JsonIndexContext(new JsonIndexFactory(new Dictionary<string, Action<IJsonIndexBuilder>>(configurators)));
    }
}

public interface IJsonIndexFactory
{
    IJsonIndex Create(string name);
}

public class JsonIndexFactory : IJsonIndexFactory
{
    private readonly IReadOnlyDictionary<string, Action<IJsonIndexBuilder>> configurators;
    private readonly ConcurrentDictionary<string, Entry> configurations = new();

    public JsonIndexFactory(IReadOnlyDictionary<string,Action<IJsonIndexBuilder>> configurators)
    {
        this.configurators = configurators;
    }

    public IJsonIndex Create(string name)
    {
        Entry entry = configurations.GetOrAdd(name, CreateConfiguration);
        return new JsonIndex(entry.StorageProvider, entry.Configuration);
    }

    private Entry CreateConfiguration(string group)
    {
        if (!TryGetConfigurator(group, out Action<IJsonIndexBuilder> configure)) 
            throw new ArgumentException("No configurators found for the given group and no default configurators provided.", nameof(group));
        
        JsonIndexBuilderForContexts builder = new JsonIndexBuilderForContexts();
        configure(builder);

        //TODO: StorageProviderFactory instead.
        return new Entry(builder.StorageProvider, builder.BuildConfiguration());
    }

    private bool TryGetConfigurator(string group, out Action<IJsonIndexBuilder> cfg)
    {
        return configurators.TryGetValue(group, out cfg)
               || configurators.TryGetValue("*", out cfg);
    }

    private record struct Entry(IIndexStorageProvider StorageProvider, IJsonIndexConfiguration Configuration);
}

public class JsonIndexBuilderForContexts : IJsonIndexBuilder
{
    public string Name { get; } = Guid.NewGuid().ToString("D");
    public IIndexStorageProvider StorageProvider { get; private set; } = new RamIndexStorageProvider();
    private readonly Dictionary<Type, Func<IJsonIndexConfiguration, object>> factories = new();

    public IJsonIndexBuilder UsingStorage(IIndexStorageProvider storageProvider)
    {
        this.StorageProvider = storageProvider;
        return this;
    }

    public IJsonIndexBuilder WithService<TService>(bool replace, Func<IJsonIndexConfiguration, TService> factory)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        if (replace)
            factories[typeof(TService)] = cfg => factory(cfg);
        else
        {

#if NETSTANDARD2_0
            if(!factories.ContainsKey(typeof(TService)))
                factories.Add(typeof(TService), cfg => factory(cfg));
#else
            factories.TryAdd(typeof(TService), cfg => factory(cfg));
#endif
        }
        return this;
    }

    public IJsonIndex Build()
    {
        return new JsonIndex(StorageProvider, BuildConfiguration());
    }

    public IJsonIndexConfiguration BuildConfiguration()
    {
        return new JsonIndexConfiguration(LuceneVersion.LUCENE_48, factories
            .Select(pair => new ServiceDescriptor(pair.Key, pair.Value)));
    }
}
