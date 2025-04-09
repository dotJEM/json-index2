using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Documents.Builder;
using DotJEM.Json.Index2.Documents.Data;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.Documents.Strategies;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Searching;
using DotJEM.ObservableExtensions.InfoStreams;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
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

    [Test]
    public async Task IdentityFieldsShouldNotRerturnAsTokenized()
    {
        IJsonIndex index = new JsonIndexBuilder("myIndex")
            .UsingMemmoryStorage()
            .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
            .WithFieldResolver(new FieldResolver("uuid", "type"))
            .WithDocumentFactory(cfg => new LuceneDocumentFactory(cfg.FieldInformationManager, new FuncFactory<ILuceneDocumentBuilder>(()=> new FakeDocumentBuilder())))
            .Build();

        IJsonIndexWriter writer = index.CreateWriter();
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", identity = "abc" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", identity = "abc def" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", identity = "abc def xyz" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", identity = "xyz def" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", identity = "abc-fgh" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR", identity = "abc-lmb" }));

        IJsonIndexSearcher? searcher = index.CreateSearcher();
        Assert.That(searcher.Search(new TermQuery(new Term("identity", "abc"))).Count(), Is.EqualTo(1));
    }
}

public class FakeDocumentBuilder : LuceneDocumentBuilder
{
    protected override void VisitGuid(JValue json, IPathContext context)
    {
        this.Add(new IdentityFieldStrategy().CreateFields(json, context));
    }

    protected override void VisitBoolean(JValue json, IPathContext context)
    {
        Add(new TextFieldStrategy().CreateFields(json, context));
    }

    protected override void VisitString(JValue json, IPathContext context)
    {
        string value = (string)json;
        if (value?.Length == 36 && Guid.TryParse(value, out _))
            base.VisitGuid(json, context);
        else
            base.VisitString(json, context);
    }

    protected override void Visit(JValue json, IPathContext context)
    {
        switch (context.Path)
        {
            case "identity":
                this.Add(new IdentityFieldStrategy()
                    .CreateFields(json, context));
                break;
        }
    }
}

