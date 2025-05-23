﻿using DotJEM.Json.Index2.Management.Observables;
using DotJEM.Json.Index2.Management.Source;
using DotJEM.Json.Storage;
using DotJEM.ObservableExtensions.InfoStreams;
using DotJEM.Web.Scheduler;
using StressTester.Adapter;

namespace Stress.Adapter;

public class JsonStorageDocumentSource : IJsonDocumentSource
{
    private readonly Dictionary<string, IJsonStorageAreaObserver> observers;
    private readonly DocumentChangesStream observable = new();
    private readonly InfoStream<JsonStorageDocumentSource> infoStream = new();

    public IObservable<IJsonDocumentSourceEvent> DocumentChanges => observable;
    public IObservableValue<bool> Initialized { get; } = new ObservableValue<bool>();
    public IInfoStream InfoStream => infoStream;

    public JsonStorageDocumentSource(IStorageContext context, IWebTaskScheduler scheduler)
        : this(new JsonStorageAreaObserverFactory(context, scheduler))
    {
    }

    public JsonStorageDocumentSource(IJsonStorageAreaObserverFactory factory)
    {
        observers = factory
            .CreateAll()
            .Select(observer =>
            {
                observer.DocumentChanges.Subscribe(Forward);
                observer.InfoStream.Subscribe(infoStream);
                observer.Initialized.Subscribe(_ => InitializedChanged());
                return observer;
            })
            .ToDictionary(x => x.AreaName);
    }

    private void Forward(IJsonDocumentSourceEvent sourceEvent)
    {
        //Only pass a commit signal through once all are initialized.
        if(sourceEvent is JsonDocumentSourceDigestCompleted && !Initialized.Value)
            return;

        observable.Publish(sourceEvent);
    }

    private void InitializedChanged()
    {
        this.Initialized.Value = observers.Values.All(observer => observer.Initialized.Value);
    }

    public async Task StartAsync()
    {
        await Task.WhenAll(
            observers.Values.Select(async observer => await observer.StartAsync().ConfigureAwait(false))
        ).ConfigureAwait(false);
    }

    public async Task StopAsync()
    {
        await Task.WhenAll(
            observers.Values.Select(async observer => await observer.StopAsync().ConfigureAwait(false))
        ).ConfigureAwait(false);
    }

    public void UpdateGeneration(string area, long generation)
    {
        if (!observers.TryGetValue(area, out IJsonStorageAreaObserver observer))
            return; // TODO?

        observer.UpdateGeneration(area, generation);
    }

    public async Task ResetAsync()
    {
        foreach (IJsonStorageAreaObserver observer in observers.Values)
            await observer.ResetAsync();
    }
}