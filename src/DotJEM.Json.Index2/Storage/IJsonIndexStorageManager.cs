using System;
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
    private readonly IIndexStorageProvider provider;
    private readonly object padlock = new ();
    private Directory directory;

    private readonly Lazy<IIndexWriterManager> writerManager;
    private readonly Lazy<IIndexSearcherManager> searcherManager;

    public IIndexWriterManager WriterManager => writerManager.Value;
    public IIndexSearcherManager SearcherManager => searcherManager.Value;
    
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

    public JsonIndexStorageManager(IJsonIndex index, IIndexStorageProvider provider)
    {
        this.index = index;
        this.provider = provider;
        this.writerManager = new Lazy<IIndexWriterManager>(()=> new IndexWriterManager(index));
        this.searcherManager = new Lazy<IIndexSearcherManager>(()=>  new IndexSearcherManager(WriterManager, index.Configuration.Serializer));
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
        if(!Exists)
            return;

        Close();
        Unlock();
        foreach (string file in Directory.ListAll())
            Directory.DeleteFile(file);
        provider.Delete();
    }
}