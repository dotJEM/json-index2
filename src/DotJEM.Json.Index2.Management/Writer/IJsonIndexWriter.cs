using System.Runtime.CompilerServices;
using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.ObservableExtensions.InfoStreams;
using Lucene.Net.Index;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Management.Writer;

public interface IJsonIndexWriter
{
    IInfoStream InfoStream { get; }
    void Create(JObject entity);
    void Update(JObject entity);
    void Delete(JObject entity);
    void Commit();
    void Flush(bool triggerMerge, bool applyAllDeletes);
    void MaybeMerge();
}

public class JsonIndexWriter : IJsonIndexWriter
{
    private readonly IJsonIndex index;
    private readonly ILuceneDocumentFactory mapper;
    private readonly IFieldInformationManager resolver;
    private readonly IInfoStream<JsonIndexManager> infoStream = new InfoStream<JsonIndexManager>();

    private IndexWriter Writer => index.WriterManager.Writer;

    public IInfoStream InfoStream => infoStream;

    public JsonIndexWriter(IJsonIndex index)
    {
        this.index = index;
        this.mapper = index.Configuration.DocumentFactory;
        this.resolver = index.Configuration.FieldInformationManager;
    }

    public void Update(JObject entity)
    {
        Term term = resolver.Resolver.Identity(entity);
        LuceneDocumentEntry doc = mapper.Create(entity);
        Writer.UpdateDocument(term, doc.Document);
        DebugInfo($"Writer.UpdateDocument({term}, <doc>)");
    }

    public void Create(JObject entity)
    {
        LuceneDocumentEntry doc = mapper.Create(entity);
        Writer.AddDocument(doc.Document);
        DebugInfo($"Writer.AddDocument(<doc>)");
    }

    public void Delete(JObject entity)
    {
        Term term = resolver.Resolver.Identity(entity);
        Writer.DeleteDocuments(term);
        DebugInfo($"Writer.UpdateDocuments({term})");
    }


    public void Commit()
    {
        Writer.Commit();
        DebugInfo($"Writer.Commit()");
    }

    public void Flush(bool triggerMerge, bool applyAllDeletes)
    {
        Writer.Flush(triggerMerge, applyAllDeletes);
        DebugInfo($"Writer.Flush({triggerMerge}, {applyAllDeletes})");
    }

    public void MaybeMerge()
    {
        Writer.MaybeMerge();
        DebugInfo($"Writer.MaybeMerge()");
    }

    private void DebugInfo(string message, [CallerMemberName] string caller = null)
        => infoStream.WriteDebug(message, caller);
}