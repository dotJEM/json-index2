using System;
using System.Threading.Tasks;
using Lucene.Net.Store;

namespace DotJEM.Json.Index2.Snapshots
{
    public interface ISnapshotWriter : IDisposable
    {
        ISnapshot Snapshot { get; }
        Task WriteFileAsync(string fileName, Directory dir);
    }
}