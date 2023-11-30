using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Index;

namespace DotJEM.Json.Index2.Snapshots.Zip;

public class ZipSnapshotStorage : ISnapshotStorage
{
    private readonly string path;

    public ZipSnapshotStorage(string path)
    {
        this.path = path;
    }
     
    public ISnapshot CreateSnapshot(IndexCommit commit)
    {
        string snapshotPath = Path.Combine(path, $"{commit.Generation:x8}.zip");
        ZipFileSnapshot snapshot = new ZipFileSnapshot(snapshotPath);
        return snapshot;
    }

    public IEnumerable<ISnapshot> LoadSnapshots()
    {
        return Directory.EnumerateFiles(path, "*.zip")
            .Select(file => new ZipFileSnapshot(file))
            .OrderByDescending(f => f.Generation);
    }

}