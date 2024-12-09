using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace DotJEM.Json.Index2.IO;

public class IndexWriterSafeProxy : IIndexWriter
{
    private readonly IndexWriter inner;

    public IndexWriterSafeProxy(IndexWriter writer)
    {
        inner = writer;
    }

    public IndexWriter UnsafeValue => inner;

    public DirectoryReader GetReader(bool applyAllDeletes)
    {
        return inner.GetReader(applyAllDeletes);
    }

    public int NumDeletedDocs(SegmentCommitInfo info)
    {
        return inner.NumDeletedDocs(info);
    }

    public void Dispose()
    {
        inner.Dispose();
    }

    public void Dispose(bool waitForMerges)
    {
        inner.Dispose(waitForMerges);
    }

    public bool HasDeletions()
    {
        return inner.HasDeletions();
    }

    public void AddDocument(IEnumerable<IIndexableField> doc)
    {
        inner.AddDocument(doc);
    }

    public void AddDocument(IEnumerable<IIndexableField> doc, Analyzer analyzer)
    {
        inner.AddDocument(doc, analyzer);
    }

    public void AddDocuments(IEnumerable<IEnumerable<IIndexableField>> docs)
    {
        inner.AddDocuments(docs);
    }

    public void AddDocuments(IEnumerable<IEnumerable<IIndexableField>> docs, Analyzer analyzer)
    {
        inner.AddDocuments(docs, analyzer);
    }

    public void UpdateDocuments(Term delTerm, IEnumerable<IEnumerable<IIndexableField>> docs)
    {
        inner.UpdateDocuments(delTerm, docs);
    }

    public void UpdateDocuments(Term delTerm, IEnumerable<IEnumerable<IIndexableField>> docs, Analyzer analyzer)
    {
        inner.UpdateDocuments(delTerm, docs, analyzer);
    }

    public void DeleteDocuments(Term term)
    {
        inner.DeleteDocuments(term);
    }

    public bool TryDeleteDocument(IndexReader readerIn, int docID)
    {
        return inner.TryDeleteDocument(readerIn, docID);
    }

    public void DeleteDocuments(params Term[] terms)
    {
        inner.DeleteDocuments(terms);
    }

    public void DeleteDocuments(Query query)
    {
        inner.DeleteDocuments(query);
    }

    public void DeleteDocuments(params Query[] queries)
    {
        inner.DeleteDocuments(queries);
    }

    public void UpdateDocument(Term term, IEnumerable<IIndexableField> doc)
    {
        inner.UpdateDocument(term, doc);
    }

    public void UpdateDocument(Term term, IEnumerable<IIndexableField> doc, Analyzer analyzer)
    {
        inner.UpdateDocument(term, doc, analyzer);
    }

    public void UpdateNumericDocValue(Term term, string field, long? value)
    {
        inner.UpdateNumericDocValue(term, field, value);
    }

    public void UpdateBinaryDocValue(Term term, string field, BytesRef value)
    {
        inner.UpdateBinaryDocValue(term, field, value);
    }

    public void ForceMerge(int maxNumSegments)
    {
        inner.ForceMerge(maxNumSegments);
    }

    public void ForceMerge(int maxNumSegments, bool doWait)
    {
        inner.ForceMerge(maxNumSegments, doWait);
    }

    public void ForceMergeDeletes(bool doWait)
    {
        inner.ForceMergeDeletes(doWait);
    }

    public void ForceMergeDeletes()
    {
        inner.ForceMergeDeletes();
    }

    public void MaybeMerge()
    {
        inner.MaybeMerge();
    }

    public MergePolicy.OneMerge GetNextMerge()
    {
        return inner.GetNextMerge();
    }

    public bool HasPendingMerges()
    {
        return inner.HasPendingMerges();
    }

    public void Rollback()
    {
        inner.Rollback();
    }

    public void DeleteAll()
    {
        inner.DeleteAll();
    }

    public void WaitForMerges()
    {
        inner.WaitForMerges();
    }

    public void AddIndexes(params Directory[] dirs)
    {
        inner.AddIndexes(dirs);
    }

    public void AddIndexes(params IndexReader[] readers)
    {
        inner.AddIndexes(readers);
    }

    public void PrepareCommit()
    {
        inner.PrepareCommit();
    }

    public void SetCommitData(IDictionary<string, string> commitUserData)
    {
        inner.SetCommitData(commitUserData);
    }

    public void Commit()
    {
        inner.Commit();
    }

    public bool HasUncommittedChanges()
    {
        return inner.HasUncommittedChanges();
    }

    public void Flush(bool triggerMerge, bool applyAllDeletes)
    {
        inner.Flush(triggerMerge, applyAllDeletes);
    }

    public long RamSizeInBytes()
    {
        return inner.RamSizeInBytes();
    }

    public int NumRamDocs()
    {
        return inner.NumRamDocs();
    }

    public void Merge(MergePolicy.OneMerge merge)
    {
        inner.Merge(merge);
    }

    public void MergeFinish(MergePolicy.OneMerge merge)
    {
        inner.MergeFinish(merge);
    }

    public string SegString()
    {
        return inner.SegString();
    }

    public string SegString(IEnumerable<SegmentCommitInfo> infos)
    {
        return inner.SegString(infos);
    }

    public string SegString(SegmentCommitInfo info)
    {
        return inner.SegString(info);
    }

    public void DeleteUnusedFiles()
    {
        inner.DeleteUnusedFiles();
    }

    public LiveIndexWriterConfig Config => inner.Config;

    public Directory Directory => inner.Directory;

    public Analyzer Analyzer => inner.Analyzer;

    public int MaxDoc => inner.MaxDoc;

    public int NumDocs => inner.NumDocs;

    public ICollection<SegmentCommitInfo> MergingSegments => inner.MergingSegments;

    public IDictionary<string, string> CommitData => inner.CommitData;

    public bool KeepFullyDeletedSegments
    {
        get => inner.KeepFullyDeletedSegments;
        set => inner.KeepFullyDeletedSegments = value;
    }

    public bool IsClosed => inner.IsClosed;
}