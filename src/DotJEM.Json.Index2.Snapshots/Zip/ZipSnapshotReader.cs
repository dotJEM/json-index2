using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DotJEM.Json.Index2.Util;

namespace DotJEM.Json.Index2.Snapshots.Zip;

public class ZipSnapshotReader : Disposable, ISnapshotReader
{
    protected ZipArchive Archive { get; }

    public IReadOnlyCollection<string> FileNames { get; }

    public ZipSnapshotReader(string path)
    {
        this.Archive = ZipFile.Open(path, ZipArchiveMode.Read);
        this.FileNames = Archive.Entries.Select(entry => entry.Name).ToList();
    }

    public virtual Stream OpenStream(string fileName)
    {
        EnsureNotDisposed();
        ZipArchiveEntry entry = Archive.GetEntry(fileName);
        if (entry == null)
            throw new FileNotFoundException("", fileName);
        return Archive.GetEntry(fileName).Open();
    }

    public virtual IEnumerable<IIndexFile> GetIndexFiles()
    {
        EnsureNotDisposed();
        return Archive.Entries
            .Where(entry => entry.Name.StartsWith("index/"))
            .Select(entry => new IndexFile(entry.Name.Remove(6), ()=>OpenStream(entry.Name)));


    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Archive.Dispose();
        base.Dispose(disposing);
    }
}