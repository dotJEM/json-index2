using System;
using System.Text.RegularExpressions;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Leases;
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
    private readonly IIndexStorageProvider provider;
    private readonly object padlock = new ();
    private volatile Directory directory;
    private readonly LeaseManager<Directory> leaseManager = new();

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
                if (directory != null)
                    return directory;
                return directory = provider.Get();
            }
        }
    }

    public JsonIndexStorageManager(IJsonIndex index, IIndexStorageProvider provider)
    {
        this.provider = provider;
        this.writerManager = new(()=> new IndexWriterManager(index));
        this.searcherManager = new(()=>  new IndexSearcherManager(WriterManager, index.Configuration.Serializer));
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
        if (directory == null)
            return;


        lock (padlock)
        {
            if (directory == null)
                return;

            leaseManager.RecallAll();
            
            Close();
            Unlock();
            foreach (string file in directory.ListAll())
                directory.DeleteFile(file);
            provider.Delete();
        }
    }
}