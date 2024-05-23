using System.Linq;
using DotJEM.Json.Index2.Searching;
using DotJEM.Json.Index2.Serialization;
using DotJEM.Json.Index2.Util;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace DotJEM.Json.Index2.Contexts.Searching
{
    public class MultiIndexJsonSearcherManager : Disposable, IIndexSearcherManager
    {
        private readonly IJsonIndex[] indicies;
        public IJsonDocumentSerializer Serializer { get; }

        public MultiIndexJsonSearcherManager(IJsonIndex[] indicies, IJsonDocumentSerializer serializer)
        {
            this.indicies = indicies;
            Serializer = serializer;
        }

        public IIndexSearcherContext Acquire()
        {
            //TODO: Do we need to respect the Writer Lease here, or is this OK as we just get the reader?
            IndexReader[] readers = indicies
                .Select(idx => idx.WriterManager.Lease().Value.GetReader(true))
                .Select(r => DirectoryReader.OpenIfChanged(r) ?? r)
                .Cast<IndexReader>()
                .ToArray();

            MultiReader reader = new MultiReader(readers, false);
            return new IndexSearcherContext(new IndexSearcher(reader), searcher => {});
        }

        public void Close()
        {
        }
    }
}