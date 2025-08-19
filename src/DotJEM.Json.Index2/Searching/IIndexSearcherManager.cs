using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Leases;
using DotJEM.Json.Index2.Serialization;
using DotJEM.Json.Index2.Util;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace DotJEM.Json.Index2.Searching;

public interface IIndexSearcherManager : IDisposable
{
    IJsonDocumentSerializer Serializer { get; }
    IIndexSearcherContext Acquire();

    void Close();
}

public class IndexSearcherManager : Disposable, IIndexSearcherManager
{
    private readonly object padlock = new();
    private readonly IIndexWriterManager writerManager;

    private IndexSearcher searcher;
    private object writerRef;

    public IJsonDocumentSerializer Serializer { get; }

    public IndexSearcherManager(IIndexWriterManager writerManager, IJsonDocumentSerializer serializer)
    {
        this.writerManager = writerManager;
        Serializer = serializer;
    }

    public IIndexSearcherContext Acquire()
    {
        lock (padlock)
        {
            ILease<IndexWriter> lease = writerManager.Lease();
            if (searcher is null || !ReferenceEquals(writerRef, lease.Value))
            {
                writerRef = lease.Value;
                searcher = new IndexSearcher(DirectoryReader.Open(lease.Value, true));
                return new IndexSearcherContext(searcher, s => lease.Dispose());
            }
            IndexReader newReader = DirectoryReader.OpenIfChanged((DirectoryReader)searcher.IndexReader);
            if (newReader is null)
                return new IndexSearcherContext(searcher, s => lease.Dispose());

            searcher.IndexReader.Dispose();
            searcher = new IndexSearcher(newReader);
            return new IndexSearcherContext(searcher, s => lease.Dispose());
        }
    }


    public void Close()
    {
        lock (padlock)
        { 
            searcher.IndexReader.Dispose();
            searcher = null;
            writerRef = null;
        }
    }
}
