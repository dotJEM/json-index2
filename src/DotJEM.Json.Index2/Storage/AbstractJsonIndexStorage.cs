using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Searching;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace DotJEM.Json.Index2.Storage;

public interface IJsonIndexStorageManager
{
    IIndexWriterManager WriterManager { get; }
    IIndexSearcherManager SearcherManager { get; }
    bool Exists { get; }
    Directory Directory { get; }
    void Unlock();
    void Close();
    void Delete();
}

public class JsonIndexStorageManager: IJsonIndexStorageManager
{
    private readonly IJsonIndex index;
    private readonly IJsonIndexStorage provider;
    private readonly object padlock = new ();
    private Directory directory;

    public IIndexWriterManager WriterManager { get; }
    
    public IIndexSearcherManager SearcherManager { get; }
    
    public bool Exists => DirectoryReader.IndexExists(Directory);

    public Directory Directory
    {
        get
        {
            if (directory != null)
                return directory;

            lock (padlock)
            {
                return directory ??= provider.Get();
            }
        }
        protected set => directory = value;
    }

    public JsonIndexStorageManager(IJsonIndex index, IJsonIndexStorage provider)
    {
        this.index = index;
        this.provider = provider;
        this.WriterManager = new IndexWriterManager(index);
        this.SearcherManager = new IndexSearcherManager(WriterManager, index.Configuration.Serializer);
    }
    
    public void Unlock()
    {
        if (IndexWriter.IsLocked(Directory))
            IndexWriter.Unlock(Directory);
    }

    public void Close()
    {
        SearcherManager.Close();
        WriterManager.Close();
    }

    public void Delete()
    {
        Close();
        Unlock();
        foreach (string file in Directory.ListAll())
            Directory.DeleteFile(file);

        Directory.Dispose();
        Directory = null;

        provider.Delete();
    }
}


public interface IJsonIndexStorage
{
    Directory Get();
    void Delete();
}

public class RamJsonIndexStorage : IJsonIndexStorage
{
    public Directory Get() => new RAMDirectory();
    public void Delete()
    {
        throw new System.NotImplementedException();
    }
}

public class SimpleFsJsonIndexStorage : IJsonIndexStorage
{
    private readonly string path;

    public SimpleFsJsonIndexStorage(string path)
    {
        this.path = path;
    }
    
    public Directory Get() => new SimpleFSDirectory(path);

    public void Delete()
    {
        throw new System.NotImplementedException();
    }
}