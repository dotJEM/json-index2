using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Searching;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotJEM.Json.Index2.Contexts.Test;

public class LuceneIndexContextTest
{
    [Test]
    public async Task SayHello_ReturnsHello()
    {
        IJsonIndexContextBuilder builder = new JsonIndexContextBuilder();
        builder
            .ByDefault(x => x
                .UsingMemmoryStorage()
                .UsingAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
                .UsingFieldResolver(new FieldResolver("uuid", "type")));
        //builder
        //    .For("IndexName", x => x
        //        .UsingMemmoryStorage()
        //        .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
        //        .WithFieldResolver(new FieldResolver("uuid", "type")));


        IJsonIndexContext context = builder.Build();
        context.Open("index1");

        IJsonIndex index = context.Open("index1");
        IJsonIndex index2 = context.Open("index2");

        WriteToIndex(index);
        WriteToIndex(index2);

        IJsonIndexSearcher? searcher = index.CreateSearcher();
        IJsonIndexSearcher? searcher2 = index2.CreateSearcher();

        Assert.That(searcher.Search(new TermQuery(new Term("type", "car"))).Count(), Is.EqualTo(5));
        Assert.That(searcher2.Search(new TermQuery(new Term("type", "car"))).Count(), Is.EqualTo(5));

        IJsonIndexSearcher? combined = context.CreateSearcher();
        Assert.That(combined.Search(new TermQuery(new Term("type", "car"))).Count(), Is.EqualTo(10));

    }

    private static void WriteToIndex(IJsonIndex index)
    {
        IJsonIndexWriter writer = index.CreateWriter();
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type = "CAR" }));
        writer.Commit();
    }
}