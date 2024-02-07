using System;
using DotJEM.Json.Index2.Management.Tracking;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management.Info;

public class TrackerStateInfoStreamEvent : InfoStreamEvent
{
    public ITrackerState State { get; }

    public TrackerStateInfoStreamEvent(Type source, InfoLevel level, ITrackerState state, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, message, callerMemberName, callerFilePath, callerLineNumber)
    {
        State = state;
    }
}