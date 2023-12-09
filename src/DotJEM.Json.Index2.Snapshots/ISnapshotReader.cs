using System;
using System.Collections.Generic;
using System.IO;

namespace DotJEM.Json.Index2.Snapshots
{
    public interface ISnapshotReader : IDisposable
    {
        IReadOnlyCollection<string> FileNames { get; }
        Stream OpenStream(string fileName);
        IEnumerable<IIndexFile> GetIndexFiles();
    }
}