using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotJEM.Json.Index2.Management.Snapshots.Zip.Meta;
using DotJEM.Json.Index2.Snapshots;
using DotJEM.Json.Index2.Snapshots.Zip;
using DotJEM.ObservableExtensions.InfoStreams;
using Lucene.Net.Index;

namespace DotJEM.Json.Index2.Management.Snapshots.Zip;

public class ZipSnapshotStrategy : ISnapshotStrategy
{
    private readonly InfoStream<ZipSnapshotStrategy> infoStream = new();
    public IInfoStream InfoStream => infoStream;

    private readonly int maxSnapshots;
    private readonly MetaZipSnapshotStorage storage;
    public ISnapshotStorage Storage => storage;

    public ZipSnapshotStrategy(string path, int maxSnapshots = 2)
    {
        this.maxSnapshots = maxSnapshots;
        this.storage = new MetaZipSnapshotStorage(path);
        this.storage.InfoStream.Subscribe(infoStream);
    }

    public void CleanOldSnapshots()
    {
        foreach (ISnapshot snapshot in Storage.LoadSnapshots().Skip(maxSnapshots))
        {
            try
            {
                snapshot.Delete();
                infoStream.WriteInfo($"Deleted snapshot: {snapshot}");
            }
            catch (Exception ex)
            {
                infoStream.WriteError($"Failed to delete snapshot: {snapshot}", ex);
            }
        }
    }
}


public class MetaZipSnapshotStorage : ISnapshotStorage
{
    private readonly InfoStream<MetaZipSnapshotStorage> infoStream = new();
    public IInfoStream InfoStream => infoStream;

    private readonly string path;

    public MetaZipSnapshotStorage(string path)
    {
        this.path = path;
    }

    public ISnapshot CreateSnapshot(IndexCommit commit)
    {
        string snapshotPath = Path.Combine(path, $"{commit.Generation:x8}.zip");
        return CreateSnapshot(snapshotPath);
    }

    public IEnumerable<ISnapshot> LoadSnapshots()
    {
        return Directory.GetFiles(path, "*.zip")
            .Select(CreateSnapshot)
            .OrderByDescending(f => f.Generation);
    }

    private ISnapshot CreateSnapshot(string path)
    {
        MetaZipFileSnapshot snapshot = new MetaZipFileSnapshot(path);
        snapshot.InfoStream.Subscribe(infoStream);
        return snapshot;
    }
}

public class MetaZipFileSnapshot : ZipFileSnapshot
{
    private readonly InfoStream<MetaZipFileSnapshot> infoStream = new();
    public IInfoStream InfoStream => infoStream;

    public override ISnapshotReader OpenReader()
    {
        infoStream.WriteSnapshotOpenEvent(this, "");
        MetaZipSnapshotReader reader = new(this.FilePath);
        reader.InfoStream.Subscribe(infoStream);
        return reader;
    }

    public override ISnapshotWriter OpenWriter()
    {
       ManagerZipSnapshotWriter writer= new(this.FilePath);
       writer.InfoStream.Subscribe(infoStream);
       return writer;
    }

    public MetaZipFileSnapshot(string path) : base(path)
    {
    }

    public MetaZipFileSnapshot(string path, long generation) : base(path, generation)
    {
    }

    public void Delete()
    {
        //File.Delete(FilePath);
    }

    public bool Verify()
    {
        return true;
    }
}

public class MetaZipSnapshotReader : ZipSnapshotReader
{
    private readonly InfoStream<MetaZipSnapshotReader> infoStream = new();
    public IInfoStream InfoStream => infoStream;


    public MetaZipSnapshotReader(string path) : base(path)
    {
    }

    public override IEnumerable<IIndexFile> GetIndexFiles()
    {
        EnsureNotDisposed();
        return Archive.Entries
            .Where(entry => entry.FullName.StartsWith("index/"))
            .Select(entry =>
        {
            ManagerIndexFile file = new(entry);
            file.InfoStream.Subscribe(infoStream);
            infoStream.WriteFileOpenEvent(file, $"Restoring file {entry.Name}.", new FileProgress(entry.Length, 0));
            return file;
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Archive.Dispose();
        base.Dispose(disposing);
    }
}

public class ManagerZipSnapshotWriter : ZipSnapshotWriter
{
    private readonly InfoStream<ManagerZipSnapshotWriter> infoStream = new();
    public IInfoStream InfoStream => infoStream;

    private List<string> files = new List<string>();

    public ManagerZipSnapshotWriter(string path) : base(path)
    {
    }
    
    public override Stream OpenStream(string name)
    {
        files.Add( name);
        return base.OpenStream(name);
    }
}