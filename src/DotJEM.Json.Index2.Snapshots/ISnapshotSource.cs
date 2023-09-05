using System.Collections.Generic;

namespace DotJEM.Json.Index2.Snapshots
{
    public interface ISnapshotSource
    {
        IReadOnlyCollection<ISnapshot> Snapshots { get; }
        ISnapshotReader Open();
    }
}