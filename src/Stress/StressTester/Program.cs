// See https://aka.ms/new-console-template for more information


using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotJEM.Json.Index2;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Management;
using DotJEM.Json.Index2.Management.Info;
using DotJEM.Json.Index2.Management.Snapshots;
using DotJEM.Json.Index2.Management.Snapshots.Zip;
using DotJEM.Json.Index2.Management.Tracking;
using DotJEM.Json.Index2.Management.Writer;
using DotJEM.Json.Index2.Searching;
using DotJEM.Json.Index2.Snapshots;
using DotJEM.Json.Storage;
using DotJEM.Json.Storage.Configuration;
using DotJEM.ObservableExtensions.InfoStreams;
using DotJEM.Web.Scheduler;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Search;
using Stress.Adapter;
using JsonIndexWriter = DotJEM.Json.Index2.Management.Writer.JsonIndexWriter;

//TraceSource trace; 

IStorageContext storage = new SqlServerStorageContext("Data Source=.\\DEV;Initial Catalog=nsw;Integrated Security=True");
//IStorageContext storage = new SqlServerStorageContext("Data Source=.\\DEV;Initial Catalog=STRESS;Integrated Security=True");
storage.Configure.MapField(JsonField.Id, "id");
storage.Configure.MapField(JsonField.ContentType, "contentType");
storage.Configure.MapField(JsonField.Version, "$version");
storage.Configure.MapField(JsonField.Created, "$created");
storage.Configure.MapField(JsonField.Updated, "$updated");
storage.Configure.MapField(JsonField.SchemaVersion, "$schemaVersion");

//StressDataGenerator generator = new StressDataGenerator(
//    storage.Area("Settings"),
//    storage.Area("Queue"),
//    storage.Area("Recipes"),
//    storage.Area("Animals"),
//    storage.Area("Games"),
//    storage.Area("Players"),
//    storage.Area("Planets"),
//    storage.Area("Universe"),
//    storage.Area("Trashcan")
//);
//Task genTask = generator.StartAsync();
//await Task.Delay(5000);

if(Directory.Exists(@".\app_data\index"))
    Directory.Delete(@".\app_data\index", true);
Directory.CreateDirectory(@".\app_data\index");
Directory.CreateDirectory(@".\app_data\snapshots");

IJsonIndex index = new JsonIndexBuilder("main")
    .UsingSimpleFileStorage(@".\app_data\index")
    .WithAnalyzer(cfg=> new StandardAnalyzer(cfg.Version,CharArraySet.EMPTY_SET))
    .WithFieldResolver(new FieldResolver("id", "contentType"))
    .WithSnapshoting()
    .Build();

string[] areas = new[] { "content", "settings", "diagnostic", "emsaqueue", "statistic" };

IWebTaskScheduler scheduler = new WebTaskScheduler();
IJsonIndexManager jsonIndexManager = new JsonIndexManager(
    new JsonStorageDocumentSource(new JsonStorageAreaObserverFactory(storage, scheduler,areas)),
    new JsonIndexSnapshotManager(index, new ZipSnapshotStrategy(".\\app_data\\snapshots"), scheduler, "10s"),
    new JsonIndexWriter(index, scheduler)
);

Task run = Task.WhenAll(
    jsonIndexManager.InfoStream.ForEachAsync(Reporter.CaptureInfo),
    jsonIndexManager.RunAsync()
    //,genTask
);


jsonIndexManager.Tracker.WhenState(IngestInitializationState.Initialized).ContinueWith(task =>
{
    Console.Clear();
    Console.WriteLine("COMPLETED");
    Console.WriteLine("COMPLETED");
    Console.WriteLine("COMPLETED");
    Console.WriteLine("COMPLETED");
    Console.WriteLine("COMPLETED");
    Console.WriteLine("COMPLETED");

});

while (true)
{
    string? input = Console.ReadLine();
    switch (input?.ToUpper().FirstOrDefault())
    {
        case 'E':
            //generator.Stop();
            goto EXIT;

        case 'S':
            await jsonIndexManager.TakeSnapshotAsync();
            break;

        case 'C':
            jsonIndexManager.Tracker.WhenState(IngestInitializationState.Initialized).ContinueWith(task =>
            {
                Console.Clear();
                Console.WriteLine("COMPLETED");
            });

            break;

        case 'I':
            Console.Clear();
            break;

        case 'L':
            Console.Clear();
            break;

        case 'Q':
            int matches = index.Search(new MatchAllDocsQuery()).Count();
            Console.WriteLine($"Matched documents: {matches}");
            break;

        default:
            Reporter.Report(true);
            break;
    }
}

EXIT:
await run;


public static class Reporter
{
    private static ITrackerState lastState;
    private static IInfoStreamEvent lastEvent;
    private static DateTime lastReport = DateTime.Now;

    private static readonly Queue<string> messages = new Queue<string>();

    public static void CaptureInfo(IInfoStreamEvent evt)
    {
        lock (messages)
        {
            messages.Enqueue(evt.Message);
            if (messages.Count > 50)
                messages.Dequeue();
        }

        switch (evt)
        {
            case InfoStreamExceptionEvent error:
                Console.WriteLine(error.Exception);
                return;

            case TrackerStateInfoStreamEvent ievt:
                lastState = ievt.State;
                break;

            default:

                lastEvent = evt;
                break;
        }
        Report();
    }

    private static string CleanLine = new string(' ', Console.BufferWidth);
    private static StringBuilder buffer = new StringBuilder();
    private static Regex nl = new Regex("\n|\r\n", RegexOptions.Compiled);

    public static void Report(bool force = false)
    {
        if(!force && DateTime.Now - lastReport < TimeSpan.FromSeconds(30))
            return;
        lastReport = DateTime.Now;
        int lines = nl.Matches(buffer.ToString()).Count;
        buffer.Clear();
        Console.SetCursorPosition(0,0);
        for (int i = 0; i < lines; i++)
            buffer.AppendLine(CleanLine);       
        Console.WriteLine(buffer);
        Console.SetCursorPosition(0,0);
        buffer.Clear();
        string[] msgs;
        lock (messages)
        {
            msgs = messages.ToArray();
        }

        foreach (string message in msgs)
        {
            buffer.AppendLine(message);
        }

        buffer.AppendLine();
        //Console.WriteLine(lastEvent.Message);
        buffer.AppendLine(lastState.ToString());
        Console.WriteLine(buffer);
    }
}
