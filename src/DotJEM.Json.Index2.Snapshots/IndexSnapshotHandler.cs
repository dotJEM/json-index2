using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Snapshots.Streams;
using DotJEM.Json.Index2.Snapshots.Zip;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace DotJEM.Json.Index2.Snapshots;

public interface ISnapshot : IDisposable
{
    long Generation { get; }
    ISnapshotReader OpenReader();
    ISnapshotWriter OpenWriter();
}

public interface IIndexSnapshotHandler
{
    Task<ISnapshot> TakeSnapshotAsync(IJsonIndex index, ISnapshotStorage storage, bool leaveOpen = false);
    Task<ISnapshot> RestoreSnapshotAsync(IJsonIndex index, ISnapshot source, bool leaveOpen = false);
}

public class IndexSnapshotHandler : IIndexSnapshotHandler
{
    public async Task<ISnapshot> TakeSnapshotAsync(IJsonIndex index, ISnapshotStorage storage, bool leaveOpen = false)
    {
        IndexWriter writer = index.WriterManager.Writer;
        SnapshotDeletionPolicy sdp = writer.Config.IndexDeletionPolicy as SnapshotDeletionPolicy;
        if (sdp == null)
            throw new InvalidOperationException("Index must use an implementation of the SnapshotDeletionPolicy.");

        IndexCommit commit = null;
        try
        {
            commit = sdp.Snapshot();
            Directory dir = commit.Directory;

            ISnapshot snapshot = storage.CreateSnapshot(commit);
            ISnapshotWriter snapshotWriter = snapshot.OpenWriter();
            foreach (string fileName in commit.FileNames)
                await snapshotWriter.WriteFileAsync(fileName, dir);
            
            if (leaveOpen)
                return snapshot;

            snapshot.Dispose();
            return snapshot;
        }
        finally
        {
            if (commit != null)
            {
                sdp.Release(commit);
            }
        }
    }

    public async Task<ISnapshot> RestoreSnapshotAsync(IJsonIndex index, ISnapshot snapshot, bool leaveOpen = false)
    {
        index.Storage.Delete();
        Directory dir = index.Storage.Directory;
        using ISnapshotReader reader = snapshot.OpenReader();

        ISnapshotFile segmentsFile = null;
        List<string> files = new();
        foreach (ISnapshotFile file in reader.ReadFiles())
        {
            if (Regex.IsMatch(file.Name, "^" + IndexFileNames.SEGMENTS + "_.*$"))
            {
                segmentsFile = file;
                continue;
            }

            using IndexOutputStream output = dir.CreateOutputStream(file.Name, IOContext.DEFAULT);
            using Stream sourceStream = file.Open();
            await sourceStream.CopyToAsync(output);
            files.Add(file.Name);
        }
        dir.Sync(files);

        if (segmentsFile == null)
            throw new ArgumentException();

        using IndexOutputStream segOutput = dir.CreateOutputStream(segmentsFile.Name, IOContext.DEFAULT);
        using Stream segmentsSourceStream = segmentsFile.Open();
        await segmentsSourceStream.CopyToAsync(segOutput);
        segOutput.Dispose();
        segmentsSourceStream.Dispose();
        dir.Sync(new [] { segmentsFile.Name });

        SegmentInfos.WriteSegmentsGen(dir, reader.Snapshot.Generation);

        //NOTE: (jmd 2020-09-30) Not quite sure what this does at this time, but the Lucene Replicator does it so better keep it for now.
        IndexCommit last = DirectoryReader.ListCommits(dir).Last();
        if (last != null)
        {
            ISet<string> commitFiles = new HashSet<string>(last.FileNames);
            commitFiles.Add(IndexFileNames.SEGMENTS_GEN);
        }
        index.WriterManager.Close();
        return reader.Snapshot;
    }
}