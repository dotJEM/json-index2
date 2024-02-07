using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Management.Info;
using DotJEM.Json.Index2.Management.Source;
using DotJEM.ObservableExtensions;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management.Tracking;

public enum IngestInitializationState
{
    Started,
    Restoring,
    Ingesting,
    Initialized
}

// ReSharper disable once PossibleInterfaceMemberAmbiguity -> Just dictates implementation must be explicit which is OK.
public interface IIngestProgressTracker :
    IObserver<IJsonDocumentChange>, 
    IObserver<IInfoStreamEvent>, IObservable<ITrackerState>
{
    IngestInitializationState InitializationState { get; }
    IInfoStream InfoStream { get; }
    StorageIngestState IngestState { get; }
    SnapshotRestoreState RestoreState { get; }
    void UpdateState(StorageAreaIngestState state);
    void SetInitialized(bool initialized);
    Task WhenState(IngestInitializationState state);
}
public interface ITrackerState {}

public class IngestProgressTracker : BasicSubject<ITrackerState>, IIngestProgressTracker
{
    //TODO: Along with the Todo later down, this should be changed so that we can compute the state quicker.
    //      It's fine that data-in is guarded by a ConcurrentDictionary, but for data out it shouldn't matter.
    private readonly ConcurrentDictionary<string, StorageAreaIngestStateTracker> observerTrackers = new();
    private readonly ConcurrentDictionary<string, IndexFileRestoreStateTracker> restoreTrackers = new();
    private readonly IInfoStream<JsonIndexManager> infoStream = new InfoStream<JsonIndexManager>();
    private IngestInitializationState initializationState;

    public IInfoStream InfoStream => infoStream;

    public IngestInitializationState InitializationState
    {
        get => initializationState;
        private set
        {
            if(initializationState == value) return;

            initializationState = value;
            TaskCompletionSource<bool> target = value switch
            {
                IngestInitializationState.Started => startedEntered,
                IngestInitializationState.Restoring => restoringEntered,
                IngestInitializationState.Ingesting => ingestingEntered,
                IngestInitializationState.Initialized => initializedEntered,
                _ => null
            };
            target?.TrySetResult(true);
        }
    }

    private readonly TaskCompletionSource<bool> startedEntered = new ();
    private readonly TaskCompletionSource<bool> restoringEntered = new ();
    private readonly TaskCompletionSource<bool> ingestingEntered = new ();
    private readonly TaskCompletionSource<bool> initializedEntered = new ();


    // TODO: We are adding a number of computational cycles here on each single update, this should be improved as well.
    //       So we don't have to do a loop on each turn, but later with that.
    public StorageIngestState IngestState => new (observerTrackers.Select(kv => kv.Value.State).ToArray());
    public SnapshotRestoreState RestoreState => new (restoreTrackers.Select(kv => kv.Value.State).ToArray());

    public IngestProgressTracker()
    {
        InitializationState = IngestInitializationState.Started;
    }

    public void OnNext(IJsonDocumentChange value)
    {
        observerTrackers.AddOrUpdate(value.Area, _ => throw new InvalidDataException(), (_, state) => state.UpdateState(value.Generation, value.Size));
        InternalPublish(IngestState);
    }

    public void UpdateState(StorageAreaIngestState state)
    {
        observerTrackers.AddOrUpdate(state.Area, s => new StorageAreaIngestStateTracker(s, JsonSourceEventType.Initialized).UpdateState(state)
            , (s, tracker) => tracker.UpdateState(state));
    }

    public void SetInitialized(bool initialized)
    {
        if (initialized) this.InitializationState = IngestInitializationState.Initialized;
    }

    public void OnNext(IInfoStreamEvent value)
    {
        switch (value)
        {
            case StorageObserverInfoStreamEvent evt:
                OnStorageObserverInfoStreamEvent(evt);
                break;
                
            case SnapshotInfoEvent evt:
                OnSnapshotEvent(evt);
                break;
                
            case FileInfoEvent evt:
                OnFileEvent(evt);
                break;
        }
    }

    private void OnFileEvent(FileInfoEvent sne)
    {
        switch (sne)
        {
            case FileOpenedInfoEvent:
                restoreTrackers.AddOrUpdate(
                    sne.FileName,
                    name => new IndexFileRestoreStateTracker(name, sne.Progress),
                    (name, tracker) => tracker.Restoring(sne.Progress)
                );
                break;
            case FileClosedInfoEvent:
                restoreTrackers.AddOrUpdate(
                    sne.FileName,
                    name => new IndexFileRestoreStateTracker(name, sne.Progress),
                    (name, tracker) => tracker.Complete(sne.Progress)
                );
                break;

            case FileProgressInfoEvent:
                restoreTrackers.AddOrUpdate(
                    sne.FileName,
                    name => new IndexFileRestoreStateTracker(name, sne.Progress),
                    (name, tracker) => tracker.Progress(sne.Progress)
                );
                break;


            default:
                throw new ArgumentOutOfRangeException();
        }
        InternalPublish(RestoreState);
    }

    private void OnSnapshotEvent(SnapshotInfoEvent sne)
    {
        InternalPublish(RestoreState);
    }

