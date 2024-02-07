using System;
using DotJEM.Json.Index2.Snapshots;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management.Snapshots.Zip.Meta;

public class ZipSnapshotEvent : InfoStreamEvent
{
    private readonly MetaZipFileSnapshot snapshot;

    public FileEventType EventType { get; }

    //public IEnumerable<string> SnapshotFiles => snapshot.Files.Select(file => file.Name);

    public ZipSnapshotEvent(Type source, InfoLevel level, MetaZipFileSnapshot snapshot, FileEventType eventType, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, message, callerMemberName, callerFilePath, callerLineNumber)
    {
        this.snapshot = snapshot;
        EventType = eventType;
    }
}
public class SnapshotCreatedEvent : InfoStreamEvent
{
    public ISnapshot Snapshot { get; }

    public FileEventType EventType { get; }

    public SnapshotCreatedEvent(Type source, InfoLevel level, ISnapshot snapshot, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, message, callerMemberName, callerFilePath, callerLineNumber)
    {
        this.Snapshot = snapshot;
    }
}
public class SnapshotDeletedEvent : InfoStreamEvent
{
    public ISnapshot Snapshot { get; }

    public FileEventType EventType { get; }

    public SnapshotCreatedEvent(Type source, InfoLevel level, ISnapshot snapshot, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, message, callerMemberName, callerFilePath, callerLineNumber)
    {
        this.Snapshot = snapshot;
    }
}
