using DotJEM.Json.Index2.Snapshots;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Index2.Management.Snapshots;

public interface ISnapshotStrategy
{
    IInfoStream InfoStream { get; }
    ISnapshotStorage Storage { get; }
    void CleanOldSnapshots();
}