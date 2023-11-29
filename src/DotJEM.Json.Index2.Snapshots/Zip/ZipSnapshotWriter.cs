using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Snapshots.Streams;
using DotJEM.Json.Index2.Util;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace DotJEM.Json.Index2.Snapshots.Zip
{
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
            this.archive = ZipFile.Open(snapshot.FilePath, ZipArchiveMode.Create);
            this.Snapshot = snapshot;
        }

        public async Task WriteFileAsync(string fileName, Directory dir)
        {
            EnsureNotDisposed();
            using IndexInputStream source = dir.OpenInputStream(fileName, IOContext.READ_ONCE);
            using Stream target = archive.CreateEntry(fileName).Open();
            await source.CopyToAsync(target);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) archive?.Dispose();
            base.Dispose(disposing);
        }
    }
}