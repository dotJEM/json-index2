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
        private readonly IIndexWriterManager writerManager;
        private readonly object padlock = new();
        private SearcherManager manager;

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
                manager ??= new SearcherManager(writerManager.Writer, true, new SearcherFactory());
                manager.MaybeRefreshBlocking();
                return new IndexSearcherContext(manager.Acquire(), manager.Release);
            }
        }

        public void Close()
        {
            lock (padlock)
            {
                manager?.Dispose();
                manager = null;
            }
        }
    }
}