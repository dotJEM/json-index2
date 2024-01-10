using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Management.Observables;
using DotJEM.Json.Index2.Management.Snapshots;
using DotJEM.Json.Index2.Management.Source;
using DotJEM.Json.Index2.Management.Tracking;
using DotJEM.Json.Index2.Management.Writer;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management;

public interface IJsonIndexManager
{
    IInfoStream InfoStream { get; }
    IIngestProgressTracker Tracker { get; }
    IObservable<IJsonDocumentChange> DocumentChanges { get; }
    Task<bool> TakeSnapshotAsync();
    Task RunAsync();
    Task UpdateGenerationAsync(string area, long generation);
    Task ResetIndexAsync();
}

public class JsonIndexManager : IJsonIndexManager
{
    private readonly IJsonDocumentSource jsonDocumentSource;
    private readonly IJsonIndexSnapshotManager snapshots;
    private readonly IJsonIndexWriter writer;
    
    private readonly IInfoStream<JsonIndexManager> infoStream = new InfoStream<JsonIndexManager>();
    private readonly DocumentChangesStream changesStream = new();

    public IInfoStream InfoStream => infoStream;
    public IIngestProgressTracker Tracker { get; }
    public IObservable<IJsonDocumentChange> DocumentChanges => changesStream;

    public JsonIndexManager(
        IJsonDocumentSource jsonDocumentSource,
        IJsonIndexSnapshotManager snapshots,
        //TODO: Allow multiple indexes and something that can shard
        IJsonIndexWriter writer)
    {
        this.jsonDocumentSource = jsonDocumentSource;
        this.snapshots = snapshots;
        this.writer = writer;

        Tracker = new IngestProgressTracker();
        jsonDocumentSource.DocumentChanges.ForEachAsync(CaptureChange);
        jsonDocumentSource.InfoStream.Subscribe(infoStream);
        jsonDocumentSource.Initialized.Subscribe(Tracker.SetInitialized);
        snapshots.InfoStream.Subscribe(infoStream);

        jsonDocumentSource.InfoStream.Subscribe(Tracker);
        jsonDocumentSource.DocumentChanges.Subscribe(Tracker);
        snapshots.InfoStream.Subscribe(Tracker);

        Tracker.InfoStream.Subscribe(infoStream);
        Tracker.ForEachAsync(state => infoStream.WriteTrackerStateEvent(state, "Tracker state updated"));
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

    public Task UpdateGenerationAsync(string area, long generation)
    {
        jsonDocumentSource.UpdateGeneration(area, generation);
        return Task.CompletedTask;
    }

    public async Task ResetIndexAsync()
    {
        await jsonDocumentSource.ResetAsync();
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

            changesStream.Publish(change);
        }
        catch (Exception ex)
        {
            infoStream.WriteError($"Failed to ingest change from {change.Area}", ex);
        }
    }
}
