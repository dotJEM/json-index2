using System;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management.Info;

public class StorageIngestStateInfoStreamEvent : InfoStreamEvent
{
    public StorageIngestState State { get; }

    public StorageIngestStateInfoStreamEvent(Type source, InfoLevel level, StorageIngestState state, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, message, callerMemberName, callerFilePath, callerLineNumber)
    {
        State = state;
    }
}