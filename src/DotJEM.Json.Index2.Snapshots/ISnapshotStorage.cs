using Lucene.Net.Index;
using System.Collections.Generic;

namespace DotJEM.Json.Index2.Snapshots;

public interface ISnapshotStorage
{
    ISnapshot CreateSnapshot(IndexCommit commit);
    IEnumerable<ISnapshot> loadSnapshots();
}