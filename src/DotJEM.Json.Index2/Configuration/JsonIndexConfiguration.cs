using System.Collections.Generic;
using DotJEM.Json.Index2.Analysis;
using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.Serialization;
using Lucene.Net.Analysis;
using Lucene.Net.Util;

namespace DotJEM.Json.Index2.Configuration;

public class JsonIndexConfiguration : IJsonIndexConfiguration
{
    public LuceneVersion Version { get; init; }
    public Analyzer Analyzer { get; init; }
    public IFieldResolver FieldResolver { get; init; }
    public IFieldInformationManager FieldInformationManager { get; init; }
    public ILuceneDocumentFactory DocumentFactory { get; init; }
    public IJsonDocumentSerializer Serializer { get; init; }

    public IServiceCollection Services { get; }


    public JsonIndexConfiguration(LuceneVersion version, IEnumerable<ServiceDescriptor> services)
    {
        Services = new ServiceCollection(this, services);
        Version = version;
        Analyzer = Services.Get<Analyzer>() ?? new JsonAnalyzer(Version);
        FieldResolver = Services.Get<IFieldResolver>() ?? new FieldResolver();
        FieldInformationManager = Services.Get<IFieldInformationManager>() ?? new DefaultFieldInformationManager(FieldResolver);
        DocumentFactory = Services.Get<ILuceneDocumentFactory>() ?? new LuceneDocumentFactory(FieldInformationManager);
        Serializer = Services.Get<IJsonDocumentSerializer>() ?? new DefaultJsonDocumentSerialier();
    }

}