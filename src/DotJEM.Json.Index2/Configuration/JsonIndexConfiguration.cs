using System.Collections.Generic;
using System.Linq;
using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.Serialization;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
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

    public JsonIndexConfiguration() 
    {
        Services = new ServiceCollection(this, Enumerable.Empty<ServiceDescriptor>());
        Version = LuceneVersion.LUCENE_48;
        Analyzer = new StandardAnalyzer(Version, CharArraySet.EMPTY_SET);
        FieldResolver = new FieldResolver();
        FieldInformationManager = new DefaultFieldInformationManager(FieldResolver);
        DocumentFactory = new LuceneDocumentFactory(FieldInformationManager);
        Serializer = new GZipJsonDocumentSerialier();
    }

    internal JsonIndexConfiguration(LuceneVersion version, IEnumerable<ServiceDescriptor> services)
    {
        Services = new ServiceCollection(this, services);
        Version = version;
        Analyzer = Services.Get<Analyzer>() ?? new StandardAnalyzer(Version, CharArraySet.EMPTY_SET);
        FieldResolver = Services.Get<IFieldResolver>() ?? new FieldResolver();
        FieldInformationManager = Services.Get<IFieldInformationManager>() ?? new DefaultFieldInformationManager(FieldResolver);
        DocumentFactory = Services.Get<ILuceneDocumentFactory>() ?? new LuceneDocumentFactory(FieldInformationManager);
        Serializer = Services.Get<IJsonDocumentSerializer>() ?? new GZipJsonDocumentSerialier();
    }

}