using System;
using System.IO;

namespace DotJEM.Json.Index2.Contexts.Storage
{
    public class SimpleFileSystemRootStorageFacility : ILuceneStorageFactoryProvider
    {
        private readonly string rootDirectory;

        public SimpleFileSystemRootStorageFacility(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public Func<ILuceneStorageFactory> Create(string name)
        {
            return () => new LuceneSimpleFileSystemStorageFactory(Path.Combine(rootDirectory, name));
        }

        public ILuceneStorageFactory Get(string name)
        {
            return new LuceneSimpleFileSystemStorageFactory(Path.Combine(rootDirectory, name));
        }
    }
}