using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.Serialization;
using Lucene.Net.Analysis;
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