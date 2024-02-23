using System;
using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Util;
using Lucene.Net.Analysis;
using Lucene.Net.Index;

namespace DotJEM.Json.Index2.IO;

public interface IIndexWriterManager : IDisposable
{
    event EventHandler<EventArgs> OnClose;

    IndexWriter Writer { get; }
    void Close();
}

public class IndexWriterManager : Disposable, IIndexWriterManager
{
    public static int DEFAULT_RAM_BUFFER_SIZE_MB { get; set; } = 512;

    public event EventHandler<EventArgs> OnClose;

    private readonly ResetableLazy<IndexWriter> writer;

    public IndexWriter Writer => writer.Value;

    public IndexWriterManager(IJsonIndex index)
    {
        writer = new(() => Open(index));
    }

    protected virtual IndexWriter Open(IJsonIndex index)
    {
        IndexWriterConfig config = new(index.Configuration.Version, index.Configuration.Analyzer);
        config.RAMBufferSizeMB = DEFAULT_RAM_BUFFER_SIZE_MB;
        config.OpenMode = OpenMode.CREATE_OR_APPEND;
        config.IndexDeletionPolicy = new SnapshotDeletionPolicy(config.IndexDeletionPolicy);
        return new(index.Storage.Directory, config);
    }

    public void Close()
    {
        if (!writer.IsValueCreated)
            return;
            
        writer.Value.Dispose();
        writer.Reset();
        RaiseOnClose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Close();
        base.Dispose(disposing);
    }

    protected virtual void RaiseOnClose()
    {
        OnClose?.Invoke(this, EventArgs.Empty);
    }
}