using Lucene.Net.Index;
using System.Collections.Generic;

namespace DotJEM.Json.Index2.Snapshots
{
    public interface ISnapshotTarget
    {
        IReadOnlyCollection<ISnapshot> Snapshots { get; }
        ISnapshotWriter Open(IndexCommit commit);
    }
}