using System;
using System.Collections.Generic;
using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.Serialization;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;

namespace DotJEM.Json.Index2.Configuration;

public interface IJsonIndexConfiguration
{
    LuceneVersion Version { get; }
    Analyzer Analyzer { get; }
    IFieldResolver FieldResolver { get; }
    IFieldInformationManager FieldInformationManager { get;  }
    ILuceneDocumentFactory DocumentFactory { get;  }
    IJsonDocumentSerializer Serializer { get;  }
    IServiceCollection Services { get; }
}

public class JsonIndexConfiguration : IJsonIndexConfiguration
{
    public LuceneVersion Version { get; init; }
    public Analyzer Analyzer { get; init; }
    public IFieldResolver FieldResolver { get; init; }
    public IFieldInformationManager FieldInformationManager { get; init; }
    public ILuceneDocumentFactory DocumentFactory { get; init; }
    public IJsonDocumentSerializer Serializer { get; init; }
    public IServiceCollection Services { get; }

    public JsonIndexConfiguration()
    {
        Services = new ServiceCollection();
        
        Version = LuceneVersion.LUCENE_48;
        Analyzer = new StandardAnalyzer(Version, CharArraySet.EMPTY_SET);
        FieldResolver = new FieldResolver();
        FieldInformationManager = new DefaultFieldInformationManager(FieldResolver);
        DocumentFactory = new LuceneDocumentFactory(FieldInformationManager);
        Serializer = new GZipJsonDocumentSerialier();
    }
}

public static class JsonIndexConfigurationExt
{
    public static TService Get<TService>(this IJsonIndexConfiguration self) => self.Services.Get<TService>();
    public static bool TryGet<TService>(this IJsonIndexConfiguration self, out TService service) => self.Services.TryGet(out service);
}

public interface IServiceCollection
{
    IServiceCollection Register<TService>(TService impl);
    IServiceCollection Register<TService>(Func<IServiceCollection, TService> factory);
    IServiceCollection RegisterOrReplace<TService>(TService impl);
    IServiceCollection RegisterOrReplace<TService>(Func<IServiceCollection, TService> factory);

    bool TryGet<TService>(out TService value);
    TService Get<TService>();
}

public class ServiceCollection : IServiceCollection
{
    private readonly Dictionary<Type, Lazy<object>> factories = new ();

    public IServiceCollection Register<TService>(TService impl)
        => Register(_ => impl);

    public IServiceCollection Register<TService>(Func<IServiceCollection, TService> factory)
    {
        factories.Add(typeof(TService), new Lazy<object>(()=> factory(this)));
        return this;
    }

    public IServiceCollection RegisterOrReplace<TService>(TService impl)
        => RegisterOrReplace(_ => impl);

    public IServiceCollection RegisterOrReplace<TService>(Func<IServiceCollection, TService> factory)
    {
        factories[typeof(TService)] = new Lazy<object>(()=> factory(this));
        return this;
    }

    public bool TryGet<TService>(out TService value)
    {
        if (factories.TryGetValue(typeof(TService), out Lazy<object> val))
        {
            value= (TService)val.Value;
            return true;
        }

        value = default;
        return false;
    }

    public TService Get<TService>()
    {
        return TryGet(out TService service)
            ? service 
            : default;
    }
}