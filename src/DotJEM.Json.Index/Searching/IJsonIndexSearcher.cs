using System;
using DotJEM.Json.Index.Diagnostics;
using DotJEM.Json.Index.Results;
using DotJEM.Json.Index.Util;
using DotJEM.ObservableExtensions.InfoStreams;
using Lucene.Net.Search;

namespace DotJEM.Json.Index.Searching
{
    public interface IJsonIndexSearcher : IDisposable
    {
        IJsonIndex Index { get; }
        IInfoStream InfoStream { get; }
        ISearch Search(Query query);
    }

    public class JsonIndexSearcher : Disposable, IJsonIndexSearcher
    {
        public IJsonIndex Index { get; }
        public IInfoStream InfoStream { get; } = new InfoStream<JsonIndexSearcher>();

        public JsonIndexSearcher(IJsonIndex index)
        {
            Index = index;
        }

        public ISearch Search(Query query)
        {
            return new Search(Index.SearcherManager, InfoStream, query);
        }
    }

    public static class IndexSearcherExtensions
    {
        public static ISearch Search(this IJsonIndex self, Query query)
        {
            return self.CreateSearcher().Search(query);
        }
    }
}