using System;
using Lucene.Net.Store;

namespace DotJEM.Json.Index2.Snapshots
{
    public interface ISnapshotWriter : IDisposable
    {
        ISnapshot Snapshot { get; }

        void WriteFile(string fileName, Directory dir);
        void WriteSegmentsFile(string segmentsFile, Directory dir);
    }
}