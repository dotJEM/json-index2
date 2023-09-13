using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Contexts.Searching;
using DotJEM.Json.Index2.Contexts.Storage;
using DotJEM.Json.Index2.Searching;

namespace DotJEM.Json.Index2.Contexts;

public interface IJsonIndexContext : IJsonIndexSearcherProvider
{
    IJsonIndex Open(string name);
}

public class JsonIndexContext : IJsonIndexContext
{
    private readonly IJsonIndexFactory factory;
    private readonly ConcurrentDictionary<string, IJsonIndex> indices = new ConcurrentDictionary<string, IJsonIndex>();

    //public IServiceResolver Services { get; }
    //public JsonIndexContext(IServiceCollection services = null)
    //    : this(new LuceneIndexContextBuilder(), services) { }

    //public JsonIndexContext(string path, IServiceCollection services = null)
    //    : this(new LuceneIndexContextBuilder(path), services) { }

    public JsonIndexContext(IJsonIndexFactory factory)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        //this.Services = resolver ?? throw new ArgumentNullException(nameof(resolver));
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

public interface IJsonIndexContextBuilder
{
    IJsonIndexContextBuilder ByDefault(Func<IJsonIndexBuilder, IJsonIndex> defaultConfig);
    IJsonIndexContextBuilder For(string name, Func<IJsonIndexBuilder, IJsonIndex> defaultConfig);
    IJsonIndexContext Build();
}

public class JsonIndexContextBuilder : IJsonIndexContextBuilder
{
    private readonly ConcurrentDictionary<string, Func<IJsonIndexBuilder, IJsonIndex>> configurators = new();
    public IJsonIndexContextBuilder ByDefault(Func<IJsonIndexBuilder, IJsonIndex> defaultConfig)
    {
        configurators.AddOrUpdate("*", s => defaultConfig, (s, func) => defaultConfig);
        return this;
    }

    public IJsonIndexContextBuilder For(string name, Func<IJsonIndexBuilder, IJsonIndex> defaultConfig)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (defaultConfig == null) throw new ArgumentNullException(nameof(defaultConfig));
        if (name is "*" or "") throw new ArgumentException("Invalid name for an index.", nameof(name));

        configurators.AddOrUpdate(name, s => defaultConfig, (s, func) => defaultConfig);
        return this;
    }

    public IJsonIndexContext Build()
    {
        return new JsonIndexContext(new JsonIndexFactory(new Dictionary<string, Func<IJsonIndexBuilder, IJsonIndex>>(configurators)));
    }
}

public interface IJsonIndexFactory
{
    IJsonIndex Create(string name);
}

public class JsonIndexFactory : IJsonIndexFactory
{
    private readonly IReadOnlyDictionary<string, Func<IJsonIndexBuilder, IJsonIndex>> configurators;

    public JsonIndexFactory(IReadOnlyDictionary<string, Func<IJsonIndexBuilder, IJsonIndex>> configurators)
    {
        this.configurators = configurators;
    }

    public IJsonIndex Create(string name)
    {
        if (configurators.TryGetValue(name, out Func<IJsonIndexBuilder, IJsonIndex> func))
            return func(new JsonIndexBuilder(name));

        if(configurators.TryGetValue("*", out func))
            return func(new JsonIndexBuilder(name));

        throw new InvalidOperationException("");
    }
}