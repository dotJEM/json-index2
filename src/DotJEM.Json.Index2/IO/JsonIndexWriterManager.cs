using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Leases;
using DotJEM.Json.Index2.Util;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace DotJEM.Json.Index2.IO;

public interface IIndexWriterManager : IDisposable
{
    event EventHandler<EventArgs> OnClose;

    ILease<IndexWriter> Lease();
    void Close();
}


public class IndexWriterManager : Disposable, IIndexWriterManager
{
    public static int DEFAULT_RAM_BUFFER_SIZE_MB { get; set; } = 512;

    private readonly IJsonIndex index;
    private volatile IndexWriter writer;
    private readonly object writerPadLock = new();
    private readonly LeaseManager<IndexWriter> leaseManager = new();

    //TODO: With leases, this should not be needed.
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


    public ILease<IndexWriter> Lease() => leaseManager.Create(Writer, TimeSpan.FromSeconds(3));

    public IndexWriterManager(IJsonIndex index)
    {
        this.index = index;
    }

    private static IndexWriter Open(IJsonIndex index)
    {
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

        lock (writerPadLock)
        {
            leaseManager.RecallAll();
            if (writer == null)
                return;

            writer.Dispose();
            writer = null;
            RaiseOnClose();
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
}