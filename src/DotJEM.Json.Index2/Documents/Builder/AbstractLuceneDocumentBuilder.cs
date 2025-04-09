using System.Collections.Generic;
using DotJEM.Json.Index2.Documents.Data;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Serialization;
using DotJEM.Json.Visitor;
using DotJEM.ObservableExtensions.InfoStreams;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Documents.Builder;

public interface ILuceneDocumentBuilder
{
    IInfoStream EventInfoStream { get; }
    IIndexableJsonDocument Build(JObject json);
}

public abstract class AbstractLuceneDocumentBuilder : JValueVisitor<IPathContext>, ILuceneDocumentBuilder
{
    private readonly IJsonDocumentSerializer documentSerializer;
    private readonly IInfoStream<AbstractLuceneDocumentBuilder> eventInfoStream;

    private IIndexableJsonDocument document;

    public IInfoStream EventInfoStream => eventInfoStream;

    protected IFieldResolver Resolver { get; }

    protected AbstractLuceneDocumentBuilder(IFieldResolver resolver = null, IJsonDocumentSerializer documentSerializer = null)
    {
        this.Resolver = resolver ?? new FieldResolver();
        this.eventInfoStream = new InfoStream<AbstractLuceneDocumentBuilder>();
        this.documentSerializer = documentSerializer ?? new DefaultJsonDocumentSerialier();
    }

    public virtual IIndexableJsonDocument Build(JObject json)
    {
        document = new IndexableJsonDocument(Resolver.ContentType(json), Resolver.Identity(json));
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
        
    protected virtual void Add(IIndexableJsonField field) 
        => document.Add(field);

    protected virtual void Add(IEnumerable<IIndexableJsonField> fields)
    {
        foreach (IIndexableJsonField field in fields)
            Add(field);
    }

    private class PathContext : IPathContext
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