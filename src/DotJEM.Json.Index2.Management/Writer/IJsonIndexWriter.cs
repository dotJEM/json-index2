using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.ObservableExtensions.InfoStreams;
using DotJEM.Web.Scheduler;
using Lucene.Net.Index;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Management.Writer;

public interface IJsonIndexWriter 
{
    IInfoStream InfoStream { get; }
    void Write(JObject entity);
    void Create(JObject entity);
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

    public void Write(JObject entity)
    {
        Term term = resolver.Resolver.Identity(entity);
        LuceneDocumentEntry doc = mapper.Create(entity);
        Writer.UpdateDocument(term, doc.Document);
        DebugInfo();
    }

    public void Create(JObject entity)
    {
        LuceneDocumentEntry doc = mapper.Create(entity);
        Writer.AddDocument(doc.Document);
        DebugInfo();
    }

    public void Delete(JObject entity)
    {
        Term term = resolver.Resolver.Identity(entity);
        Writer.DeleteDocuments(term);
        DebugInfo();
    }


    public void Commit()
    {
        Writer.Commit();
        DebugInfo();
    }
    public void Flush(bool triggerMerge, bool applyAllDeletes)
    {
        Writer.Flush(triggerMerge, applyAllDeletes);
        DebugInfo();
    }

    public void MaybeMerge()
    {
        Writer.MaybeMerge();
        DebugInfo();
    }

    private void DebugInfo([CallerMemberName] string caller = null) => infoStream.WriteDebug($"{nameof(JsonIndexWriter)}.{caller} called.");
    }

