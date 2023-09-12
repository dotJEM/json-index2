using System;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Serialization;
using DotJEM.Json.Index2.Util;
using Lucene.Net.Search;

namespace DotJEM.Json.Index2.Searching
{
    public interface IIndexSearcherManager : IDisposable
    {
        IJsonDocumentSerializer Serializer { get; }
        IIndexSearcherContext Acquire();

        void Close();
    }

    public class IndexSearcherManager : Disposable, IIndexSearcherManager
    {
        private readonly SearcherManager manager;

        public IJsonDocumentSerializer Serializer { get; }

        public IndexSearcherManager(IIndexWriterManager writerManager, IJsonDocumentSerializer serializer)
        {
            Serializer = serializer;
            manager = new SearcherManager(writerManager.Writer, true, new SearcherFactory());
        }

        public IIndexSearcherContext Acquire()
        {
            manager.MaybeRefreshBlocking();
            return new IndexSearcherContext(manager.Acquire(), searcher => manager.Release(searcher));
        }

        public void Close()
        {
            manager.Dispose();
        }
    }
}