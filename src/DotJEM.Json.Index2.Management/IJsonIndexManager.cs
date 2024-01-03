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
    IIngestProgressTracker Tracker { get; }
    Task RunAsync();
    Task<bool> TakeSnapshotAsync();
}

public class JsonIndexManager : IJsonIndexManager
{
    private readonly IJsonDocumentSource jsonDocumentSource;
    private readonly IJsonIndexSnapshotManager snapshots;
    private readonly IManagerJsonIndexWriter writer;
    
    private readonly IInfoStream<JsonIndexManager> infoStream = new InfoStream<JsonIndexManager>();

    public IInfoStream InfoStream => infoStream;
    public IIngestProgressTracker Tracker { get; }

    public JsonIndexManager(IJsonDocumentSource jsonDocumentSource, IJsonIndexSnapshotManager snapshots, IManagerJsonIndexWriter writer)
    {
        this.jsonDocumentSource = jsonDocumentSource;
        this.snapshots = snapshots;
        this.writer = writer;
        
        jsonDocumentSource.Observable.ForEachAsync(CaptureChange);
        jsonDocumentSource.InfoStream.Subscribe(infoStream);
        snapshots.InfoStream.Subscribe(infoStream);

        Tracker = new IngestProgressTracker();
        jsonDocumentSource.InfoStream.Subscribe(Tracker);
        jsonDocumentSource.Observable.Subscribe(Tracker);
        snapshots.InfoStream.Subscribe(Tracker);

        Tracker.InfoStream.Subscribe(infoStream);
        Tracker.ForEachAsync(state => infoStream.WriteTrackerStateEvent(state));
    }

    public async Task RunAsync()
    {
        bool restoredFromSnapshot = await RestoreSnapshotAsync();
        infoStream.WriteInfo($"Index restored from a snapshot: {restoredFromSnapshot}.");
        await Task.WhenAll(
            snapshots.RunAsync(Tracker, restoredFromSnapshot), 
            jsonDocumentSource.RunAsync()).ConfigureAwait(false);
    }

    public async Task<bool> TakeSnapshotAsync()
    {
        StorageIngestState state = Tracker.IngestState;
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
            Tracker.UpdateState(state);
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
