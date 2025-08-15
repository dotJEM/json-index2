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
    private SearcherManager manager;
    private IIndexWriter writer;

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
            ILease<IIndexWriter> lease = writerManager.Lease();
            if (searcher is null || !ReferenceEquals(writer, lease.Value))
            {
                writer = lease.Value;
                searcher = new IndexSearcher(DirectoryReader.Open(lease.Value.UnsafeValue, true));
                return new IndexSearcherContext(searcher, lease);
            }

            IndexReader newReader = DirectoryReader.OpenIfChanged((DirectoryReader)searcher.IndexReader);
            if (newReader is null)
                return new IndexSearcherContext(searcher, lease);

            searcher = new IndexSearcher(newReader);
            return new IndexSearcherContext(searcher, lease);


            //manager ??= new SearcherManager(writerManager.Lease().Value.UnsafeValue, true, new SearcherFactory());
            //manager.MaybeRefreshBlocking();
            //return new IndexSearcherContext(manager.Acquire(), manager.Release);
        }
    }


    public void Close()
    {
        lock (padlock)
        {
            searcher = null;
            writer = null;
            manager?.Dispose();
        }
    }
}
