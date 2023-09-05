using System.Collections.Generic;

namespace DotJEM.Json.Index2.Snapshots
{
    public interface ISnapshotTarget
    {
        IReadOnlyCollection<ISnapshot> Snapshots { get; }
        ISnapshotWriter Open(long generation);
    }
}