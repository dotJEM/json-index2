using System;
using System.Collections.Generic;
using System.Diagnostics;
using DotJEM.Json.Index2.Leases;
using DotJEM.Json.Index2.Util;
using Lucene.Net.Index;

namespace DotJEM.Json.Index2.IO;


public interface IIndexWriterManager : IDisposable
{
    event EventHandler<EventArgs> OnClose;

    ILease<IIndexWriter> Lease();
    void Close();
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

    //TODO: With leases, this should not be needed.
    public event EventHandler<EventArgs> OnClose;

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

                return writer = Open(index);
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
            IEnumerable<IIndexWriter> recalled = leaseManager.RecallAll();
            foreach (IIndexWriter leasedValue in recalled)
                leasedValue.Dispose();

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
