using Lucene.Net.Util;

namespace DotJEM.Json.Index.Storage
{
    public interface ILuceneStorageFactory
    {
        IJsonIndexStorage Create(IJsonIndex index, LuceneVersion version);
    }

    public class LuceneRamStorageFactory : ILuceneStorageFactory
    {
        public IJsonIndexStorage Create(IJsonIndex index, LuceneVersion version) => new RamJsonIndexStorage(index);
    }

    public class LuceneSimpleFileSystemStorageFactory : ILuceneStorageFactory
    {
        private readonly string path;
        
        public LuceneSimpleFileSystemStorageFactory(string path)
        {
            this.path = path;
        }

        public IJsonIndexStorage Create(IJsonIndex index, LuceneVersion version) => new SimpleFsJsonIndexStorage(index, path);
    }
}
