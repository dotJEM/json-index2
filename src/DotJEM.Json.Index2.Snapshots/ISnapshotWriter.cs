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

    Stream OpenOutput(string name);
}

public static class SnapshotWriterExtensions
{
    public static async Task WriteFileAsync(this ISnapshotWriter writer, string fileName, Directory dir)
    {
        using IndexInputStream stream = dir.OpenInputStream(fileName, IOContext.READ_ONCE);
        await writer.CopyFileAsync(fileName, stream);
    }
    
    public static async Task CopyFileAsync(this ISnapshotWriter writer, string fileName, Stream stream)
    {
        using Stream target = writer.OpenOutput(fileName);
        await stream.CopyToAsync(target);
    }
}