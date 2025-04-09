using Lucene.Net.Store;

namespace DotJEM.Json.Index2.Storage;

public interface IIndexStorageProvider
{
    Directory Get();
    void Delete();
}

public class RamIndexStorageProvider : IIndexStorageProvider
{

    public Directory Get() => new RAMDirectory();
    
    public void Delete()
    {

    }
}

public class SimpleFsIndexStorageProvider : IIndexStorageProvider
{
    private readonly string path;

    public SimpleFsIndexStorageProvider(string path)
    {
        this.path = path;
    }
    
    public Directory Get() => new SimpleFSDirectory(path);

    public void Delete()
    {
    }
}