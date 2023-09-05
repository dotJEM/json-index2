﻿using System.IO;
using System.IO.Compression;
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
        {
            this.archive = ZipFile.Open(path, ZipArchiveMode.Create);
            this.Snapshot = new ZipFileSnapshot(path);
        }

        public void WriteFile(string fileName, Directory dir)
        {
            using IndexInputStream source = new IndexInputStream(dir.OpenInput(fileName, IOContext.READ_ONCE));
            using Stream target = archive.CreateEntry(fileName).Open();
            source.CopyTo(target);
        }

        public void WriteSegmentsFile(string segmentsFile, Directory dir)
        {
            this.WriteFile(segmentsFile, dir);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) archive?.Dispose();
            base.Dispose(disposing);
        }
    }
}