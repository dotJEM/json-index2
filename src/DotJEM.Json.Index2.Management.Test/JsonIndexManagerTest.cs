using System.Diagnostics;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using Bogus;
using DotJEM.AdvParsers;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Management.Info;
using DotJEM.Json.Index2.Management.Observables;
using DotJEM.Json.Index2.Management.Snapshots;
using DotJEM.Json.Index2.Management.Snapshots.Zip;
using DotJEM.Json.Index2.Management.Source;
using DotJEM.Json.Index2.Snapshots;
using DotJEM.ObservableExtensions.InfoStreams;
using DotJEM.Web.Scheduler;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotJEM.Json.Index2.Management.Test;



[TestFixture]
public class JsonIndexManagerTest
{
    [Test, Explicit]
    public async Task IndexWriterShouldNotBeDisposed()
    {
        using TestDirectory dir = new();

        IJsonIndex index = new JsonIndexBuilder("Test")
            .UsingSimpleFileStorage(dir.Info.CreateSubdirectory("index").FullName)
            .WithFieldResolver(new FieldResolver("id", "type"))
            .WithSnapshoting()
            .Build();

        IJsonDocumentSource source = new DummyDocumentSource();
        IWebTaskScheduler scheduler = new WebTaskScheduler();
        ISnapshotStrategy strategy = new ZipSnapshotStrategy(dir.Info.CreateSubdirectory("snapshot").FullName);
        IJsonIndexSnapshotManager snapshots = new JsonIndexSnapshotManager(index, strategy, scheduler, "60h");
        IJsonIndexManager manager = new JsonIndexManager(source, snapshots, index);

        InfoStreamExceptionEvent? disposedEvent = null;
        InfoStreamExceptionEvent? exceptionEvent = null;
        manager.InfoStream
            .OfType<InfoStreamExceptionEvent>()
            .Where(@event => @event.Exception is ObjectDisposedException)
            .Subscribe(@event =>
            {
                disposedEvent = @event;
            });
        manager.InfoStream
            .OfType<InfoStreamExceptionEvent>()
            .Where(@event => @event.Exception.Message != "Can't write to an existing snapshot.")
            .Subscribe(@event =>
            {
                exceptionEvent = @event;
            });

        try
        {
            await manager.RunAsync();
            Debug.WriteLine("TEST STARTED");
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.Elapsed < 10.Minutes() && disposedEvent == null && exceptionEvent == null)
            {
                Task result = Random.Shared.Next(100) switch
                {
                    (>= 0 and <= 50) => DoAfterDelay(manager.TakeSnapshotAsync, 1.Seconds()),
                    (> 50 and <= 100) => DoAfterDelay(manager.ResetIndexAsync, 1.Seconds()),
                    _ => Task.CompletedTask
                };
                await result;
            }

            async Task DoAfterDelay(Func<Task> action, TimeSpan? delay = null)
            {
                await Task.Delay(delay ?? Random.Shared.Next(1, 5).Seconds());
                await action();
            }

            await manager.StopAsync();
        }
        catch (TaskCanceledException)
        {
            //Ignore.
        }
        finally
        {
            index.Close();
        }

        Assert.That(disposedEvent, Is.Null, () => disposedEvent?.Exception.ToString());
        Assert.That(exceptionEvent, Is.Null, () => exceptionEvent?.Exception.ToString());
    }


}

public class TestDirectory : IDisposable
{
    public DirectoryInfo Info { get; }

    public TestDirectory()
    {
        Info = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"TEST-{Guid.NewGuid():N}"));
        Debug.WriteLine("TEST DIR: " + Info.FullName);
    }


    public void Dispose()
    {
        try
        {
            Info.Delete(true);
        }
        catch (Exception e)
        {
        }
    }

    ~TestDirectory()
    {
        Dispose();
    }
}
public class DummyDocumentSource : IJsonDocumentSource
{
    private readonly DocumentChangesStream observable = new();
    private readonly InfoStream<DummyDocumentSource> infoStream = new();

    public IObservable<IJsonDocumentSourceEvent> DocumentChanges => observable;
    public IObservableValue<bool> Initialized { get; } = new ObservableValue<bool>();
    public IInfoStream InfoStream => infoStream;

    private Faker faker = new Faker("en");
    private long gen;

    private Task? runningTask;
    private CancellationTokenSource cancellationTokenSource = new ();
    private readonly string area = "Test";

    public async Task StartAsync()
    {
        infoStream.WriteJsonSourceEvent(JsonSourceEventType.Starting, area, $"Ingest starting for storageArea '{area}'.");
        runningTask = Task.Run(async () =>
        {
            while (gen < 1_000_000 && !cancellationTokenSource.IsCancellationRequested)
            {
                if (!Initialized.Value)
                {
                    infoStream.WriteJsonSourceEvent(JsonSourceEventType.Initializing, area, $"Initializing for storageArea '{area}'.");
                    RunLoop();
                    Initialized.Value = true;
                    infoStream.WriteJsonSourceEvent(JsonSourceEventType.Initialized, area, $"Initializing for storageArea '{area}'.");
                }
                else
                {
                    infoStream.WriteJsonSourceEvent(JsonSourceEventType.Updating, area, $"Checking updates for storageArea '{area}'.");
                    RunLoop();
                    infoStream.WriteJsonSourceEvent(JsonSourceEventType.Updated, area, $"Done checking updates for storageArea '{area}'.");
                }
                observable.Publish(new JsonDocumentSourceDigestCompleted(area));
                await Task.Delay(Random.Shared.Next(50, 250));
            }
        }, cancellationTokenSource.Token);

        void RunLoop()
        {
            foreach (JObject json in Enumerable.Repeat(0, Random.Shared.Next(1, 100))
                         .Select(_ => Guid.NewGuid())
                         .Select(id => new
                         {
                             id,
                             type = "Test",
                             area,
                             time = DateTime.Now,
                             user = new
                             {
                                 name = faker.Name.FirstName()
                             }
                         })
                         .Select(JObject.FromObject))
            {
                observable.Publish(new JsonDocumentCreated(area, json, 10, new GenerationInfo(gen++, 1_000_000)));
            }
        }
    }

    public async Task StopAsync()
    {
        cancellationTokenSource.Cancel();

        if(runningTask != null)
            await runningTask.ConfigureAwait(false);
        
        runningTask = null;
        cancellationTokenSource = new();
        infoStream.WriteJsonSourceEvent(JsonSourceEventType.Stopped, area, $"Stopping for storageArea '{area}'.");
    }

    public void UpdateGeneration(string area, long generation)
    {
        gen = generation;
    }

    public async Task ResetAsync()
    {
        gen = 0;
    }
}
