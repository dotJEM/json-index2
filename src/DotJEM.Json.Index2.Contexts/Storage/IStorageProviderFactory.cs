using System.IO;
using DotJEM.Json.Index2.Storage;

namespace DotJEM.Json.Index2.Contexts.Storage;

public interface IStorageProviderFactory
{
    IIndexStorageProvider Create(string indexName);
}

public class RamStorageProviderFactory : IStorageProviderFactory
{
    private readonly IIndexStorageProvider provider = new RamIndexStorageProvider();

    public IIndexStorageProvider Create(string indexName) => provider;
}


public class SimpleFsStorageProviderFactory : IStorageProviderFactory
{
    private readonly string root;

    public SimpleFsStorageProviderFactory(string root)
    {
        this.root = root;
    }

    public IIndexStorageProvider Create(string indexName) => new SimpleFsIndexStorageProvider(Path.Combine(root, indexName));
}