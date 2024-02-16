using System;
using System.Collections.Concurrent;
using DotJEM.Json.Index2.Configuration;
using Lucene.Net.Util;

namespace DotJEM.Json.Index2.Contexts.Configuration
{
    public interface IJsonIndexConfigurationProvider
    {
        IJsonIndexConfiguration Acquire(string name);
        IJsonIndexConfigurationProvider Use(string name, IJsonIndexConfiguration config);
    }

    public class LuceneIndexConfigurationProvider : IJsonIndexConfigurationProvider
    {
        public IJsonIndexConfiguration Global { get; set; } = new JsonIndexConfiguration(LuceneVersion.LUCENE_48, Array.Empty<ServiceDescriptor>());

        private readonly ConcurrentDictionary<string, IJsonIndexConfiguration> configurations
            = new ConcurrentDictionary<string, IJsonIndexConfiguration>();

        public IJsonIndexConfiguration Acquire(string name)
            => configurations.GetOrAdd(name, s => new JsonContextIndexConfiguration(Global));

        public IJsonIndexConfigurationProvider Use(string name, IJsonIndexConfiguration config)
        {
            configurations[name] = config;
            return this;
        }
    }
}