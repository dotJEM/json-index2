using System;
using System.Collections.Generic;

namespace DotJEM.Json.Index2.Snapshots
{
    public interface ISnapshotReader : IDisposable
    {
        ISnapshot Snapshot { get; }

        IEnumerable<ISnapshotFile> ReadFiles();
    }
}