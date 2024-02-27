using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.ObservableExtensions.InfoStreams;
using Lucene.Net.Index;
using Newtonsoft.Json.Linq;
using static DotJEM.Json.Index2.Management.Writer.JsonIndexWriter;

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
    private readonly ThrottledCommit throttledCommit;
    
    private IndexWriter Writer => index.WriterManager.Writer;
    public IInfoStream InfoStream => infoStream;

    public JsonIndexWriter(IJsonIndex index)
    {
        this.index = index;
        this.mapper = index.Configuration.DocumentFactory;
        this.resolver = index.Configuration.FieldInformationManager;
        throttledCommit = new ThrottledCommit(this);
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
        throttledCommit.Invoke();
        //Writer.Commit();
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
    
    public class ThrottledCommit
    {
        private readonly JsonIndexWriter target;
        private readonly WaitHandle handle = new AutoResetEvent(false);
        private readonly long upperBound = Stopwatch.Frequency * 10;
        private readonly long lowerBound = Stopwatch.Frequency / 10;

        private long lastInvocation = 0;
        private long lastRequest = 0;

        public ThrottledCommit(JsonIndexWriter target)
        {
            this.target = target;
            ThreadPool.RegisterWaitForSingleObject(handle, (_,_)=>Tick(), null, 200, false);
        }

        private void Tick()
        {
            long time = Stopwatch.GetTimestamp();
            if (time - lastInvocation > upperBound)
            {
                Commit();
                lastInvocation = time;
                return;
            }

            if (time - lastRequest > lowerBound)
            {
                Commit();
                lastInvocation = time;
            }
        }

        private void Commit()
        {
            try
            {
                target.Writer.Commit();
            }
            catch (Exception e)
            {
                // SWALLOW FOR NOW
            }
        }

        public void Invoke()
        {
            lastRequest = Stopwatch.GetTimestamp();
            Tick();
        }
    }
}