using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Searching;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotJEM.Json.Index2.Test;

public class JsonIndexTest
{
    [Test]
    public async Task Create_AddsDocument()
    {
        IJsonIndex index = new JsonIndexBuilder("myIndex")
            .UsingMemmoryStorage()
            .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
            .WithFieldResolver(new FieldResolver("uuid", "type"))
            .Build();

        IJsonIndexWriter writer = index.CreateWriter();
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Commit();

        IJsonIndexSearcher? searcher = index.CreateSearcher();
        int count = searcher.Search(new TermQuery(new Term("type", "car"))).Count();
        //int count = searcher.Search(new MatchAllDocsQuery()).Count();
        Assert.AreEqual(5, count);
    }

    [Test]
    public async Task SayHello_ReturnsHello()
    {
        IJsonIndex index = new JsonIndexBuilder("myIndex")
            .UsingMemmoryStorage()
            .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
            .WithFieldResolver(new FieldResolver("uuid", "type"))
            .Build();

        IJsonIndexWriter writer = index.CreateWriter();
        Guid uuid = Guid.NewGuid();
        writer.Update(JObject.FromObject(new { uuid, type = "CAR" }));
        writer.Update(JObject.FromObject(new { uuid, type = "CAR" }));
        writer.Update(JObject.FromObject(new { uuid, type = "CAR" }));
        writer.Update(JObject.FromObject(new { uuid, type = "CAR" }));
        writer.Update(JObject.FromObject(new { uuid, type = "CAR" }));
        writer.Commit();



        IJsonIndexSearcher? searcher = index.CreateSearcher();
        //int count = searcher.Search(new TermQuery(new Term("type", "car"))).Count();
        int count = searcher.Search(new MatchAllDocsQuery()).Count();
        Assert.AreEqual(1, count);
    }
    [Test]
    public async Task Search_Booleans()
    {
        IJsonIndex index = new JsonIndexBuilder("myIndex")
            .UsingMemmoryStorage()
            .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
            .WithFieldResolver(new FieldResolver("uuid", "type"))
            .Build();

        IJsonIndexWriter writer = index.CreateWriter();
        writer.Update(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", inStock = false }));
        writer.Update(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", inStock = true }));
        writer.Update(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", inStock = false }));
        writer.Update(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", inStock = true }));
        writer.Update(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", inStock = true }));
        writer.Commit();



        IJsonIndexSearcher? searcher = index.CreateSearcher();
        //int count = searcher.Search(new TermQuery(new Term("type", "car"))).Count();
        int count = searcher.Search(new TermQuery(new Term("inStock", "true"))).Count();
        Assert.AreEqual(3, count);
    }
    [Test]
    public async Task FindBeforeCommit_AddsDocument()
    {
        IJsonIndex index = new JsonIndexBuilder("myIndex")
            .UsingMemmoryStorage()
            .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
            .WithFieldResolver(new FieldResolver("uuid", "type"))
            .Build();

        IJsonIndexWriter writer = index.CreateWriter();
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));

        IJsonIndexSearcher? searcher = index.CreateSearcher();
        Assert.That(searcher.Search(new TermQuery(new Term("type", "car"))).Count(), Is.EqualTo(5));

        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        Assert.That(searcher.Search(new TermQuery(new Term("type", "car"))).Count(), Is.EqualTo(6));
    }
}