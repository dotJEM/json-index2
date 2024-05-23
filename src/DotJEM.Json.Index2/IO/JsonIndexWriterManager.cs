using System;
using System.Diagnostics;
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

    private volatile IndexWriter writer;
    private readonly IJsonIndex index;
    private readonly object padlock = new();

    public event EventHandler<EventArgs> OnClose;

    public IndexWriter Writer
    {
        get
        {
            if (writer != null)
                return writer;

            lock (padlock)
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

        lock (padlock)
        {
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