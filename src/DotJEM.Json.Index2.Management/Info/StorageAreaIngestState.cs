using System;
using DotJEM.Json.Index2.Management.Source;

namespace DotJEM.Json.Index2.Management.Info;

public record StorageAreaIngestState(
    string Area,
    DateTime StartTime,
    TimeSpan Duration,
    long IngestedCount,
    GenerationInfo Generation,
    JsonSourceEventType LastEvent,
    long UpdatedCount,
    long UpdateCycles,
    TimeSpan TotalUpdateDuration,
    TimeSpan LastUpdateDuration,
    long BytesLoaded)
{
    public override string ToString()
    {
        switch (LastEvent)
        {
            case JsonSourceEventType.Starting:
            case JsonSourceEventType.Initializing:
            case JsonSourceEventType.Initialized:
                return $" -> [{LastEvent}:{Duration:hh\\:mm\\:ss}] {Area} {Generation.Current:N0} of {Generation.Latest:N0} changes processed:" + Environment.NewLine +
                       $"    {IngestedCount + UpdatedCount:N0} objects indexed." + Environment.NewLine +
                       $"    {IngestedCount / Duration.TotalSeconds:F} / sec " + Environment.NewLine +
                       $"    {FormatBytes(BytesLoaded)}";
            case JsonSourceEventType.Updating:
            case JsonSourceEventType.Updated:
            case JsonSourceEventType.Stopped:
                return $" -> [{LastEvent}:{LastUpdateDuration.TotalMilliseconds}ms] {Area} {Generation.Current:N0} of {Generation.Latest:N0} changes processed:" + Environment.NewLine +
                       $"    {IngestedCount + UpdatedCount:N0} objects indexed." + Environment.NewLine +
                       $"    Update cycle (avg): {UpdateCycles} ({TotalUpdateDuration.TotalMilliseconds / Math.Max(1, UpdateCycles):##.000}ms)" + Environment.NewLine +
                       $"    {FormatBytes(BytesLoaded)}";
        }

        return "???";
    }

    private const long KiloByte = 1024;
    private const long MegaByte = KiloByte * KiloByte;
    private const long GigaByte = MegaByte * KiloByte;
    private const long TeraByte = GigaByte * KiloByte;
    private string FormatBytes(long amount)
    {
        const int offset = 10;
        switch (amount)
        {
            case < KiloByte * offset:
                return $"{amount} Bytes";
            case >= KiloByte * offset and < MegaByte * offset:
                return $"{amount / KiloByte} KiloBytes";
            case >= MegaByte * offset and < GigaByte * offset:
                return $"{amount / MegaByte} MegaBytes";
            case >= GigaByte * offset and < TeraByte * offset:
                return $"{amount / GigaByte} GigaBytes";
            case >= TeraByte * offset:
                return $"{amount / TeraByte} TeraBytes";
        }
    }

}