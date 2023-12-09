using System.Collections.Generic;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Serialization;
using DotJEM.Json.Visitor;
using DotJEM.ObservableExtensions.InfoStreams;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Documents.Builder
{
    public interface ILuceneDocumentBuilder
    {
        IInfoStream EventInfoStream { get; }
        ILuceneDocument Build(JObject json);
    }

    public interface ILuceneDocument
    {
        Document Document { get; } 
        IContentTypeInfo Info { get; } 
        void Add(IIndexableJsonField field);
    }

    public class LuceneDocument : ILuceneDocument
    {
        public Document Document { get; } = new Document();
        
        public IContentTypeInfo Info { get; }

        public LuceneDocument(string contentType)
        {
            Info = new ContentTypeInfo(contentType);
        }

        public void Add(IIndexableJsonField field)
        {
            foreach (IIndexableField x in field.LuceneFields)
                Document.Add(x);

            Info.Add(field.Info());
        }
    }

    public abstract class AbstractLuceneDocumentBuilder : JValueVisitor<IPathContext>, ILuceneDocumentBuilder
    {
        private readonly IFieldResolver resolver;
        private readonly IJsonDocumentSerializer documentSerializer;
        private readonly IInfoStream<AbstractLuceneDocumentBuilder> eventInfoStream;

        private ILuceneDocument document;

        public IInfoStream EventInfoStream => eventInfoStream;

        protected AbstractLuceneDocumentBuilder(IFieldResolver resolver = null, IJsonDocumentSerializer documentSerializer = null)
        {
            this.resolver = resolver ?? new FieldResolver();
            this.eventInfoStream = new InfoStream<AbstractLuceneDocumentBuilder>();
            this.documentSerializer = documentSerializer ?? new DefaultJsonDocumentSerialier();
        }

        public virtual ILuceneDocument Build(JObject json)
        {
            document = new LuceneDocument(resolver.ContentType(json));
            PathContext context = new PathContext(this);
            documentSerializer.SerializeTo(json, document.Document);
            Visit(json, context);
            return document;
        }

        protected override void Visit(JArray json, IPathContext context)
        {
            int num = 0;
            foreach (JToken self in json)
                self.Accept(this, context.Next(num++));
        }

        protected override void Visit(JProperty json, IPathContext context)
            => json.Value.Accept(this, context.Next(json.Name));
        
        protected void Add(IIndexableJsonField field) 
            => document.Add(field);

        protected void Add(IEnumerable<IIndexableJsonField> fields)
        {
            foreach (IIndexableJsonField field in fields)
            {
                Add(field);
            }
        }

        public class PathContext : IPathContext
        {
            private readonly AbstractLuceneDocumentBuilder builder;

            public string Path { get; }

            public PathContext(AbstractLuceneDocumentBuilder builder, string path = "")
            {
                Path = path;
                this.builder = builder;
            }

            public IPathContext Next(int index)  => new PathContext(builder, Path);
            public IPathContext Next(string name) => new PathContext(builder, Path == "" ? name : Path + "." + name);
        }
    }
}
