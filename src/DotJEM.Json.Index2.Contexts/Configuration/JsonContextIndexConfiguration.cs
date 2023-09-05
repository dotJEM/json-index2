using DotJEM.Json.Index2.Configuration;

namespace DotJEM.Json.Index2.Contexts.Configuration
{
    public class JsonContextIndexConfiguration : JsonIndexConfiguration
    {
        public IJsonIndexConfiguration ContextConfiguration { get; }

        public JsonContextIndexConfiguration(IJsonIndexConfiguration contextConfiguration)
        {
            ContextConfiguration = contextConfiguration;
        }
    }
}