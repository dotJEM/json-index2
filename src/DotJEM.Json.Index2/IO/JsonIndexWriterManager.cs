using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Util;
using Lucene.Net.Analysis;
using Lucene.Net.Index;

namespace DotJEM.Json.Index2.IO;

public interface IIndexWriterManager : IDisposable
{
    event EventHandler<EventArgs> OnClose;
    ILease<IndexWriter> Lease();
    void Close();
}

public interface ILease<out T> : IDisposable
{
    T Value { get; }
    bool IsExpired { get; }
}

public class IndexWriterManager : Disposable, IIndexWriterManager
{
    public static int DEFAULT_RAM_BUFFER_SIZE_MB { get; set; } = 512;

    private readonly IJsonIndex index;
    private volatile IndexWriter writer;
    private readonly object writerPadLock = new();
    private readonly object leasesPadLock = new();

    public event EventHandler<EventArgs> OnClose;

    private IndexWriter Writer
    {
        get
        {
            if (writer != null)
                return writer;

            lock (writerPadLock)
            {
                if (writer != null)
                    return writer;

                return writer = Open(index);
            }
        }
    }

    private readonly List<TimeLimitedIndexWriterLease> leases = new List<TimeLimitedIndexWriterLease>();

    public ILease<IndexWriter> Lease()
    {
        //TODO: Optimizied collection for this.
        TimeLimitedIndexWriterLease lease = new(this, OnReturned);
        lock (leasesPadLock)
        {
            leases.Add(lease);
        }
        return lease;
    }

    private void OnReturned(TimeLimitedIndexWriterLease lease)
    {
        lock (leasesPadLock)
        {
            leases.Remove(lease);
        }
    }


    public IndexWriterManager(IJsonIndex index)
    {
        this.index = index;
    }

    private static IndexWriter Open(IJsonIndex index)
    {
        Debug.WriteLine("OPEN WRITER");
        IndexWriterConfig config = new(index.Configuration.Version, index.Configuration.Analyzer);
        config.RAMBufferSizeMB = DEFAULT_RAM_BUFFER_SIZE_MB;
        config.OpenMode = OpenMode.CREATE_OR_APPEND;
        config.IndexDeletionPolicy = new SnapshotDeletionPolicy(config.IndexDeletionPolicy);
        return new(index.Storage.Directory, config);
    }

    public void Close()
    {
        Debug.WriteLine($"CLOSE WRITER: {writer != null}");
        if (writer == null)
            return;

        lock (leasesPadLock)
        {
            TimeLimitedIndexWriterLease[] leasesCopy = leases.ToArray();

            Debug.WriteLine("ACTIVE LEASES: " + leasesCopy.Length);
            leases.Clear();
            foreach (TimeLimitedIndexWriterLease lease in leasesCopy)
            {
                if (!lease.IsExpired)
                    lease.Wait();
                lease.Dispose();
            }

            lock (writerPadLock)
            {
                if (writer == null)
                    return;

                writer.Dispose();
                writer = null;
                RaiseOnClose();
            }
        }
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

    private class TimeLimitedIndexWriterLease : Disposable, ILease<IndexWriter>
    {
        private readonly DateTime leaseTime = DateTime.Now;
        private readonly Action<TimeLimitedIndexWriterLease> onReturned;
        private readonly IndexWriterManager manager;
        public AutoResetEvent Handle { get; } = new(false);

        public IndexWriter Value
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException("Index writer lease has been returned or is expired.");
                }
                return manager.Writer;
            }
        }

        public bool IsExpired => DateTime.Now - leaseTime > TimeSpan.FromSeconds(5);

        public TimeLimitedIndexWriterLease(IndexWriterManager manager, Action<TimeLimitedIndexWriterLease> onReturned)
        {
            this.onReturned = onReturned;
            this.manager = manager;
        }

        protected override void Dispose(bool disposing)
        {
            Handle.Set();
            onReturned(this);
        }

        public void Wait()
        {
            if (IsDisposed)
                return;

            if (IsExpired)
                return;

            Handle.WaitOne(TimeSpan.FromSeconds(6) - (DateTime.Now - leaseTime));
        }
    }

}