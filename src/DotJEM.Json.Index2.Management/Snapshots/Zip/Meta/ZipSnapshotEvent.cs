﻿using System;
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
