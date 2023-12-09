using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Management.Snapshots;
using DotJEM.Json.Index2.Management.Tracking;
using DotJEM.Json.Index2.Management.Writer;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management;

public interface IJsonIndexManager
{
    IInfoStream InfoStream { get; }
    Task RunAsync();
    Task<bool> TakeSnapshotAsync();
}

public class JsonIndexManager : IJsonIndexManager
{
    private readonly IJsonDocumentSource jsonDocumentSource;
    private readonly IJsonIndexSnapshotManager snapshots;
    private readonly IIngestProgressTracker tracker;
    private readonly IManagerJsonIndexWriter writer;
    
    private readonly IInfoStream<JsonIndexManager> infoStream = new InfoStream<JsonIndexManager>();

    public IInfoStream InfoStream => infoStream;

    public JsonIndexManager(IJsonDocumentSource jsonDocumentSource, IJsonIndexSnapshotManager snapshots, IManagerJsonIndexWriter writer)
    {
        this.jsonDocumentSource = jsonDocumentSource;
        this.snapshots = snapshots;
        this.writer = writer;
        
        jsonDocumentSource.Observable.ForEachAsync(CaptureChange);
        jsonDocumentSource.InfoStream.Subscribe(infoStream);
        snapshots.InfoStream.Subscribe(infoStream);

        tracker = new IngestProgressTracker();
        jsonDocumentSource.InfoStream.Subscribe(tracker);
        jsonDocumentSource.Observable.Subscribe(tracker);
        snapshots.InfoStream.Subscribe(tracker);

        tracker.InfoStream.Subscribe(infoStream);
        tracker.ForEachAsync(state => infoStream.WriteTrackerStateEvent(state));
    }

    public async Task RunAsync()
    {
        bool restoredFromSnapshot = await RestoreSnapshotAsync();
        infoStream.WriteInfo($"Index restored from a snapshot: {restoredFromSnapshot}.");
        await Task.WhenAll(
            snapshots.RunAsync(tracker, restoredFromSnapshot), 
            jsonDocumentSource.RunAsync()).ConfigureAwait(false);
    }

    public async Task<bool> TakeSnapshotAsync()
    {
        StorageIngestState state = tracker.IngestState;
        return await snapshots.TakeSnapshotAsync(state);
    }

    public async Task<bool> RestoreSnapshotAsync()
    {
        RestoreSnapshotResult restoreResult = await snapshots.RestoreSnapshotAsync();
        if (!restoreResult.RestoredFromSnapshot)
            return false;

        foreach (StorageAreaIngestState state in restoreResult.State.Areas)
        {
            jsonDocumentSource.UpdateGeneration(state.Area, state.Generation.Current);
            tracker.UpdateState(state);
        }

        return true;
    }

    private void CaptureChange(IJsonDocumentChange change)
    {
        try
        {
            switch (change.Type)
            {
                case JsonChangeType.Create:
                    writer.Create(change.Entity);
                    break;
                case JsonChangeType.Update:
                    writer.Write(change.Entity);
                    break;
                case JsonChangeType.Delete:
                    writer.Delete(change.Entity);
                    break;
            }
        }
        catch (Exception ex)
        {
            infoStream.WriteError($"Failed to ingest change from {change.Area}", ex);
        }
    }
}


public class Initialization
{
    public static Task WhenInitializationComplete(IIngestProgressTracker tracker)
    {
        TaskCompletionSource<bool> completionSource = new ();
        tracker.SkipWhile(state =>
        {
            if (state is not StorageIngestState ingestState)
                return true;

            JsonSourceEventType[] states = ingestState.Areas
                .Select(x => x.LastEvent)
                .ToArray();
            if (!states.All(state => state is JsonSourceEventType.Updated or JsonSourceEventType.Initialized))
                return true;

            return false;
        }).FirstAsync(state => {
            completionSource.SetResult(true);
            return true;
        });

        //tracker.Subscribe(state => {
        //    if(state is not StorageIngestState ingestState)
        //        return;

        //    JsonSourceEventType[] states = ingestState.Areas
        //        .Select(x => x.LastEvent)
        //        .ToArray();
        //    if (states.All(state => state is JsonSourceEventType.Updated or JsonSourceEventType.Initialized))
        //        completionSource.SetResult(true);

            
        //}, CancellationToken.None);
        return completionSource.Task;
    }

}

