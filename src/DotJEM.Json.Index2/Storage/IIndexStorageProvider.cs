using System.IO;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

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
        //TODO: For now. But maybe there is cases where this actually makes sense to always have.
        //DirectoryInfo dir = new DirectoryInfo(path);
        //foreach (FileInfo file in dir.EnumerateFiles())
        //    file.Delete();
        
        //foreach (DirectoryInfo directory in dir.EnumerateDirectories())
        //    directory.Delete(true);
    }
}