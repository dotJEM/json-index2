using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotJEM.Json.Index2.Util;

namespace DotJEM.Json.Index2.Snapshots.Zip;

public class ZipSnapshotWriter : Disposable, ISnapshotWriter
{
    protected ZipArchive Archive { get; }

    public ZipSnapshotWriter(string path)
    {
        this.Archive = ZipFile.Open(path, File.Exists(path)
            ? ZipArchiveMode.Update : ZipArchiveMode.Create);
    }

    public virtual Stream OpenStream(string name)
    {
        EnsureNotDisposed();
        return Archive.CreateEntry(name).Open();
    }

    public virtual async Task WriteIndexAsync(IReadOnlyCollection<IIndexFile> files)
    {
        EnsureNotDisposed();
        foreach (IIndexFile file in files)
        {
            using Stream input = file.Open();
            using Stream output = Archive.CreateEntry($"index/{file.Name}").Open();
            await input.CopyToAsync(output).ConfigureAwait(false);

        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) Archive?.Dispose();
        base.Dispose(disposing);
    }
}