using System;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management.Info;

public class StorageObserverInfoStreamEvent : InfoStreamEvent
{
    public string Area { get; }
    public JsonSourceEventType EventType { get; }

    public StorageObserverInfoStreamEvent(Type source, InfoLevel level, JsonSourceEventType eventType, string area, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, message, callerMemberName, callerFilePath, callerLineNumber)
    {
        EventType = eventType;
        Area = area;
    }

    public override string ToString()
    {
        return $"[{Level}] {Area}:{EventType}:{Message} ({Source} {CallerMemberName} - {CallerFilePath}:{CallerLineNumber})";
    }
}