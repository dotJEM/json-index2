using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Contexts.Configuration;

namespace DotJEM.Json.Index2.Contexts
{
    public class ContextedLuceneJsonIndexBuilder : LuceneJsonIndexBuilder
    {
        public ContextedLuceneJsonIndexBuilder(string name)
            : this(name, ServiceCollection.CreateDefault())
        {
        }
        public ContextedLuceneJsonIndexBuilder(string name,IServiceCollection services)
            : base(name, new PerIndexServiceCollection(services))
        {
        }
    }
}