using System;
using System.Collections.Generic;

namespace DotJEM.Json.Index2.Snapshots
{
    public interface ISnapshotReader : IDisposable, IEnumerable<ILuceneFile>
    {
        ISnapshot Snapshot { get; }
    }
}