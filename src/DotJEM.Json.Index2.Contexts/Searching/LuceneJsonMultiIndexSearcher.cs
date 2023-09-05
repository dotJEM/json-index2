using System.Collections.Generic;
using System.Linq;
using DotJEM.Json.Index.Diagnostics;
using DotJEM.Json.Index.Results;
using DotJEM.Json.Index.Searching;
using DotJEM.Json.Index.Util;
using DotJEM.ObservableExtensions.InfoStreams;
using Lucene.Net.Search;

namespace DotJEM.Json.Index.Contexts.Searching
{
    public class LuceneJsonMultiIndexSearcher : Disposable, IJsonIndexSearcher
    {
        public IJsonIndex Index { get; }

        public IInfoStream InfoStream { get; } = new InfoStream<LuceneJsonMultiIndexSearcher>();

        private readonly IJsonIndex[] indicies;

        public LuceneJsonMultiIndexSearcher(IEnumerable<IJsonIndex> indicies)
        {
            this.indicies = indicies.ToArray();
        }

        public ISearch Search(Query query)
        {
            return new Search(new MultiIndexJsonSearcherManager(indicies, null), InfoStream, query);
        }
    }
}