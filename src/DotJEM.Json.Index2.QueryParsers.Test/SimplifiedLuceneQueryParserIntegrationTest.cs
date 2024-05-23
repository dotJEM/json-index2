using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.QueryParsers.Query;
using DotJEM.Json.Index2.Results;
using DotJEM.Json.Index2.Searching;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.QueryParsers.Flexible.Standard;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotJEM.Json.Index2.QueryParsers.Test;

public class SimplifiedLuceneQueryParserIntegrationTest
{
    [Test]
    public async Task SayHello_ReturnsHello()
    {
        IJsonIndex index = new JsonIndexBuilder("myIndex")
            .UsingMemmoryStorage()
            .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version, CharArraySet.EMPTY_SET))
            .WithFieldResolver(new FieldResolver("uuid", "type"))
            .WithSimplifiedLuceneQueryParser()
            .Build();

        IJsonIndexWriter writer = index.CreateWriter();
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Commit();

        IJsonIndexSearcher? searcher = index.CreateSearcher();
        int count = searcher.Search("type:CAR").Count();
        //int count = searcher.Search(new MatchAllDocsQuery()).Count();
        Assert.AreEqual(5, count);
    }
    [Test]
    public async Task SayHello_ReturnsHell2o()
    {
        IJsonIndex index = new JsonIndexBuilder("myIndex")
            .UsingMemmoryStorage()
            .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version, CharArraySet.EMPTY_SET))
            .WithFieldResolver(new FieldResolver("uuid", "type"))
            .WithSimplifiedLuceneQueryParser()
            .Build();

        IJsonIndexWriter writer = index.CreateWriter();
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "AXE" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "AXE" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "AXE" }));

        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));

        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "FAT" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "FAT" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "FAT" }));

        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAT" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAT" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAT" }));

        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "HAT" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "HAT" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "HAT" }));
        writer.Commit();


        IJsonIndexSearcher? searcher = index.CreateSearcher();
        //new TermQuery(new Term("type", "AXE"))

        //ISearch? search = searcher.Search(new InQuery("type", "car", "foo", "fat"));
        ISearch? search = searcher.Search("type IN (car, foo, fat)");
        //int count = searcher.Search(new MatchAllDocsQuery()).Count();

        foreach (SearchResult result in search.Take(100).Execute())
        {
            Console.Write(result.Data.ToString(Formatting.None));
        }

        Assert.That(search.Count(), Is.EqualTo(6));
    }

    [Test, Explicit]
    public async Task SayHello_ReturnsHell2()
    {
        IJsonIndex index = new JsonIndexBuilder("myIndex")
            .UsingMemmoryStorage()
            //.WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version, CharArraySet.EMPTY_SET))
            .WithAnalyzer(cfg => new EnglishAnalyzer(cfg.Version, CharArraySet.EMPTY_SET))
            .WithFieldResolver(new FieldResolver("uuid", "type"))
            .WithSimplifiedLuceneQueryParser()
            .Build();

        IJsonIndexWriter writer = index.CreateWriter();
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "AXE", name= "Olawale" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "AXE", name= "Wolfhard" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "AXE", name= "Columbo" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", name= "Malle" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", name= "LaToya" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", name= "Jayanti" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "FAT", name= "Sebastian" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "FAT", name= "Tahlako" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "FAT", name= "Kadriye" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAT", name= "Encarnita" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAT", name= "Nyarai" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAT", name= "Vasudha" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "HAT", name= "Genesio" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "HAT", name= "Aleksander" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "HAT", name= "Timothy" }));
        writer.Commit();


        IJsonIndexSearcher? searcher = index.CreateSearcher();
        //new TermQuery(new Term("type", "AXE"))

        //ISearch? search = searcher.Search(new InQuery("type", "car", "foo", "fat"));
        //int count = searcher.Search(new MatchAllDocsQuery()).Count()

        QueryParser orgParser = new QueryParser(LuceneVersion.LUCENE_48, "type", index.Configuration.Analyzer);
        StandardQueryParser stdParser = new StandardQueryParser(index.Configuration.Analyzer);
        ILuceneQueryParser? customParser = index.Configuration.Get<ILuceneQueryParser>();
        
        
        Lucene.Net.Search.Query? query1 = customParser.Parse("name:T*").Query;
        query1 = customParser.Parse("name:Test*").Query;
        query1 = customParser.Parse("name:Test*Hest").Query;
        query1 = customParser.Parse("name:?est*Hes?").Query;
        query1 = customParser.Parse("name:?HORSE*HAS?").Query;
        query1 = customParser.Parse("name:?HORSES*HAS?").Query;

        Lucene.Net.Search.Query? query2 = orgParser.Parse("name:T*");
        Lucene.Net.Search.Query? query3 = stdParser.Parse("name:T*", "type");
        
        ISearch? search = searcher.Search("name:T*");
        foreach (SearchResult result in search.Take(100).Execute())
        {
            Console.Write(result.Data.ToString(Formatting.None));
        }

        Assert.That(search.Count(), Is.EqualTo(2));
    }
}