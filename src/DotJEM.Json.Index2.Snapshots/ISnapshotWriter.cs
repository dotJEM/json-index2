using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Snapshots.Streams;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace DotJEM.Json.Index2.Snapshots;

public interface ISnapshotWriter : IDisposable
{
    Stream OpenStream(string name);
    Task WriteIndexAsync(IReadOnlyCollection<IIndexFile> files);
}

public static class SnapshotWriterExtensions
{
    public static async Task WriteFileAsync(this ISnapshotWriter writer, string fileName, Directory dir)
    {
        // ReSharper disable once UseAwaitUsing - Not supported on .NET Std 2
        using IndexInputStream stream = dir.OpenInputStream(fileName, IOContext.READ_ONCE);
        await writer.CopyFileAsync(fileName, stream).ConfigureAwait(false);
    }
    
    public static async Task CopyFileAsync(this ISnapshotWriter writer, string fileName, Stream stream)
    {
        // ReSharper disable once UseAwaitUsing - Not supported on .NET Std 2
        using Stream target = writer.OpenStream(fileName);
        await stream.CopyToAsync(target).ConfigureAwait(false);
    }
}