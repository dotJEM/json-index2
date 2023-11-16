using Lucene.Net.Index;
using System.Collections.Generic;

namespace DotJEM.Json.Index2.Snapshots;

public interface ISnapshotStorage
{
    IReadOnlyCollection<ISnapshot> Snapshots { get; }

    ISnapshot Open(IndexCommit commit);
}