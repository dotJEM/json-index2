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
        private readonly ResetableLazy<SearcherManager> manager;
        
        public IJsonDocumentSerializer Serializer { get; }

        public IndexSearcherManager(IIndexWriterManager writerManager, IJsonDocumentSerializer serializer)
        {
            Serializer = serializer;
            manager = new ResetableLazy<SearcherManager>(() => new SearcherManager(writerManager.Writer, true, new SearcherFactory()));
        }

        public IIndexSearcherContext Acquire()
        {

            manager.Value.MaybeRefreshBlocking();
            return new IndexSearcherContext(manager.Value.Acquire(), searcher => manager.Value.Release(searcher));
        }

        public void Close()
        {
            manager.Value.Dispose();
            manager.Reset();
        }
    }
}