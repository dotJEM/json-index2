using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

    int Count();
    SearchResults Execute();
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
        : this(manager, new InfoStream<Search>(), query)
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

    public int Count() => Take(1).Execute().TotalHits;

    public SearchResults Execute() => Execute(query, skip, take, sort, filter, doDocScores, doMaxScores);

    private SearchResults Execute(Query query, int skip, int take, Sort sort, Filter filter, bool doDocScores, bool doMaxScores)
    {
        //TODO: Replace yield
        infoStream.WriteSearchStartEvent("", new SearchInfo(query, skip, take, sort, filter,doDocScores,doMaxScores), TimeSpan.Zero);
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

        TopFieldDocs topDocs = searcher.Search(query, filter, take + skip, sort, doDocScores, doMaxScores);

        infoStream.WriteSearchCompletedEvent("", new SearchInfo(query, skip, take, sort, filter, doDocScores, doMaxScores), timer.Elapsed);
        IEnumerable<SearchResult> loaded =
            from hit in topDocs.ScoreDocs.Skip(skip)
            let document = searcher.Doc(hit.Doc, serializer.FieldsToLoad)
            select new SearchResult(hit.Score, serializer.DeserializeFrom(document));

        //TODO: We could throw in another measurement (Load vs Deserialization)...
        //      That would require us to force evaluate the above though (e.g. ToList it)....

        SearchResult[] results = loaded.ToArray();

        infoStream.WriteSearchDataLoadedEvent("", new SearchInfo(query, skip, take, sort, filter, doDocScores, doMaxScores), timer.Elapsed);
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

public static class InfoStreamSearchExtensions
{
    public static void WriteSearchStartEvent<TSource>(this IInfoStream<TSource> self, string message, SearchInfo search, TimeSpan elapsed, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
    {
        self.WriteEvent(
            new SearchInfoStreamEvent(typeof(TSource), InfoLevel.INFO, message, SearchEventType.Start, search, elapsed, callerMemberName, callerFilePath, callerLineNumber)
        );
    }
    public static void WriteSearchCompletedEvent<TSource>(this IInfoStream<TSource> self, string message, SearchInfo search, TimeSpan elapsed, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
    {
        self.WriteEvent(
            new SearchInfoStreamEvent(typeof(TSource), InfoLevel.INFO, message, SearchEventType.SearchCompleted, search, elapsed, callerMemberName, callerFilePath, callerLineNumber)
        );
    }
    public static void WriteSearchDataLoadedEvent<TSource>(this IInfoStream<TSource> self, string message, SearchInfo search, TimeSpan elapsed, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
    {
        self.WriteEvent(
            new SearchInfoStreamEvent(typeof(TSource), InfoLevel.INFO, message, SearchEventType.DataLoaded, search, elapsed, callerMemberName, callerFilePath, callerLineNumber)
        );
    }
}

public readonly record struct SearchInfo(Query query, int skip, int take, Sort sort, Filter filter, bool doDocScores, bool doMaxScores)
{
}

public enum SearchEventType
{
    Start,
    SearchCompleted,
    DataLoaded
}

public class SearchInfoStreamEvent : InfoStreamEvent
{
    public TimeSpan Elapsed { get; }
    public SearchEventType EventType { get; }
    public SearchInfo SearchInfo { get; }

    public SearchInfoStreamEvent(Type source, InfoLevel level, string message, SearchEventType eventType, SearchInfo search, TimeSpan elapsed, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, message, callerMemberName, callerFilePath, callerLineNumber)
    {
        EventType = eventType;
        SearchInfo = search;
        Elapsed = elapsed;
    }

    public override string ToString()
    {
        return $"[{Level}] {EventType}:{Message} ({SearchInfo})";
    }
}

