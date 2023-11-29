using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
    IJsonIndexContextBuilder ByDefault(Action<IJsonIndexBuilderForContexts> configure);
    IJsonIndexContextBuilder For(string group, Action<IJsonIndexBuilderForContexts> configure);
    IJsonIndexContext Build();
}

public class JsonIndexContextBuilder : IJsonIndexContextBuilder
{
    private readonly ConcurrentDictionary<string, Action<IJsonIndexBuilderForContexts>> configurators = new();

    public IJsonIndexContextBuilder ByDefault(Action<IJsonIndexBuilderForContexts> configure)
    {
        configurators.AddOrUpdate("*", s => configure, (s, func) => configure);
        return this;
    }

    public IJsonIndexContextBuilder For(string group, Action<IJsonIndexBuilderForContexts> configure)
    {
        if (group == null) throw new ArgumentNullException(nameof(group));
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        if (group is "*" or "") throw new ArgumentException("Invalid name for an index.", nameof(group));
        configurators.AddOrUpdate(group, s => configure, (s, func) => configure);
        return this;
    }

    public IJsonIndexContext Build()
    {
        return new JsonIndexContext(new JsonIndexFactory(new Dictionary<string, Action<IJsonIndexBuilderForContexts>>(configurators)));
    }
}

public interface IJsonIndexFactory
{
    IJsonIndex Create(string name);
}

public class JsonIndexFactory : IJsonIndexFactory
{
    private readonly IReadOnlyDictionary<string, Action<IJsonIndexBuilderForContexts>> configurators;
    private readonly ConcurrentDictionary<string, IndexGroupConfiguration> configurations = new();

    public JsonIndexFactory(IReadOnlyDictionary<string,Action<IJsonIndexBuilderForContexts>> configurators)
    {
        this.configurators = configurators;
    }

    public IJsonIndex Create(string name)
    {
        IndexGroupConfiguration entry = configurations.GetOrAdd(name, CreateConfiguration);
        return new JsonIndex(entry.StorageProviderFactory.Create(name), entry.Configuration);
    }

    private IndexGroupConfiguration CreateConfiguration(string group)
    {
        if (!TryGetConfigurator(group, out Action<IJsonIndexBuilderForContexts> configure)) 
            throw new ArgumentException("No configurators found for the given group and no default configurators provided.", nameof(group));
        
        JsonIndexBuilderForContexts builder = new JsonIndexBuilderForContexts();
        configure(builder);
        return builder.Build();
    }

    private bool TryGetConfigurator(string group, out Action<IJsonIndexBuilderForContexts> cfg)
    {
        return configurators.TryGetValue(group, out cfg)
               || configurators.TryGetValue("*", out cfg);
    }

}
public interface IJsonIndexBuilderForContexts
{
    IJsonIndexBuilderForContexts UsingStorageProviderFactory(IStorageProviderFactory storageProviderFactory);
    IJsonIndexBuilderForContexts WithService<TService>(bool replace, Func<IJsonIndexConfiguration, TService> factory);
    IndexGroupConfiguration Build();
}

public record struct IndexGroupConfiguration(IStorageProviderFactory StorageProviderFactory, IJsonIndexConfiguration Configuration);

public class JsonIndexBuilderForContexts : IJsonIndexBuilderForContexts
{
    private IStorageProviderFactory storageProviderFactory = new RamStorageProviderFactory();
    private readonly Dictionary<Type, Func<IJsonIndexConfiguration, object>> factories = new();

    public IJsonIndexBuilderForContexts UsingStorageProviderFactory(IStorageProviderFactory storageProviderFactory)
    {
        this.storageProviderFactory = storageProviderFactory;
        return this;
    }

    public IJsonIndexBuilderForContexts WithService<TService>(bool replace, Func<IJsonIndexConfiguration, TService> factory)
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

    public IndexGroupConfiguration Build()
    {
        return new IndexGroupConfiguration(storageProviderFactory, new JsonIndexConfiguration(LuceneVersion.LUCENE_48, factories
            .Select(pair => new ServiceDescriptor(pair.Key, pair.Value))));
    }
}