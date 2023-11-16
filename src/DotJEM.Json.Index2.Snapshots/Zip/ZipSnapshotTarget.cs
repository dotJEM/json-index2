using System.Collections.Generic;
using System.IO;
using Lucene.Net.Index;

namespace DotJEM.Json.Index2.Snapshots.Zip
{
    public class ZipSnapshotTarget : ISnapshotTarget
    {
        private readonly string path;
        private readonly List<ZipFileSnapshot> snapShots = new List<ZipFileSnapshot>();

        public IReadOnlyCollection<ISnapshot> Snapshots => snapShots.AsReadOnly(); 

        public ZipSnapshotTarget(string path)
        {
            this.path = path;
        }

        public ISnapshotWriter Open(IndexCommit generation)
        {
            string snapshotPath = Path.Combine(path, $"{generation:x8}.zip");
            snapShots.Add(new ZipFileSnapshot(snapshotPath));
            return new ZipSnapshotWriter(snapshotPath);
        }
    }
}