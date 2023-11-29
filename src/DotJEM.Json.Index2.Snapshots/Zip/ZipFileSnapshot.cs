using System;
using System.Globalization;
using System.IO;

namespace DotJEM.Json.Index2.Snapshots.Zip;

public class ZipFileSnapshot : ISnapshot
{
    public long Generation { get; }
    public string FilePath { get; }

    public ISnapshotReader OpenReader() => new ZipSnapshotReader(this);

    public ISnapshotWriter OpenWriter() => new ZipSnapshotWriter(this);
    public ZipFileSnapshot(string path)
        : this(path, long.Parse(Path.GetFileNameWithoutExtension(path), NumberStyles.AllowHexSpecifier))
    {
    }

    public ZipFileSnapshot(string path, long generation)
    {
        FilePath = path;
        Generation = generation;
    }

    public void Delete()
    {
        File.Delete(FilePath);
    }

}
