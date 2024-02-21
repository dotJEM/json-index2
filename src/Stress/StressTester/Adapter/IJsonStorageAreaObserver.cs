using System.Diagnostics;
using DotJEM.Json.Index2.Management.Info;
using DotJEM.Json.Index2.Management.Observables;
using DotJEM.Json.Index2.Management.Source;
using DotJEM.Json.Storage.Adapter;
using DotJEM.Json.Storage.Adapter.Materialize.ChanceLog.ChangeObjects;
using DotJEM.Json.Storage.Adapter.Observable;
using DotJEM.ObservableExtensions.InfoStreams;
using DotJEM.Web.Scheduler;

namespace StressTester.Adapter;

public interface IJsonStorageAreaObserver : IJsonDocumentSource
{
    string AreaName { get; }
}

public class JsonStorageAreaObserver : IJsonStorageAreaObserver
{
    private readonly string pollInterval;
    private readonly IWebTaskScheduler scheduler;
    private readonly IStorageAreaLog log;
    private readonly DocumentChangesStream observable = new();
    private readonly IInfoStream<JsonStorageAreaObserver> infoStream = new InfoStream<JsonStorageAreaObserver>();
    
    private long generation = 0;
    private long initialGeneration = 0;
    private IScheduledTask task;
    public IStorageArea StorageArea { get; }

    public string AreaName => StorageArea.Name;
    public IInfoStream InfoStream => infoStream;
    public IObservable<IJsonDocumentSourceEvent> DocumentChanges => observable;
    public IObservableValue<bool> Initialized { get; } = new ObservableValue<bool>();

    public JsonStorageAreaObserver(IStorageArea storageArea, IWebTaskScheduler scheduler, string pollInterval = "10s")
    {
        StorageArea = storageArea;
        this.scheduler = scheduler;
        this.pollInterval = pollInterval;
        log = storageArea.Log;
    }

    public async Task RunAsync()
    {
        infoStream.WriteJsonSourceEvent(JsonSourceEventType.Starting, StorageArea.Name, $"Ingest starting for storageArea '{StorageArea.Name}'.");
        task = scheduler.Schedule($"JsonStorageAreaObserver:{StorageArea.Name}", _ => RunUpdateCheck(), pollInterval);
        task.InfoStream.Subscribe(infoStream);
        await task.WhenCompleted();
    }

    public async Task StopAsync()
    {
        task.Dispose();
        await task.WhenCompleted();
        infoStream.WriteJsonSourceEvent(JsonSourceEventType.Stopped, StorageArea.Name, $"Initializing for storageArea '{StorageArea.Name}'.");
    }

    public void UpdateGeneration(string area, long value)
    {
        if(!AreaName.Equals(area))
            return;

        generation = value;
        initialGeneration = value;
        Initialized.Value = true;
    }

    public async Task ResetAsync()
    {
        UpdateGeneration(AreaName, initialGeneration);
        observable.Publish(new JsonDocumentSourceReset(AreaName));
        task.Signal();
    }

    public void RunUpdateCheck()
    {
        long latestGeneration = log.LatestGeneration;
        
        Stopwatch timer = Stopwatch.StartNew();
        if (!Initialized.Value)
        {
            BeforeInitialize();
            infoStream.WriteJsonSourceEvent(JsonSourceEventType.Initializing, StorageArea.Name, $"Initializing for storageArea '{StorageArea.Name}'.");
            using IStorageAreaLogReader changes = log.OpenLogReader(generation, Initialized.Value );
            PublishChanges(changes, row => new JsonDocumentCreated(row.Area, row.CreateEntity(), row.Size, new GenerationInfo(row.Generation, latestGeneration)));
            Initialized.Value  = true;
            infoStream.WriteJsonSourceEvent(JsonSourceEventType.Initialized, StorageArea.Name, $"Initialization complete for storageArea '{StorageArea.Name}' in {timer.Elapsed}.");
            AfterInitialize();

        }
        else
        {
            BeforeUpdate();
            infoStream.WriteJsonSourceEvent(JsonSourceEventType.Updating, StorageArea.Name, $"Checking updates for storageArea '{StorageArea.Name}'.");
            using IStorageAreaLogReader changes = log.OpenLogReader(generation, Initialized.Value );
            PublishChanges(changes, MapRow);
            infoStream.WriteJsonSourceEvent(JsonSourceEventType.Updated, StorageArea.Name, $"Done checking updates for storageArea '{StorageArea.Name}' in {timer.Elapsed}.");
            AfterUpdate();
        }
        PublishCommitSignal();

        IJsonDocumentSourceEvent MapRow(IChangeLogRow row)
        {
            return row.Type switch
            {
                ChangeType.Create => new JsonDocumentCreated(row.Area, row.CreateEntity(), row.Size, new GenerationInfo(row.Generation, latestGeneration)),
                ChangeType.Update => new JsonDocumentUpdated(row.Area, row.CreateEntity(), row.Size, new GenerationInfo(row.Generation, latestGeneration)),
                ChangeType.Delete => new JsonDocumentDeleted(row.Area, row.CreateEntity(), row.Size, new GenerationInfo(row.Generation, latestGeneration)),
                _ => throw new NotSupportedException()
            };
        }

        void PublishCommitSignal()
        {
            observable.Publish(new JsonDocumentSourceDigestCompleted(AreaName));
        }

        void PublishChanges(IStorageAreaLogReader changes, Func<IChangeLogRow, IJsonDocumentSourceEvent> rowMapper)
        {
            foreach (IChangeLogRow change in changes)
            {
                generation = change.Generation;
                if (change.Type == ChangeType.Faulty)
                    continue;

                observable.Publish(rowMapper(change));
            }
        }
    }
    public virtual void BeforeInitialize() { }
    public virtual void AfterInitialize() { }

    public virtual void BeforeUpdate() { }
    public virtual void AfterUpdate() { }
}
