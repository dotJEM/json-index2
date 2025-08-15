using System;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Leases;
using DotJEM.Json.Index2.Util;
using Lucene.Net.Search;

namespace DotJEM.Json.Index2.Searching
{
    public interface IIndexSearcherContext : IDisposable
    {
        IndexSearcher Searcher { get; }
    }

    public class IndexSearcherContext : Disposable, IIndexSearcherContext
    {
        private readonly ILease<IIndexWriter> lease;
        private readonly IndexSearcher searcher;

        public IndexSearcher Searcher
        {
            get
            {
                if (lease.IsExpired || lease.IsTerminated)
                    throw new LeaseExpiredException("");
                return searcher;
            }
        }

        public IndexSearcherContext(IndexSearcher searcher, ILease<IIndexWriter> lease)
        {
            this.lease = lease;
            this.searcher = searcher;
        }

        protected override void Dispose(bool disposing)
        {
            lease.Dispose();
            base.Dispose(disposing);
        }
    }
}