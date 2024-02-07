using System;
using DotJEM.Json.Index2.Snapshots;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management.Info;

public abstract class FileInfoEvent : InfoStreamEvent
{
    public IIndexFile File { get; }
    public FileProgress Progress { get; }
    public string FileName => File.Name;

    protected FileInfoEvent(Type source, InfoLevel level, IIndexFile file, FileProgress progress, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, message, callerMemberName, callerFilePath, callerLineNumber)
    {
        File = file;
        Progress = progress;
    }
}

public class FileOpenedInfoEvent : FileInfoEvent
{
    public FileOpenedInfoEvent(Type source, InfoLevel level, IIndexFile file, FileProgress progress, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, file, progress, message, callerMemberName, callerFilePath, callerLineNumber)
    {
    }
}

public class FileProgressInfoEvent : FileInfoEvent
{
    public FileProgressInfoEvent(Type source, InfoLevel level, IIndexFile file, FileProgress progress, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, file, progress, message, callerMemberName, callerFilePath, callerLineNumber)
    {
    }
}

public class FileClosedInfoEvent : FileInfoEvent
{
    public FileClosedInfoEvent(Type source, InfoLevel level, IIndexFile file, FileProgress progress, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        : base(source, level, file, progress, message, callerMemberName, callerFilePath, callerLineNumber)
    {
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