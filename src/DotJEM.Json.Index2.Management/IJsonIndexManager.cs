using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Management.Info;
using DotJEM.Json.Index2.Management.Observables;
using DotJEM.Json.Index2.Management.Snapshots;
using DotJEM.Json.Index2.Management.Source;
using DotJEM.Json.Index2.Management.Tracking;
using DotJEM.Json.Index2.Management.Writer;
using DotJEM.ObservableExtensions.InfoStreams;
using DotJEM.Web.Scheduler;
using Lucene.Net.Search;

namespace DotJEM.Json.Index2.Management;

public interface IJsonIndexManager
{
    IInfoStream InfoStream { get; }
    IIngestProgressTracker Tracker { get; }
    IObservable<IJsonDocumentSourceEvent> DocumentChanges { get; }
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
    private readonly IJsonIndex index;

    private readonly IInfoStream<JsonIndexManager> infoStream = new InfoStream<JsonIndexManager>();
    private readonly DocumentChangesStream changesStream = new();

    public IInfoStream InfoStream => infoStream;
    public IIngestProgressTracker Tracker { get; }
    public IObservable<IJsonDocumentSourceEvent> DocumentChanges => changesStream;

    public JsonIndexManager(
        IJsonDocumentSource jsonDocumentSource,
        IJsonIndexSnapshotManager snapshots,
        //TODO: Allow multiple indexes and something that can shard
        IJsonIndex index,
        IJsonIndexWriter writer = null)
    {
        this.jsonDocumentSource = jsonDocumentSource;
        this.snapshots = snapshots;
        this.index = index;
        this.writer = writer ?? new JsonIndexWriter(index);
        this.writer.InfoStream.Subscribe(infoStream);

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
        bool restoredFromSnapshot = await RestoreSnapshotAsync().ConfigureAwait(false);
        infoStream.WriteInfo($"Index restored from a snapshot: {restoredFromSnapshot}.");

        await Task
            .WhenAll(
                jsonDocumentSource.StartAsync(),
                snapshots.RunAsync(Tracker, restoredFromSnapshot))
            .ConfigureAwait(false);
    }

    public async Task<bool> TakeSnapshotAsync()
    {
        StorageIngestState state = Tracker.IngestState;
        return await snapshots.TakeSnapshotAsync(state).ConfigureAwait(false);
    }

    public async Task<bool> RestoreSnapshotAsync()
    {
        RestoreSnapshotResult restoreResult = await snapshots.RestoreSnapshotAsync().ConfigureAwait(false);
        if (!restoreResult.RestoredFromSnapshot)
            return false;

        foreach (StorageAreaIngestState state in restoreResult.State.Areas)
        {
            jsonDocumentSource.UpdateGeneration(state.Area, state.Generation.Current);
            Tracker.UpdateState(state);
        }

        return true;
    }

    //TODO: Area is a concept that belongs to the specific storage implementation.
    //      So is the generation, How can we pass these in a decoupled way?
    public Task UpdateGenerationAsync(string area, long generation)
    {
        jsonDocumentSource.UpdateGeneration(area, generation);
        return Task.CompletedTask;
    }


    /// <summary>
    /// Stops the underlying <see cref="IJsonDocumentSource"/>, deletes it's storage,
    /// requests reset of the underlying <see cref="IJsonDocumentSource"/> and then starts it again.
    /// </summary>
    public async Task ResetIndexAsync()
    {
        await jsonDocumentSource.StopAsync().ConfigureAwait(false);
        index.Storage.Delete();
        await jsonDocumentSource.ResetAsync().ConfigureAwait(false);
        await jsonDocumentSource.StartAsync().ConfigureAwait(false);
    }

    private void CaptureChange(IJsonDocumentSourceEvent sourceEvent)
    {
        try
        {
            switch (sourceEvent)
            {
                case JsonDocumentSourceDigestCompleted:
                    writer.Commit();
                    break;
                case JsonDocumentCreated created:
                    writer.Create(created.Document);
                    break;
                case JsonDocumentDeleted deleted:
                    writer.Delete(deleted.Document);
                    break;
                case JsonDocumentUpdated updated:
                    writer.Update(updated.Document);
                    break;
            }

            changesStream.Publish(sourceEvent);
        }
        catch (Exception ex)
        {
            infoStream.WriteError($"Failed to ingest change from {sourceEvent.Area}", ex);
        }
    }
}
