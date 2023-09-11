using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Searching;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotJEM.Json.Index2.Test
{
    public class MyFrameworkClassTest
    {
        [Test]
        public async Task SayHello_ReturnsHello()
        {
            IJsonIndex index = new JsonIndexBuilder("myIndex")
                .UsingMemmoryStorage()
                .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
                .WithFieldResolver(new FieldResolver("uuid", "type"))
                .Build();


            index.CreateWriter().Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type="CAR" }));
            index.Commit();

            int count = await index.Search(new TermQuery(new Term("type", "CAR"))).Count();
            Assert.AreEqual(1, count);
        }
    }
}