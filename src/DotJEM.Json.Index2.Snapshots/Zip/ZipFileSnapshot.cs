using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

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

    public bool Verify()
    {
        try
        {
            using var zipFile = ZipFile.OpenRead(FilePath);
            ReadOnlyCollection<ZipArchiveEntry> _ = zipFile.Entries;
            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }
}
