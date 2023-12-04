using System;
using DotJEM.Json.Index2.Snapshots;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Index2.Management.Snapshots.Zip.Meta;

public class ZipFileEvent : InfoStreamEvent
{
    private readonly IIndexFile file;

    public FileEventType EventType { get; }
    public FileProgress Progress { get; }

    public string FileName => file.Name;

    public ZipFileEvent(Type source, InfoLevel level, IIndexFile file, FileEventType eventType, string message, FileProgress progress, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, message, callerMemberName, callerFilePath, callerLineNumber)
    {
        this.file = file;
        EventType = eventType;
        Progress = progress;
    }
}

public readonly struct FileProgress
{
    public long Size { get; }
    public long Copied { get; }

    public FileProgress(long size, long copied)
    {
        Size = size;
        Copied = copied;
    }
}