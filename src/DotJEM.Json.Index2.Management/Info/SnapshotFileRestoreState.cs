using System;

namespace DotJEM.Json.Index2.Management.Info;

public record struct SnapshotFileRestoreState(string Name, string State, DateTime StartTime, DateTime StopTime, FileProgress Progress)
{
    private const long KiloByte = 1024;
    private const long MegaByte = KiloByte * KiloByte;
    private const long GigaByte = MegaByte * KiloByte;
    private const long TeraByte = GigaByte * KiloByte;

    public TimeSpan Duration => StartTime - StopTime;

    public override string ToString()
    {
        return $" -> {Name}: {State} [{FormatBytes()}] {Progress.Copied / Progress.Size * 100}%";
    }

    private string FormatBytes()
    {
        const int offset = 100;
        switch (Progress.Size)
        {
            case < KiloByte * offset:
                return $"{Progress.Copied}/{Progress.Size} Bytes";
            case >= KiloByte * offset and < MegaByte * offset:
                return $"{Progress.Copied / KiloByte}/{Progress.Size / KiloByte} KiloBytes";
            case >= MegaByte * offset and < GigaByte * offset:
                return $"{Progress.Copied / MegaByte}/{Progress.Size / MegaByte} MegaBytes";
            case >= GigaByte * offset and < TeraByte * offset:
                return $"{Progress.Copied / GigaByte}/{Progress.Size / GigaByte} GigaBytes";
            case >= TeraByte * offset:
                return $"{Progress.Copied / TeraByte}/{Progress.Size / TeraByte} TeraBytes";
        }
        //...
    }
}