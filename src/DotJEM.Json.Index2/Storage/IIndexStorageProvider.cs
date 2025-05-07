using System;
using System.Diagnostics;
using System.IO;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace DotJEM.Json.Index2.Storage;

public interface IIndexStorageProvider
{
    string Path { get; }
    Directory Get();
    void Delete();
}

public class RamIndexStorageProvider : IIndexStorageProvider
{
    public string Path => "N/A";
    public Directory Get() => new RAMDirectory();
    
    public void Delete()
    {

    }
}

public class SimpleFsIndexStorageProvider : IIndexStorageProvider
{
    private readonly string path;
    public string Path => path;

    public SimpleFsIndexStorageProvider(string path)
    {
        this.path = path;
    }

    public Directory Get() => new SimpleFSDirectory(path);

    public void Delete()
    {
        Debug.WriteLine("DELETE FILES");
        //TODO: For now. But maybe there is cases where this actually makes sense to always have.
        try
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (FileInfo file in dir.EnumerateFiles())
                file.Delete();

            foreach (DirectoryInfo directory in dir.EnumerateDirectories())
                directory.Delete(true);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}