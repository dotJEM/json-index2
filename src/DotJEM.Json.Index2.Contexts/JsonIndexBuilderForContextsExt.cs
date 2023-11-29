using System;
using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Contexts.Storage;
using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.Serialization;
using Lucene.Net.Analysis;

namespace DotJEM.Json.Index2.Contexts;

public static class JsonIndexBuilderForContextsExt
{
    public static IJsonIndexBuilderForContexts UsingSimpleFileStorage(this IJsonIndexBuilderForContexts self, string path)
        => self.UsingStorageProviderFactory(new SimpleFsStorageProviderFactory(path));
    public static IJsonIndexBuilderForContexts UsingMemmoryStorage(this IJsonIndexBuilderForContexts self)
        => self.UsingStorageProviderFactory(new RamStorageProviderFactory());

    public static IJsonIndexBuilderForContexts UsingAnalyzer(this IJsonIndexBuilderForContexts self, Func<IJsonIndexConfiguration, Analyzer> analyzerProvider)
        => self.WithService(analyzerProvider);
    public static IJsonIndexBuilderForContexts UsingFieldResolver(this IJsonIndexBuilderForContexts self, IFieldResolver resolver)
        => self.WithService(resolver);
    public static IJsonIndexBuilderForContexts UsingFieldInformationManager(this IJsonIndexBuilderForContexts self, Func<IJsonIndexConfiguration, IFieldInformationManager> managerProvider)
        => self.WithService(managerProvider);
    public static IJsonIndexBuilderForContexts UsingDocumentFactory(this IJsonIndexBuilderForContexts self, Func<IJsonIndexConfiguration, ILuceneDocumentFactory> factoryProvider)
        => self.WithService(factoryProvider);
    public static IJsonIndexBuilderForContexts UsingSerializer(this IJsonIndexBuilderForContexts self, IJsonDocumentSerializer serializer)
        => self.WithService(serializer);
    
    public static IJsonIndexBuilderForContexts WithService<TService>(this IJsonIndexBuilderForContexts self, TService impl)
        => self.WithService(_ => impl);
    public static IJsonIndexBuilderForContexts WithService<TService>(this IJsonIndexBuilderForContexts self, Func<IJsonIndexConfiguration, TService> factory)
        => self.WithService(true, factory);
    public static IJsonIndexBuilderForContexts TryWithService<TService>(this IJsonIndexBuilderForContexts self, TService impl)
        => self.TryWithService(_ => impl);
    public static IJsonIndexBuilderForContexts TryWithService<TService>(this IJsonIndexBuilderForContexts self, Func<IJsonIndexConfiguration, TService> factory)
        => self.WithService(false, factory);
}