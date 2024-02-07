using System;
using System.Linq;
using System.Text;
using DotJEM.Json.Index2.Management.Tracking;

namespace DotJEM.Json.Index2.Management.Info;

public record struct SnapshotRestoreState(SnapshotFileRestoreState[] Files) : ITrackerState
{
    public DateTime StartTime { get; set; } = DateTime.Now;

    public override string ToString()
    {
        return Files
            .OrderBy(f => f.State).ThenBy(f => f.Name)
            .Aggregate(new StringBuilder()
                    .AppendLine($"Restoring {Files.Length} files from snapshot."),
                (sb, state) => sb.AppendLine(state.ToString()))
            .ToString();
    }

}