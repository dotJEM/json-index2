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
    public string FileName { get; }
    public string FilePath { get; }

    public bool Exists => File.Exists(FilePath);

    public virtual ISnapshotReader OpenReader() => new ZipSnapshotReader(this.FilePath);

    public virtual ISnapshotWriter OpenWriter() => new ZipSnapshotWriter(this.FilePath);

    public ZipFileSnapshot(string path)
        : this(path, long.Parse(Path.GetFileNameWithoutExtension(path), NumberStyles.AllowHexSpecifier))
    {
    }

    public ZipFileSnapshot(string path, long generation)
    {
        FilePath = path;
        Generation = generation;
        FileName = Path.GetFileName(path);
    }

    public virtual void Delete()
    {
        File.Delete(FilePath);
    }

    public virtual bool Verify()
    {
        try
        {
            using var zipFile = ZipFile.OpenRead(FilePath);
            ReadOnlyCollection<ZipArchiveEntry> _ = zipFile.Entries;
            return true;
        }
        catch (InvalidDataException ex)
        {
            return false;
        }
    }

    public override string ToString()
        => FileName;
}
