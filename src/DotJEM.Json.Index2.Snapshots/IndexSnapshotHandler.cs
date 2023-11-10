using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotJEM.Json.Index2.Snapshots.Streams;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace DotJEM.Json.Index2.Snapshots
{
    
    public interface IIndexSnapshotHandler
    {
        ISnapshot Snapshot(IJsonIndex index, ISnapshotTarget target);
        ISnapshot Restore(IJsonIndex index, ISnapshotSource source);
    }

    public class IndexSnapshotHandler : IIndexSnapshotHandler
    {
        public ISnapshot Snapshot(IJsonIndex index, ISnapshotTarget target)
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

                using ISnapshotWriter snapshotWriter = target.Open(commit.Generation);
                foreach (string fileName in commit.FileNames)
                    snapshotWriter.WriteFileAsync(fileName, dir);
                return snapshotWriter.Snapshot;
            }
            finally
            {
                if (commit != null)
                {
                    sdp.Release(commit);
                }
            }
        }

        public ISnapshot Restore(IJsonIndex index, ISnapshotSource source)
        {
            index.Storage.Delete();
            Directory dir = index.Storage.Directory;
            using ISnapshotReader reader = source.Open();

            ILuceneFile sementsFile = null;
            List<string> files = new List<string>();
            foreach (ILuceneFile file in reader.ReadFiles())
            {
                if (Regex.IsMatch(file.Name, "^" + IndexFileNames.SEGMENTS + "_.*$"))
                {
                    sementsFile = file;
                    continue;
                }

                using IndexOutputStream output = dir.CreateOutputStream(file.Name, IOContext.DEFAULT);
                using Stream sourceStream = file.Open();
                sourceStream.CopyToAsync(output);
                files.Add(file.Name);
            }
            dir.Sync(files);

            if (sementsFile == null)
                throw new ArgumentException();

            using IndexOutputStream segOutput = dir.CreateOutputStream(sementsFile.Name, IOContext.DEFAULT);
            using Stream sementsSourceStream = sementsFile.Open();
            sementsSourceStream.CopyToAsync(segOutput);
            dir.Sync(new [] { sementsFile.Name });

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
}