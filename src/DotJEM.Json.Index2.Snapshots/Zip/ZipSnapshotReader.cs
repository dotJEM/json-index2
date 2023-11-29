using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DotJEM.Json.Index2.Util;

namespace DotJEM.Json.Index2.Snapshots.Zip
{
    public class ZipSnapshotReader : Disposable, ISnapshotReader
    {
        private readonly ZipArchive archive;
        public ISnapshot Snapshot { get; }
        
        public ZipSnapshotReader(string path)
            :this(new ZipFileSnapshot(path))
        {
        }

        public ZipSnapshotReader(ZipFileSnapshot snapshot)
        {
            this.Snapshot = snapshot;
            this.archive = ZipFile.Open(snapshot.FilePath, ZipArchiveMode.Read);
        }

        public IEnumerable<ISnapshotFile> ReadFiles()
        {
            EnsureNotDisposed();
            return archive.Entries.Select(entry => new SnapshotFile(entry.Name, entry.Open));
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                archive.Dispose();
            base.Dispose(disposing);
        }

    }
}