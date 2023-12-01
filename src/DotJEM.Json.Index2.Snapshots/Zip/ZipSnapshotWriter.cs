using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Util;

namespace DotJEM.Json.Index2.Snapshots.Zip;

public class ZipSnapshotWriter : Disposable, ISnapshotWriter
{
    private readonly ZipArchive archive;

    public ISnapshot Snapshot { get; }

    public ZipSnapshotWriter(string path)
        : this(new ZipFileSnapshot(path))
    {
    }

    public ZipSnapshotWriter(ZipFileSnapshot snapshot)
    {
        this.archive = ZipFile.Open(snapshot.FilePath, File.Exists(snapshot.FilePath) ? ZipArchiveMode.Update : ZipArchiveMode.Create);
        this.Snapshot = snapshot;
    }

    public Stream OpenOutput(string name)
    {
        EnsureNotDisposed();
        return archive.CreateEntry(name).Open();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) archive?.Dispose();
        base.Dispose(disposing);
    }
}