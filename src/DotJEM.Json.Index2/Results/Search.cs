using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Searching;
using DotJEM.Json.Index2.Serialization;
using DotJEM.ObservableExtensions.InfoStreams;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Results;
    public readonly record struct SearchResult(float Score, JObject Data);


    public interface ISearch
    {
        IInfoStream EventInfoStream { get; }

        ISearch Take(int newTake);
        ISearch Skip(int newSkip);
        ISearch Query(Query newQuery);
        ISearch OrderBy(Sort newSort);
        ISearch Filter(Filter newFilter);
        ISearch WithoutDocScores();
        ISearch WithoutMaxScores();
        ISearch WithoutScores();
        ISearch WithDocScores();
        ISearch WithMaxScores();
        ISearch WithScores();

        Task<int> Count();
        Task<SearchResults> Execute();
    }

    public sealed class Search : ISearch
    {
        private readonly IIndexSearcherManager manager;

        private readonly int skip, take;
        private readonly Query query;
        private readonly Filter filter;
        private readonly bool doDocScores;
        private readonly bool doMaxScores;
        private readonly Sort sort;
        private readonly InfoStream<Search> infoStream;

        public IInfoStream EventInfoStream => infoStream;

        public ISearch Take(int newTake) => new Search(manager, infoStream, query, skip, newTake, sort, filter, doDocScores, doMaxScores);
        public ISearch Skip(int newSkip) => new Search(manager, infoStream, query, newSkip, take, sort, filter, doDocScores, doMaxScores);
        public ISearch Query(Query newQuery) => new Search(manager, infoStream, newQuery, skip, take, sort, filter, doDocScores, doMaxScores);
        public ISearch OrderBy(Sort newSort) => new Search(manager, infoStream, query, skip, take, newSort, filter, doDocScores, doMaxScores);
        public ISearch Filter(Filter newFilter) => new Search(manager, infoStream, query, skip, take, sort, newFilter, doDocScores, doMaxScores);

        public ISearch WithoutDocScores() => new Search(manager, infoStream, query, skip, take, sort, filter, false, doMaxScores);
        public ISearch WithoutMaxScores() => new Search(manager, infoStream, query, skip, take, sort, filter, doDocScores, false);
        public ISearch WithoutScores() => new Search(manager, infoStream, query, skip, take, sort, filter, false, false);

        public ISearch WithDocScores() => new Search(manager, infoStream, query, skip, take, sort, filter, true, doMaxScores);
        public ISearch WithMaxScores() => new Search(manager, infoStream, query, skip, take, sort, filter, doDocScores, true);
        public ISearch WithScores() => new Search(manager, infoStream, query, skip, take, sort, filter, true, true);

        public Search(IIndexSearcherManager manager, Query query)
            :this(manager, new InfoStream<Search>(), query)
        {
        }

        private Search(IIndexSearcherManager manager, InfoStream<Search> eventInfo, Query query = null, int skip = 0, int take = 25, Sort sort = null, Filter filter = null, bool doDocScores = true, bool doMaxScores = true)
        {
            this.manager = manager;
            this.infoStream = eventInfo;
            this.skip = skip;
            this.take = take;
            this.query = query;
            this.filter = filter;
            this.doDocScores = doDocScores;
            this.doMaxScores = doMaxScores;
            this.sort = sort ?? Sort.RELEVANCE;
        }

        public async Task<int> Count() => (await Take(1).Execute()).TotalHits;

        public Task<SearchResults> Execute() => Execute(query, skip, take, sort, filter, doDocScores, doMaxScores);

        private async Task<SearchResults> Execute(Query query, int skip, int take, Sort sort, Filter filter, bool doDocScores, bool doMaxScores)
    {
        await Task.Yield();
        infoStream.WriteDebug($"Execute search for query '{query}' (skip={skip}, take={take}, sort={sort}, filter={filter}, doDocScores={doDocScores}, doMaxScores={doMaxScores})");

        Stopwatch timer = Stopwatch.StartNew();
        using IIndexSearcherContext context = manager.Acquire();
        IndexSearcher searcher = context.Searcher;
        //s.Doc()
        //query = s.Rewrite(query);
        infoStream.WriteDebug($"Query Rewrite: {query}");

        // https://issues.apache.org/jira/secure/attachment/12430688/PagingCollector.java

        //TopScoreDocCollector collector = TopScoreDocCollector.Create(int.MaxValue, true);
        //TopDocs docs = collector.GetTopDocs(0, 100);
        //TopFieldCollector collector2 = TopFieldCollector.Create(sort, 100, false, false, false, false);
        //Query fq = filter != null 
        //    ? new FilteredQuery(query, filter)
        //    : query;
        //Weight w = s.CreateNormalizedWeight(fq);
        //collector2.GetTopDocs()
        IJsonDocumentSerializer serializer = manager.Serializer;

        TopFieldDocs topDocs = searcher.Search(query, filter, take, sort, doDocScores, doMaxScores);

        TimeSpan searchTime = timer.Elapsed;
        infoStream.WriteInfo($"Search took {searchTime.TotalMilliseconds} ms");
        IEnumerable<SearchResult> loaded =
            from hit in topDocs.ScoreDocs.Skip(skip)
            let document = searcher.Doc(hit.Doc, serializer.FieldsToLoad)
            select new SearchResult(hit.Score, serializer.DeserializeFrom(document));

        //TODO: We could throw in another measurement (Load vs Deserialization)...
        //      That would require us to force evaluate the above though (e.g. ToList it)....

        SearchResult[] results = loaded.ToArray();

        TimeSpan loadTime = timer.Elapsed;
        infoStream.WriteInfo($"Data load took: {loadTime.TotalMilliseconds} ms");
        return new SearchResults(results, topDocs.TotalHits);
    }
}

    public class SearchResults : IEnumerable<SearchResult>
    {
        public SearchResult[] Hits { get; }
        public int TotalHits { get; }

        public SearchResults(SearchResult[] hits, int totalHits)
        {
            Hits = hits;
            TotalHits = totalHits;
        }
        
        public IEnumerator<SearchResult> GetEnumerator() => Hits.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

