using System;
using System.Globalization;
using System.IO;
using DotJEM.Json.Index2.Util;

namespace DotJEM.Json.Index2.Snapshots.Zip
{
    public class ZipFileSnapshot : Disposable,  ISnapshot
    {
        public long Generation { get; }
        public string FilePath { get; }

        private readonly Lazy<ISnapshotReader> snapshotReader;
        private readonly Lazy<ISnapshotWriter> snapshotWriter;

        public ISnapshotReader OpenReader()
        {
            EnsureNotDisposed();
            return snapshotReader.Value;
        }

        public ISnapshotWriter OpenWriter()
        {
            EnsureNotDisposed();
            return snapshotWriter.Value;
        }

        public ZipFileSnapshot(string path) 
            : this(path, long.Parse(Path.GetFileNameWithoutExtension(path), NumberStyles.AllowHexSpecifier))
        {
        }

        public ZipFileSnapshot(string path, long generation, bool isReadonly = false)
        {
            FilePath = path;
            Generation = generation;

            this.snapshotReader = new Lazy<ISnapshotReader>(() => new ZipSnapshotReader(this));
            this.snapshotWriter = isReadonly 
                ? new Lazy<ISnapshotWriter>(() => throw new InvalidOperationException()) 
                : new Lazy<ISnapshotWriter>(() => new ZipSnapshotWriter(this));
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing || disposed) 
                return;
            base.Dispose(true);

            if(snapshotReader.IsValueCreated)
                snapshotReader.Value.Dispose();

            if(snapshotWriter.IsValueCreated)
                snapshotWriter.Value.Dispose();
        }
    }
}