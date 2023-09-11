using DotJEM.Json.Index2.Documents.Fields;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotJEM.Json.Index2.Test
{
    public class MyFrameworkClassTest
    {
        [Test]
        public void SayHello_ReturnsHello()
        {
            IJsonIndex index = new JsonIndexBuilder("myIndex")
                .UsingMemmoryStorage()
                .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
                .WithFieldResolver(new FieldResolver("uuid", "type"))
                .Build();


            index.CreateWriter().Create(JObject.FromObject(new { uuid = Guid.NewGuid(), type="CAR" }));
        }
    }
}