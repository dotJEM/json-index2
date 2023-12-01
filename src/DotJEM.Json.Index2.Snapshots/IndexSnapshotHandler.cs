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

public interface ISnapshot
{
    long Generation { get; }
    ISnapshotReader OpenReader();
    ISnapshotWriter OpenWriter();
    void Delete();
    bool Verify();
}

public interface IIndexSnapshotHandler
{
    Task<ISnapshot> TakeSnapshotAsync(IJsonIndex index, ISnapshotStorage storage);
    Task<bool> RestoreSnapshotAsync(IJsonIndex index, ISnapshot source);
    Task<ISnapshot> RestoreSnapshotFromAsync(IJsonIndex index, ISnapshotStorage storage);
}

public class IndexSnapshotHandler : IIndexSnapshotHandler
{
    //public async Task<ISnapshot> TakeSnapshotAsync(IJsonIndex index, ISnapshot snapshot)
    //{

    //}

    public async Task<ISnapshot> TakeSnapshotAsync(IJsonIndex index, ISnapshotStorage storage)
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
            using ISnapshotWriter snapshotWriter = snapshot.OpenWriter();
            foreach (string fileName in commit.FileNames)
                await snapshotWriter.WriteFileAsync(fileName, dir);
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




    public async Task<bool> RestoreSnapshotAsync(IJsonIndex index, ISnapshot snapshot)
    {
        index.Storage.Delete();
        Directory dir = index.Storage.Directory;
        return await UnpackSnapshotAsync(index, snapshot, dir);
        
        //using ISnapshotReader reader = snapshot.OpenReader();

        //ISnapshotFile segmentsFile = null;
        //List<string> files = new();
        //foreach (ISnapshotFile file in reader.ReadFiles())
        //{
        //    if (Regex.IsMatch(file.Name, "^" + IndexFileNames.SEGMENTS + "_.*$"))
        //    {
        //        segmentsFile = file;
        //        continue;
        //    }

        //    using IndexOutputStream output = dir.CreateOutputStream(file.Name, IOContext.DEFAULT);
        //    using Stream sourceStream = file.Open();
        //    await sourceStream.CopyToAsync(output);
        //    files.Add(file.Name);
        //}
        //dir.Sync(files);

        //if (segmentsFile == null)
        //    throw new ArgumentException("Snapshot did not contain any segments file.", nameof(snapshot));

        //using IndexOutputStream segOutput = dir.CreateOutputStream(segmentsFile.Name, IOContext.DEFAULT);
        //using Stream segmentsSourceStream = segmentsFile.Open();
        //await segmentsSourceStream.CopyToAsync(segOutput);
        //segOutput.Dispose();
        //segmentsSourceStream.Dispose();
        //dir.Sync(new [] { segmentsFile.Name });

        //SegmentInfos.WriteSegmentsGen(dir, snapshot.Generation);

        ////NOTE: (jmd 2020-09-30) Not quite sure what this does at this time, but the Lucene Replicator does it so better keep it for now.
        //IndexCommit last = DirectoryReader.ListCommits(dir).Last();
        //if (last != null)
        //{
        //    ISet<string> commitFiles = new HashSet<string>(last.FileNames);
        //    commitFiles.Add(IndexFileNames.SEGMENTS_GEN);
        //}
        //index.WriterManager.Close();
        //return true;
    }

    public async Task<ISnapshot> RestoreSnapshotFromAsync(IJsonIndex index, ISnapshotStorage storage)
    {
        index.Storage.Delete();
        Directory dir = index.Storage.Directory;
        foreach (ISnapshot snapshot in storage.LoadSnapshots())
        {
            try
            {
                if(await UnpackSnapshotAsync(index, snapshot, dir))
                    return snapshot;
            }
            catch (Exception e)
            {
                //Note: if we get an exception the process of restoring has potentially already written files, otherwise we would
                //      have gotten false sooner.
                index.Storage.Delete();
            }
        }

        return null;
    }

    private static async Task<bool> UnpackSnapshotAsync(IJsonIndex index, ISnapshot snapshot, Directory dir)
    {
        if(!snapshot.Verify())
            return false;

        using ISnapshotReader reader = snapshot.OpenReader();
        List<ISnapshotFile> snapshotFiles = reader.ReadFiles().ToList();

        ISnapshotFile segmentsFile = snapshotFiles
            .FirstOrDefault(file => Regex.IsMatch(file.Name, "^" + IndexFileNames.SEGMENTS + "_.*$"));

        if (segmentsFile == null)
            return false;

        string[] files = await Task.WhenAll(snapshotFiles.Except(new[] { segmentsFile }).Select(async file =>
        {
            using IndexOutputStream output = dir.CreateOutputStream(file.Name, IOContext.DEFAULT);
            using Stream sourceStream = file.Open();
            await sourceStream.CopyToAsync(output);
            return file.Name;
        }));
        //foreach (ISnapshotFile file in snapshotFiles.Except(new[] { segmentsFile }))
        //{
        //    using IndexOutputStream output = dir.CreateOutputStream(file.Name, IOContext.DEFAULT);
        //    using Stream sourceStream = file.Open();
        //    await sourceStream.CopyToAsync(output);
        //    files.Add(file.Name);
        //}

        dir.Sync(files);

        using IndexOutputStream segOutput = dir.CreateOutputStream(segmentsFile.Name, IOContext.DEFAULT);
        using Stream segmentsSourceStream = segmentsFile.Open();
        await segmentsSourceStream.CopyToAsync(segOutput);
        segOutput.Dispose();
        segmentsSourceStream.Dispose();
        dir.Sync(new[] { segmentsFile.Name });

        SegmentInfos.WriteSegmentsGen(dir, snapshot.Generation);

        //NOTE: (jmd 2020-09-30) Not quite sure what this does at this time, but the Lucene Replicator does it so better keep it for now.
        IndexCommit last = DirectoryReader.ListCommits(dir).Last();
        if (last != null)
        {
            ISet<string> commitFiles = new HashSet<string>(last.FileNames);
            commitFiles.Add(IndexFileNames.SEGMENTS_GEN);
        }
        index.WriterManager.Close();
        return true;
    }
}