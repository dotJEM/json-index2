using System.Runtime.CompilerServices;
using DotJEM.Json.Index2.Snapshots;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management.Info;

/// <summary>
/// 
/// </summary>
public static class SnapshotInfoStreamExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="self"></param>
    /// <param name="snapshot"></param>
    /// <param name="message"></param>
    /// <param name="callerMemberName"></param>
    /// <param name="callerFilePath"></param>
    /// <param name="callerLineNumber"></param>
    public static void WriteSnapshotOpenEvent<TSource>(this IInfoStream<TSource> self, ISnapshot snapshot, string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        => self.WriteEvent(new SnapshotOpenedEvent(typeof(TSource), InfoLevel.INFO, snapshot,  message, callerMemberName, callerFilePath, callerLineNumber));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="self"></param>
    /// <param name="snapshot"></param>
    /// <param name="message"></param>
    /// <param name="callerMemberName"></param>
    /// <param name="callerFilePath"></param>
    /// <param name="callerLineNumber"></param>
    public static void WriteSnapshotCloseEvent<TSource>(this IInfoStream<TSource> self, ISnapshot snapshot, string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        => self.WriteEvent(new SnapshotClosedEvent(typeof(TSource), InfoLevel.INFO, snapshot,  message, callerMemberName, callerFilePath, callerLineNumber));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="self"></param>
    /// <param name="snapshot"></param>
    /// <param name="message"></param>
    /// <param name="callerMemberName"></param>
    /// <param name="callerFilePath"></param>
    /// <param name="callerLineNumber"></param>
    public static void WriteSnapshotCreatedEvent<TSource>(this IInfoStream<TSource> self, ISnapshot snapshot, string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        => self.WriteEvent(new SnapshotCreatedEvent(typeof(TSource), InfoLevel.INFO, snapshot, message, callerMemberName, callerFilePath, callerLineNumber));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="self"></param>
    /// <param name="snapshot"></param>
    /// <param name="message"></param>
    /// <param name="callerMemberName"></param>
    /// <param name="callerFilePath"></param>
    /// <param name="callerLineNumber"></param>
    public static void WriteSnapshotDeletedEvent<TSource>(this IInfoStream<TSource> self, ISnapshot snapshot, string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        => self.WriteEvent(new SnapshotDeletedEvent(typeof(TSource), InfoLevel.INFO, snapshot, message, callerMemberName, callerFilePath, callerLineNumber));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="self"></param>
    /// <param name="file"></param>
    /// <param name="message"></param>
    /// <param name="progress"></param>
    /// <param name="callerMemberName"></param>
    /// <param name="callerFilePath"></param>
    /// <param name="callerLineNumber"></param>
    public static void WriteFileOpenEvent<TSource>(this IInfoStream<TSource> self, IIndexFile file, string message, FileProgress progress, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        => self.WriteEvent(new FileOpenedInfoEvent(typeof(TSource), InfoLevel.INFO, file, progress, message, callerMemberName, callerFilePath, callerLineNumber));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="self"></param>
    /// <param name="file"></param>
    /// <param name="message"></param>
    /// <param name="progress"></param>
    /// <param name="callerMemberName"></param>
    /// <param name="callerFilePath"></param>
    /// <param name="callerLineNumber"></param>
    public static void WriteFileCloseEvent<TSource>(this IInfoStream<TSource> self, IIndexFile file, string message, FileProgress progress, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        => self.WriteEvent(new FileClosedInfoEvent(typeof(TSource), InfoLevel.INFO, file, progress, message,  callerMemberName, callerFilePath, callerLineNumber));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="self"></param>
    /// <param name="file"></param>
    /// <param name="message"></param>
    /// <param name="progress"></param>
    /// <param name="callerMemberName"></param>
    /// <param name="callerFilePath"></param>
    /// <param name="callerLineNumber"></param>
    public static void WriteFileProgressEvent<TSource>(this IInfoStream<TSource> self, IIndexFile file, string message, FileProgress progress, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        => self.WriteEvent(new FileProgressInfoEvent(typeof(TSource), InfoLevel.INFO, file, progress, message, callerMemberName, callerFilePath, callerLineNumber));

}