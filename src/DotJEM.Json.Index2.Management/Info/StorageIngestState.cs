using System;
using System.Linq;
using System.Text;
using DotJEM.Json.Index2.Management.Source;
using DotJEM.Json.Index2.Management.Tracking;
using Newtonsoft.Json;

namespace DotJEM.Json.Index2.Management.Info;

public record struct StorageIngestState(StorageAreaIngestState[] Areas) : ITrackerState
{
    [JsonProperty(Order = -10)]
    public DateTime StartTime => Areas.Min(x => x.StartTime);
    [JsonProperty(Order = -8)]
    public TimeSpan Duration => Areas.Max(x => x.Duration);
    [JsonProperty(Order = -6)]
    public long IngestedCount => Areas.Sum(x => x.IngestedCount);
    [JsonProperty(Order = -4)]
    public GenerationInfo Generation => Areas.Select(x => x.Generation).Aggregate((left, right) => left + right);

    public override string ToString()
    {
        TimeSpan duration = Duration;
        GenerationInfo generation = Generation;
        long count = IngestedCount;
        return Areas.Aggregate(new StringBuilder()
                    .AppendLine($"[{duration:d\\.hh\\:mm\\:ss}] {generation.Current:N0} of {generation.Latest:N0} changes processed, {count:N0} objects indexed. ({count / duration.TotalSeconds:F} / sec)"),
                (sb, state) => sb.AppendLine(state.ToString()))
            .ToString();
    }
}