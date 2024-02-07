using System.Runtime.CompilerServices;
using DotJEM.Json.Index2.Management.Tracking;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management.Info;

public static class IndexManagerInfoStreamExtensions
{
    public static void WriteJsonSourceEvent<TSource>(this IInfoStream<TSource> self, JsonSourceEventType eventType, string area, string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
    {
        self.WriteEvent(new StorageObserverInfoStreamEvent(typeof(TSource), InfoLevel.INFO, eventType, area, message, callerMemberName, callerFilePath, callerLineNumber));
    }

    public static void WriteStorageIngestStateEvent<TSource>(this IInfoStream<TSource> self, StorageIngestState state, string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
    {
        self.WriteEvent(new StorageIngestStateInfoStreamEvent(typeof(TSource), InfoLevel.INFO, state, message, callerMemberName, callerFilePath, callerLineNumber));
    }

    public static void WriteTrackerStateEvent<TSource>(this IInfoStream<TSource> self, ITrackerState state, string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
    {
        self.WriteEvent(new TrackerStateInfoStreamEvent(typeof(TSource), InfoLevel.INFO, state, message, callerMemberName, callerFilePath, callerLineNumber));

    }
}