    private void OnStorageObserverInfoStreamEvent(StorageObserverInfoStreamEvent soe)
    {
        switch (soe.EventType)
        {
            case JsonSourceEventType.Starting:
                observerTrackers.GetOrAdd(soe.Area, new StorageAreaIngestStateTracker(soe.Area, soe.EventType));
                break;
            case JsonSourceEventType.Initializing:
            case JsonSourceEventType.Initialized:
            case JsonSourceEventType.Updating:
            case JsonSourceEventType.Updated:
            case JsonSourceEventType.Stopped:
            default:
                observerTrackers.AddOrUpdate(soe.Area, _ => new StorageAreaIngestStateTracker(soe.Area, soe.EventType), (_, state) => state.UpdateState(soe.EventType));
                break;
        }

        // TODO: We are adding a number of computational cycles here on each single update, this should be improved as well.
        //       So we don't have to do a loop on each turn, but later with that.
        InternalPublish(IngestState);
    }

    private void InternalPublish(ITrackerState state)
    {
        if (InitializationState == IngestInitializationState.Initialized)
        {
            Publish(state);
            return;
        }

        switch (state)
        {
            case SnapshotRestoreState:
                InitializationState = IngestInitializationState.Restoring;
                break;

            case StorageIngestState storageIngestState:
                JsonSourceEventType[] states = storageIngestState.Areas
                    .Select(x => x.LastEvent)
                    .ToArray();
                InitializationState = states.All(state => state is JsonSourceEventType.Updated or JsonSourceEventType.Updating or JsonSourceEventType.Initialized) 
                    ? IngestInitializationState.Initialized
                    : IngestInitializationState.Ingesting;
                break;

        }
        Publish(state);
    }


    void IObserver<IInfoStreamEvent>.OnError(Exception error) { }
    void IObserver<IInfoStreamEvent>.OnCompleted() { }
    void IObserver<IJsonDocumentChange>.OnError(Exception error) { }
    void IObserver<IJsonDocumentChange>.OnCompleted() { }

    private class IndexFileRestoreStateTracker
    {
        public SnapshotFileRestoreState State { get; private set; }

        public IndexFileRestoreStateTracker(string name, FileProgress progress)
        {
            State = new SnapshotFileRestoreState(name, "PENDING", DateTime.Now, DateTime.Now, progress);
        }

        public IndexFileRestoreStateTracker Restoring(FileProgress progress)
        {
            State = State with{ State = "RESTORING", StartTime = DateTime.Now, Progress = progress };
            return this;
        }

        public IndexFileRestoreStateTracker Complete(FileProgress progress)
        {
            State = State with{ State = "COMPLETE", StopTime = DateTime.Now, Progress = progress  };
            return this;
        }

        public IndexFileRestoreStateTracker Progress(FileProgress progress)
        {
            State = State with{ State = "RESTORING", StartTime = DateTime.Now, Progress = progress };
            return this;
        }
    }

    private class StorageAreaIngestStateTracker
    {
        private Stopwatch initTimer = new ();
        private Stopwatch updateTimer =  new ();
        private bool initializing = true;

        public StorageAreaIngestState State { get; private set; }

        public StorageAreaIngestStateTracker(string area, JsonSourceEventType state)
        {
            State = new StorageAreaIngestState(area, DateTime.Now, TimeSpan.Zero, 0, new GenerationInfo(-1,-1), state, 0, 0, TimeSpan.Zero, TimeSpan.Zero, 0);
        }

        public StorageAreaIngestStateTracker UpdateState(JsonSourceEventType state)
        {
            switch (state)
            {
                case JsonSourceEventType.Initializing:
                    initTimer = Stopwatch.StartNew();
                    initializing = true;
                    break;

                case JsonSourceEventType.Updating:
                    updateTimer.Restart();
                    initializing = false;
                    break;

                case JsonSourceEventType.Initialized:
                    initTimer.Stop();
                    initializing = false;
                    State = State with { LastEvent = state, Duration = initTimer.Elapsed };
                    break;
                
                case JsonSourceEventType.Updated:
                    updateTimer.Stop();
                    initializing = false;
                    State = State with
                    {
                        LastEvent = state, 
                        UpdateCycles = State.UpdateCycles + 1,
                        TotalUpdateDuration = State.TotalUpdateDuration + updateTimer.Elapsed, 
                        LastUpdateDuration = updateTimer.Elapsed
                    };
                    break;

            }
            return this;
        }

        public StorageAreaIngestStateTracker UpdateState(GenerationInfo generation, int size)
        {
            if (initializing)
            {
                State = State with { IngestedCount = State.IngestedCount+1, Generation = generation, Duration = initTimer.Elapsed, BytesLoaded = size + State.BytesLoaded };
            }
            else
            {
                State = State with { UpdatedCount = State.UpdatedCount+1, Generation = generation, LastUpdateDuration =updateTimer.Elapsed, BytesLoaded = size + State.BytesLoaded };

            }
            return this;
        }

        public StorageAreaIngestStateTracker UpdateState(StorageAreaIngestState areaState)
        {
            this.State = areaState;
            return this;
        }
    }

    public Task WhenState(IngestInitializationState state)
    {
        return state switch
        {
            IngestInitializationState.Started => startedEntered.Task,
            IngestInitializationState.Restoring => restoringEntered.Task,
            IngestInitializationState.Ingesting => ingestingEntered.Task,
            IngestInitializationState.Initialized => initializedEntered.Task,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}


