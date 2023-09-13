using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.QueryParsers.Query;
using DotJEM.Json.Index2.Results;
using DotJEM.Json.Index2.Searching;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotJEM.Json.Index2.QueryParsers.Test;

public class JsonIndexTest
{
    [Test]
    public async Task SayHello_ReturnsHello()
    {
        IJsonIndex index = new JsonIndexBuilder("myIndex")
            .UsingMemmoryStorage()
            .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
            .WithFieldResolver(new FieldResolver("uuid", "type"))
            .UseSimplifiedLuceneQueryParser()
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
            .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
            .WithFieldResolver(new FieldResolver("uuid", "type"))
            .UseSimplifiedLuceneQueryParser()
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
}