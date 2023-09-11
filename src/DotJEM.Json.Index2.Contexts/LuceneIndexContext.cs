using System;
using System.Collections.Concurrent;
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
    private readonly ILuceneJsonIndexFactory factory;
    private readonly ConcurrentDictionary<string, IJsonIndex> indices = new ConcurrentDictionary<string, IJsonIndex>();

    //public IServiceResolver Services { get; }
    //public JsonIndexContext(IServiceCollection services = null)
    //    : this(new LuceneIndexContextBuilder(), services) { }

    //public JsonIndexContext(string path, IServiceCollection services = null)
    //    : this(new LuceneIndexContextBuilder(path), services) { }

    public JsonIndexContext(ILuceneJsonIndexFactory factory)
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
  
public interface ILuceneJsonIndexFactory
{
    IJsonIndex Create(string name);
}
