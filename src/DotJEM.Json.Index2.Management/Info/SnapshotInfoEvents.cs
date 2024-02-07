using System;
using DotJEM.Json.Index2.Snapshots;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management.Info;

public abstract class SnapshotInfoEvent : InfoStreamEvent
{
    public ISnapshot Snapshot { get; }

    protected SnapshotInfoEvent(Type source, InfoLevel level, ISnapshot snapshot, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, message, callerMemberName, callerFilePath, callerLineNumber)
    {
        Snapshot = snapshot;
    }
}

public class SnapshotOpenedEvent : SnapshotInfoEvent
{
    public SnapshotOpenedEvent(Type source, InfoLevel level, ISnapshot snapshot, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, snapshot, message, callerMemberName, callerFilePath, callerLineNumber)
    {
    }
}

public class SnapshotClosedEvent : SnapshotInfoEvent
{
    public SnapshotClosedEvent(Type source, InfoLevel level, ISnapshot snapshot, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, snapshot, message, callerMemberName, callerFilePath, callerLineNumber)
    {
    }
}

public class SnapshotCreatedEvent : SnapshotInfoEvent
{
    public SnapshotCreatedEvent(Type source, InfoLevel level, ISnapshot snapshot, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, snapshot, message, callerMemberName, callerFilePath, callerLineNumber)
    {
    }
}
public class SnapshotDeletedEvent : SnapshotInfoEvent
{
    public SnapshotDeletedEvent(Type source, InfoLevel level, ISnapshot snapshot, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, snapshot, message, callerMemberName, callerFilePath, callerLineNumber)
    {
    }
}
