using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.Serialization;
using Lucene.Net.Analysis;
using Lucene.Net.Util;

namespace DotJEM.Json.Index2.Contexts.Configuration
{
    public class JsonContextIndexConfiguration : IJsonIndexConfiguration
    {
        public LuceneVersion Version { get; }
        public Analyzer Analyzer { get; }
        public IFieldResolver FieldResolver { get; }
        public IFieldInformationManager FieldInformationManager { get; }
        public ILuceneDocumentFactory DocumentFactory { get; }
        public IJsonDocumentSerializer Serializer { get; }
        public IServiceCollection Services { get; }

        public JsonContextIndexConfiguration(IJsonIndexConfiguration global)
        {
        }
    }
}