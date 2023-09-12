using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Searching;
using DotJEM.Json.Index2.Storage;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Contexts.Sharding
{
    internal class ShardingJsonIndex : IJsonIndex
    {
        public IJsonIndexSearcher CreateSearcher()
        {
            throw new NotImplementedException();
        }

        public IInfoStream InfoStream { get; }
        public IJsonIndexStorageManager Storage { get; }
        public IJsonIndexConfiguration Configuration { get; }
        public IIndexWriterManager WriterManager { get; }
        public IIndexSearcherManager SearcherManager { get; }

        public IJsonIndexWriter CreateWriter()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
    }
}
