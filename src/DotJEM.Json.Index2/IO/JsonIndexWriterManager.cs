using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DotJEM.Json.Index2.Leases;
using DotJEM.Json.Index2.Util;
using Lucene.Net.Index;

namespace DotJEM.Json.Index2.IO;


public interface IIndexWriterManager : IDisposable
{
    event EventHandler<EventArgs> OnClose;
    ILease<IIndexWriter> Lease();
    void Close();

    void Lock();
    void Unlock();
}


/// <summary>
/// 
/// </summary>
public class IndexWriterManager : Disposable, IIndexWriterManager
{
    public static int DEFAULT_RAM_BUFFER_SIZE_MB { get; set; } = 512;

    private readonly IJsonIndex index;
    private volatile IIndexWriter writer;
    private readonly object writerPadLock = new();
    private readonly LeaseManager<IIndexWriter> leaseManager = new();

    private readonly ManualResetEventSlim reset = new(true);
    //TODO: With leases, this should not be needed.
    public event EventHandler<EventArgs> OnClose;

    private static List<WeakReference<IIndexWriter>> writers = new();

    public void Lock()
    {
        reset.Reset();
    }
    public void Unlock()
    {
        reset.Set();
    }

    private IIndexWriter Writer
    {
        get
        {
            if (writer != null)
                return writer;

            lock (writerPadLock)
            {
                if (writer != null)
                    return writer;

                reset.Wait();

                IIndexWriter newWriter = Open(index);
                writers.Add(new(newWriter));
                return writer = newWriter;
            }
        }
    }

    public IndexWriterManager(IJsonIndex index)
    {
        this.index = index;
    }
    public ILease<IIndexWriter> Lease()
    {
        lock (writerPadLock)
        {
            return leaseManager.Create(Writer, TimeSpan.FromSeconds(10));
        }
    }

    private static IIndexWriter Open(IJsonIndex index)
    {
        IndexWriterConfig config = new(index.Configuration.Version, index.Configuration.Analyzer);
        config.RAMBufferSizeMB = DEFAULT_RAM_BUFFER_SIZE_MB;
        config.OpenMode = OpenMode.CREATE_OR_APPEND;
        config.IndexDeletionPolicy = new SnapshotDeletionPolicy(config.IndexDeletionPolicy);
        return new IndexWriterSafeProxy(new(index.Storage.Directory, config));
    }

    public void Close()
    {
        Debug.WriteLine($"CLOSE WRITER: {writer != null}");
        if (writer == null)
            return;

        lock (writerPadLock)
        {
            if (writer == null)
                return;

            IIndexWriter copy = writer;
            writer = null;
            leaseManager.RecallAll();
            copy.Dispose();
            RaiseOnClose();
        }

        int writersOpenend = writers.Count;
        int writersAlive = writers.Count(w => w.TryGetTarget(out _));
        Debug.WriteLine($"Number of opened writers: {writersOpenend} where {writersAlive} are still alive");
    }

    protected override void Dispose(bool disposing)
    {
        Debug.WriteLine($"DISPOSE WRITER: {disposing}");
        if (disposing)
            Close();
        base.Dispose(disposing);
    }

    protected virtual void RaiseOnClose()
    {
        OnClose?.Invoke(this, EventArgs.Empty);
    }
}
