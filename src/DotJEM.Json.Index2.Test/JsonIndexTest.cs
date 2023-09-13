using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Searching;
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
    public async Task SayHello_ReturnsHello()
    {
        IJsonIndex index = new JsonIndexBuilder("myIndex")
            .UsingMemmoryStorage()
            .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
            .WithFieldResolver(new FieldResolver("uuid", "type"))
            .Build();

        IJsonIndexWriter writer = index.CreateWriter();
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type="CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type="CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type="CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type="CAR" }));
        writer.Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type="CAR" }));
        writer.Commit();

        IJsonIndexSearcher? searcher = index.CreateSearcher();
        int count = searcher.Search(new TermQuery(new Term("type", "car"))).Count();
        //int count = searcher.Search(new MatchAllDocsQuery()).Count();
        Assert.AreEqual(5, count);
    }
}