using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DotJEM.Json.Index2.Util;

namespace DotJEM.Json.Index2.Snapshots.Zip;

public class ZipSnapshotReader : Disposable, ISnapshotReader
{
    private readonly ZipArchive archive;

    public IReadOnlyCollection<string> FileNames { get; }

    public ZipSnapshotReader(string path)
    {
        this.archive = ZipFile.Open(path, ZipArchiveMode.Read);
        this.FileNames = archive.Entries.Select(entry => entry.Name).ToList();
    }

    public virtual Stream OpenStream(string fileName)
    {
        EnsureNotDisposed();
        ZipArchiveEntry entry = archive.GetEntry(fileName);
        if (entry == null)
            throw new FileNotFoundException("", fileName);
        return archive.GetEntry(fileName).Open();
    }

    public virtual IEnumerable<IIndexFile> GetIndexFiles()
    {
        EnsureNotDisposed();
        return archive.Entries.Select(entry => new IndexFile(entry.Name, ()=>OpenStream(entry.Name)));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            archive.Dispose();
        base.Dispose(disposing);
    }
}