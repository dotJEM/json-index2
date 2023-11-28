using System.Collections.Generic;
using System.Linq;
using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Results;
using DotJEM.Json.Index2.Searching;
using DotJEM.Json.Index2.Util;
using DotJEM.ObservableExtensions.InfoStreams;
using Lucene.Net.Search;

namespace DotJEM.Json.Index2.Contexts.Searching
{
    public class LuceneJsonMultiIndexSearcher : Disposable, IJsonIndexSearcher
    {
        public IJsonIndex Index { get; }

        public IInfoStream InfoStream { get; } = new InfoStream<LuceneJsonMultiIndexSearcher>();

        private readonly IJsonIndex[] indicies;
        private readonly IJsonIndexConfiguration configuration;

        public LuceneJsonMultiIndexSearcher(IEnumerable<IJsonIndex> indicies)
        {
            this.indicies = indicies.ToArray();
            this.configuration = this.indicies.First().Configuration;
        }

        public ISearch Search(Query query)
        {
            return new Search(new MultiIndexJsonSearcherManager(indicies, configuration.Serializer), query);
        }
    }
}