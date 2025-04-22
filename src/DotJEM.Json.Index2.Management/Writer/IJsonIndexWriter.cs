using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Leases;
using DotJEM.ObservableExtensions.InfoStreams;
using Lucene.Net.Index;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Management.Writer;

public interface IJsonIndexWriter
{
    IInfoStream InfoStream { get; }
    void Create(JObject entity);
    void Create(IEnumerable<JObject> entities);
    void Update(JObject entity);
    void Delete(JObject entity);
    void Commit(bool force = false);
    void Flush(bool triggerMerge = false, bool applyAllDeletes = false);
    void MaybeMerge();
}

public class JsonIndexWriter : IJsonIndexWriter
{
    private readonly IJsonIndex index;
    private readonly ILuceneDocumentFactory mapper;
    private readonly IFieldInformationManager information;
    private readonly IInfoStream<JsonIndexManager> infoStream = new InfoStream<JsonIndexManager>();
    private readonly ThrottledCommit throttledCommit;
    
    private ILease<IIndexWriter> WriterLease => index.WriterManager.Lease();

    public IInfoStream InfoStream => infoStream;

    public JsonIndexWriter(IJsonIndex index)
    {
        this.index = index;
        this.mapper = index.Configuration.DocumentFactory;
        this.information = index.Configuration.FieldInformationManager;
        throttledCommit = new ThrottledCommit(this);
    }

    public void Update(JObject entity)
    {
        using ILease<IIndexWriter> lease = WriterLease;
        foreach (LuceneDocumentEntry doc in mapper.Create(entity))
        {
            lease.Value.UpdateDocument(doc.Key, doc.Document);
            DebugInfo($"Writer.UpdateDocument({doc.Key}, <doc>)");
        }
        throttledCommit.Increment();
    }

    public void Create(JObject entity)
    {
        using ILease<IIndexWriter> lease = WriterLease;
        lease.Value.AddDocuments(mapper.Create(entity).Select(entry => entry.Document));
        throttledCommit.Increment();
        DebugInfo($"Writer.AddDocument(<doc>)");
    }

    public void Create(IEnumerable<JObject> entities)
    {
        using ILease<IIndexWriter> lease = WriterLease;
        lease.Value.AddDocuments(entities.SelectMany(mapper.Create).Select(entry => entry.Document));
        throttledCommit.Increment();
        DebugInfo($"Writer.AddDocuments(<doc>)");
    }

    public void Delete(JObject entity)
    {
        using ILease<IIndexWriter> lease = WriterLease;
        Term term = information.Resolver.Identity(entity);
        lease.Value.DeleteDocuments(term);
        throttledCommit.Increment();
        DebugInfo($"Writer.UpdateDocuments({term})");
    }


    public void Commit(bool force = false)
    {
        throttledCommit.Invoke(force);
        DebugInfo($"Writer.Commit()");
    }

    public void Flush(bool triggerMerge = false, bool applyAllDeletes = false)
    {
        using ILease<IIndexWriter> lease = WriterLease;
        lease.Value.Flush(triggerMerge, applyAllDeletes);
        DebugInfo($"Writer.Flush({triggerMerge}, {applyAllDeletes})");
    }

    public void MaybeMerge()
    {
        using ILease<IIndexWriter> lease = WriterLease;
        lease.Value.MaybeMerge();
        DebugInfo($"Writer.MaybeMerge()");
    }

    private void DebugInfo(string message, [CallerMemberName] string caller = null)
        => infoStream.WriteDebug(message, caller);
    
    public class ThrottledCommit
    {
        private readonly JsonIndexWriter target;
        private readonly WaitHandle handle = new AutoResetEvent(false);
        private readonly long upperBound = Stopwatch.Frequency * 10;
        private readonly long lowerBound = Stopwatch.Frequency / 5;

        private long lastInvocation = 0;
        private long lastRequest = 0;
        private long writes = 0;
        private long calls = 0;

        public ThrottledCommit(JsonIndexWriter target)
        {
            this.target = target;
            ThreadPool.RegisterWaitForSingleObject(handle, (_,_)=>Tick(false), null, 200, false);
        }

        private void Tick(bool force)
        {
            long time = Stopwatch.GetTimestamp();
            // ReSharper disable once InvertIf
            if (force
                || time - lastInvocation > upperBound
                || time - lastRequest > lowerBound
               )
            {
                Commit();
                lastInvocation = time;
            }
        }

        private void Commit()
        {
            long writesRead = Interlocked.Exchange(ref writes, 0);
            if (writesRead < 1)
                return;

            long callsRead = Interlocked.Exchange(ref calls, 0);
            if (callsRead < 1)
                return;

            using ILease<IIndexWriter> lease = target.WriterLease;
            long start = Stopwatch.GetTimestamp();
            try
            {
                lease.Value.Commit();
            }
            catch (LeaseTerminatedException e)
            {
                long elapsed = Stopwatch.GetTimestamp() - start;
                target.infoStream.WriteError($"[{elapsed / (Stopwatch.Frequency / (double)1000)}ms] Failed to commit indexed data to storage. Lease has been terminated.", e);
            }
            catch (LeaseExpiredException e)
            {
                long elapsed = Stopwatch.GetTimestamp() - start;
                target.infoStream.WriteError($"[{elapsed / (Stopwatch.Frequency / (double)1000)}ms] Failed to commit indexed data to storage. Lease was expired.", e);
            }
            catch (LeaseDisposedException e)
            {
                long elapsed = Stopwatch.GetTimestamp() - start;
                target.infoStream.WriteError($"[{elapsed / (Stopwatch.Frequency / (double)1000)}ms] Failed to commit indexed data to storage. Lease was disposed.", e);
            }
            catch (Exception e)
            {
                target.infoStream.WriteError(e.Message, e);
            }
        }

        public void Invoke(bool force)
        {
            Interlocked.Increment(ref calls);
            lastRequest = Stopwatch.GetTimestamp();
            if (force)
            {
                Tick(force);
            }
        }

        public void Increment()
        {
            Interlocked.Increment(ref writes);
        }
    }
}
public class ThrottledAction
{
    private readonly Action target;
    private readonly Action<Exception> onException;
    private readonly WaitHandle handle = new AutoResetEvent(false);
    private readonly long upperBound = Stopwatch.Frequency * 10;
    private readonly long lowerBound = Stopwatch.Frequency / 5;

    private long lastInvocation = 0;
    private long lastRequest = 0;
    private long writes = 0;

    public ThrottledAction(Action target, Action<Exception> onException)
    {
        this.target = target;
        this.onException = onException;
        ThreadPool.RegisterWaitForSingleObject(handle, (_, _) => Tick(), null, 200, false);
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
        if (Interlocked.Exchange(ref writes, 0) < 1)
            return;

        try
        {
            target();
        }
        catch (Exception e)
        {
            onException(e);
        }
    }

    public void Invoke()
    {
        lastRequest = Stopwatch.GetTimestamp();
    }

    public void Increment()
    {
        Interlocked.Increment(ref writes);
    }
}