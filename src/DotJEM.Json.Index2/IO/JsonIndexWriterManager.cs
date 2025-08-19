using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DotJEM.Json.Index2.Leases;
using DotJEM.Json.Index2.Util;
using Lucene.Net.Index;
using Lucene.Net.Util;

namespace DotJEM.Json.Index2.IO;


public interface IIndexWriterManager : IDisposable
{
    ILease<IndexWriter> Lease();
    void Close();
}


/// <summary>
/// 
/// </summary>
public class IndexWriterManager : Disposable, IIndexWriterManager
{
    public static int DEFAULT_RAM_BUFFER_SIZE_MB { get; set; } = 512;

    private IndexWriter writer;

    private readonly IJsonIndex index;
    private readonly object writerPadLock = new();
    private readonly LeaseManager<IndexWriter> leaseManager = new();

    public IndexWriterManager(IJsonIndex index)
    {
        this.index = index;
    }

    public ILease<IndexWriter> Lease()
    {
        lock (writerPadLock)
        {
            writer ??= Open(index);
            return leaseManager.Create(writer);
        }
    }

    private static IndexWriter Open(IJsonIndex index)
    {
        IndexWriterConfig config = new(index.Configuration.Version, index.Configuration.Analyzer);
        config.RAMBufferSizeMB = DEFAULT_RAM_BUFFER_SIZE_MB;
        config.OpenMode = OpenMode.CREATE;
        config.IndexDeletionPolicy = new SnapshotDeletionPolicy(config.IndexDeletionPolicy);
        config.SetInfoStream(new RedirectInfoStream());
        return new(index.Storage.Directory, config);
    }

    public void Close()
    {
        if (writer == null)
            return;

        lock (writerPadLock)
        {
            if (writer == null)
                return;

            IndexWriter copy = writer;
            writer = null;
            
            leaseManager.RecallAll();
            copy.Dispose();
        }
    }

    protected override void Dispose(bool disposing)
    {
        Close();
        base.Dispose(disposing);
    }

}

internal class RedirectInfoStream : InfoStream
{
    public override void Message(string component, string message)
    {
    }

    public override bool IsEnabled(string component) => true;
}
