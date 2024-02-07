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
    private readonly IndexCommitter committer;
    private readonly IInfoStream<JsonIndexManager> infoStream = new InfoStream<JsonIndexManager>();

    private IndexWriter Writer => index.WriterManager.Writer;

    public IInfoStream InfoStream => infoStream;

    public JsonIndexWriter(IJsonIndex index, IWebTaskScheduler scheduler, string commitInterval = "10s", int batchSize = 20000, double ramBufferSize = 1024)
    {
        this.index = index;
        this.mapper = index.Configuration.DocumentFactory;
        this.resolver = index.Configuration.FieldInformationManager;
        this.committer = new IndexCommitter(this, AdvParsers.AdvParser.ParseTimeSpan(commitInterval), batchSize);
        scheduler.Schedule(nameof(IndexCommitter), _ => committer.Increment(), commitInterval);
    }

    public void Write(JObject entity)
    {
        Term term = resolver.Resolver.Identity(entity);
        LuceneDocumentEntry doc = mapper.Create(entity);
        Writer.UpdateDocument(term, doc.Document);
        committer.Increment();
        DebugInfo();
    }

    public void Create(JObject entity)
    {
        LuceneDocumentEntry doc = mapper.Create(entity);
        Writer.AddDocument(doc.Document);
        committer.Increment();
        DebugInfo();
    }

    public void Delete(JObject entity)
    {
        Term term = resolver.Resolver.Identity(entity);
        Writer.DeleteDocuments(term);
        committer.Increment();
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


    private class IndexCommitter
    {
        private readonly int batchSize;
        private readonly TimeSpan commitInterval;
        private readonly IJsonIndexWriter owner;

        private long writes = 0;
        private readonly Stopwatch time = Stopwatch.StartNew();

        public IndexCommitter(IJsonIndexWriter owner, TimeSpan commitInterval, int batchSize)
        {
            this.owner = owner;
            this.commitInterval = commitInterval;
            this.batchSize = batchSize;
        }

        public void Increment()
        {
            long value  = Interlocked.Increment(ref writes);
            if(value % batchSize == 0 || time.Elapsed > commitInterval)
                Commit();
        }

        private void Commit()
        {
            owner.Commit();
            time.Restart();
        }
    }
}

