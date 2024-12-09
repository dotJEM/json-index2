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
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace DotJEM.Json.Index2.IO;

public interface IIndexWriterManager : IDisposable
{
    event EventHandler<EventArgs> OnClose;

    ILease<IIndexWriter> Lease();
    void Close();
}


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
    
    public ILease<IIndexWriter> Lease() => leaseManager.Create(Writer, TimeSpan.FromSeconds(10));

    public IndexWriterManager(IJsonIndex index)
    {
        this.index = index;
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

public interface IIndexWriter
{
    DirectoryReader GetReader(bool applyAllDeletes);
    int NumDeletedDocs(SegmentCommitInfo info);
    void Dispose();
    void Dispose(bool waitForMerges);
    bool HasDeletions();
    void AddDocument(IEnumerable<IIndexableField> doc);
    void AddDocument(IEnumerable<IIndexableField> doc, Analyzer analyzer);
    void AddDocuments(IEnumerable<IEnumerable<IIndexableField>> docs);
    void AddDocuments(IEnumerable<IEnumerable<IIndexableField>> docs, Analyzer analyzer);
    void UpdateDocuments(Term delTerm, IEnumerable<IEnumerable<IIndexableField>> docs);
    void UpdateDocuments(Term delTerm, IEnumerable<IEnumerable<IIndexableField>> docs, Analyzer analyzer);
    void DeleteDocuments(Term term);
    bool TryDeleteDocument(IndexReader readerIn, int docID);
    void DeleteDocuments(params Term[] terms);
    void DeleteDocuments(Query query);
    void DeleteDocuments(params Query[] queries);
    void UpdateDocument(Term term, IEnumerable<IIndexableField> doc);
    void UpdateDocument(Term term, IEnumerable<IIndexableField> doc, Analyzer analyzer);
    void UpdateNumericDocValue(Term term, string field, long? value);
    void UpdateBinaryDocValue(Term term, string field, BytesRef value);
    void ForceMerge(int maxNumSegments);
    void ForceMerge(int maxNumSegments, bool doWait);
    void ForceMergeDeletes(bool doWait);
    void ForceMergeDeletes();
    void MaybeMerge();
    MergePolicy.OneMerge GetNextMerge();
    bool HasPendingMerges();
    void Rollback();
    void DeleteAll();
    void WaitForMerges();
    void AddIndexes(params Directory[] dirs);
    void AddIndexes(params IndexReader[] readers);
    void PrepareCommit();
    void SetCommitData(IDictionary<string, string> commitUserData);
    void Commit();
    bool HasUncommittedChanges();
    void Flush(bool triggerMerge, bool applyAllDeletes);
    long RamSizeInBytes();
    int NumRamDocs();
    void Merge(MergePolicy.OneMerge merge);
    void MergeFinish(MergePolicy.OneMerge merge);
    string SegString();
    string SegString(IEnumerable<SegmentCommitInfo> infos);
    string SegString(SegmentCommitInfo info);
    void DeleteUnusedFiles();
    LiveIndexWriterConfig Config { get; }
    Directory Directory { get; }
    Analyzer Analyzer { get; }
    int MaxDoc { get; }
    int NumDocs { get; }
    ICollection<SegmentCommitInfo> MergingSegments { get; }
    IDictionary<string, string> CommitData { get; }
    bool KeepFullyDeletedSegments { get; set; }
    bool IsClosed { get; }
    IndexWriter UnsafeValue { get; }
}

public class IndexWriterSafeProxy(IndexWriter writer) : IIndexWriter
{
    public IndexWriter UnsafeValue => writer;

    public DirectoryReader GetReader(bool applyAllDeletes)
    {
        return writer.GetReader(applyAllDeletes);
    }

    public int NumDeletedDocs(SegmentCommitInfo info)
    {
        return writer.NumDeletedDocs(info);
    }

    public void Dispose()
    {
        writer.Dispose();
    }

    public void Dispose(bool waitForMerges)
    {
        writer.Dispose(waitForMerges);
    }

    public bool HasDeletions()
    {
        return writer.HasDeletions();
    }

    public void AddDocument(IEnumerable<IIndexableField> doc)
    {
        writer.AddDocument(doc);
    }

    public void AddDocument(IEnumerable<IIndexableField> doc, Analyzer analyzer)
    {
        writer.AddDocument(doc, analyzer);
    }

    public void AddDocuments(IEnumerable<IEnumerable<IIndexableField>> docs)
    {
        writer.AddDocuments(docs);
    }

    public void AddDocuments(IEnumerable<IEnumerable<IIndexableField>> docs, Analyzer analyzer)
    {
        writer.AddDocuments(docs, analyzer);
    }

    public void UpdateDocuments(Term delTerm, IEnumerable<IEnumerable<IIndexableField>> docs)
    {
        writer.UpdateDocuments(delTerm, docs);
    }

    public void UpdateDocuments(Term delTerm, IEnumerable<IEnumerable<IIndexableField>> docs, Analyzer analyzer)
    {
        writer.UpdateDocuments(delTerm, docs, analyzer);
    }

    public void DeleteDocuments(Term term)
    {
        writer.DeleteDocuments(term);
    }

    public bool TryDeleteDocument(IndexReader readerIn, int docID)
    {
        return writer.TryDeleteDocument(readerIn, docID);
    }

    public void DeleteDocuments(params Term[] terms)
    {
        writer.DeleteDocuments(terms);
    }

    public void DeleteDocuments(Query query)
    {
        writer.DeleteDocuments(query);
    }

    public void DeleteDocuments(params Query[] queries)
    {
        writer.DeleteDocuments(queries);
    }

    public void UpdateDocument(Term term, IEnumerable<IIndexableField> doc)
    {
        writer.UpdateDocument(term, doc);
    }

    public void UpdateDocument(Term term, IEnumerable<IIndexableField> doc, Analyzer analyzer)
    {
        writer.UpdateDocument(term, doc, analyzer);
    }

    public void UpdateNumericDocValue(Term term, string field, long? value)
    {
        writer.UpdateNumericDocValue(term, field, value);
    }

    public void UpdateBinaryDocValue(Term term, string field, BytesRef value)
    {
        writer.UpdateBinaryDocValue(term, field, value);
    }

    public void ForceMerge(int maxNumSegments)
    {
        writer.ForceMerge(maxNumSegments);
    }

    public void ForceMerge(int maxNumSegments, bool doWait)
    {
        writer.ForceMerge(maxNumSegments, doWait);
    }

    public void ForceMergeDeletes(bool doWait)
    {
        writer.ForceMergeDeletes(doWait);
    }

    public void ForceMergeDeletes()
    {
        writer.ForceMergeDeletes();
    }

    public void MaybeMerge()
    {
        writer.MaybeMerge();
    }

    public MergePolicy.OneMerge GetNextMerge()
    {
        return writer.GetNextMerge();
    }

    public bool HasPendingMerges()
    {
        return writer.HasPendingMerges();
    }

    public void Rollback()
    {
        writer.Rollback();
    }

    public void DeleteAll()
    {
        writer.DeleteAll();
    }

    public void WaitForMerges()
    {
        writer.WaitForMerges();
    }

    public void AddIndexes(params Directory[] dirs)
    {
        writer.AddIndexes(dirs);
    }

    public void AddIndexes(params IndexReader[] readers)
    {
        writer.AddIndexes(readers);
    }

    public void PrepareCommit()
    {
        writer.PrepareCommit();
    }

    public void SetCommitData(IDictionary<string, string> commitUserData)
    {
        writer.SetCommitData(commitUserData);
    }

    public void Commit()
    {
        writer.Commit();
    }

    public bool HasUncommittedChanges()
    {
        return writer.HasUncommittedChanges();
    }

    public void Flush(bool triggerMerge, bool applyAllDeletes)
    {
        writer.Flush(triggerMerge, applyAllDeletes);
    }

    public long RamSizeInBytes()
    {
        return writer.RamSizeInBytes();
    }

    public int NumRamDocs()
    {
        return writer.NumRamDocs();
    }

    public void Merge(MergePolicy.OneMerge merge)
    {
        writer.Merge(merge);
    }

    public void MergeFinish(MergePolicy.OneMerge merge)
    {
        writer.MergeFinish(merge);
    }

    public string SegString()
    {
        return writer.SegString();
    }

    public string SegString(IEnumerable<SegmentCommitInfo> infos)
    {
        return writer.SegString(infos);
    }

    public string SegString(SegmentCommitInfo info)
    {
        return writer.SegString(info);
    }

    public void DeleteUnusedFiles()
    {
        writer.DeleteUnusedFiles();
    }

    public LiveIndexWriterConfig Config => writer.Config;

    public Directory Directory => writer.Directory;

    public Analyzer Analyzer => writer.Analyzer;

    public int MaxDoc => writer.MaxDoc;

    public int NumDocs => writer.NumDocs;

    public ICollection<SegmentCommitInfo> MergingSegments => writer.MergingSegments;

    public IDictionary<string, string> CommitData => writer.CommitData;

    public bool KeepFullyDeletedSegments
    {
        get => writer.KeepFullyDeletedSegments;
        set => writer.KeepFullyDeletedSegments = value;
    }

    public bool IsClosed => writer.IsClosed;
}