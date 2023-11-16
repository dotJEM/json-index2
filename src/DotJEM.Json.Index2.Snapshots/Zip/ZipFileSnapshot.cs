using System.Globalization;
using System.IO;

namespace DotJEM.Json.Index2.Snapshots.Zip
{
    public class ZipFileSnapshot : ISnapshot
    {
        public long Generation { get; }
        public string FilePath { get; }

        public ISnapshotReader OpenReader()
        {
            return new ZipSnapshotReader(FilePath);
        }

        public ISnapshotWriter OpenWriter()
        {
            return new ZipSnapshotWriter(FilePath);
        }

        public ZipFileSnapshot(string path) 
            : this(path, long.Parse(Path.GetFileNameWithoutExtension(path), NumberStyles.AllowHexSpecifier))
        {
        }

        public ZipFileSnapshot(string path, long generation)
        {
            FilePath = path;
            Generation = generation;
        }
    }
}