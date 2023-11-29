using System;
using System.IO;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Snapshots.Streams;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace DotJEM.Json.Index2.Snapshots;

public interface ISnapshotWriter : IDisposable
{
    ISnapshot Snapshot { get; }
    Task WriteFileAsync(string fileName, Stream stream);
}

public static class SnapshotWriterExtensions
{
    public static async Task WriteFileAsync(this ISnapshotWriter writer, string fileName, Directory dir)
    {
        using IndexInputStream stream = dir.OpenInputStream(fileName, IOContext.READ_ONCE);
        await writer.WriteFileAsync(fileName, stream);
    }
}