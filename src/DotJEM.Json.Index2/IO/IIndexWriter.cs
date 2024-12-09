using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace DotJEM.Json.Index2.IO;

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