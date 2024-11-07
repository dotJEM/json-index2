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
using DotJEM.Json.Index2.Management.Source;
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
using Newtonsoft.Json.Linq;
using Stress.Adapter;
using Stress.Data;
using JsonIndexWriter = DotJEM.Json.Index2.Management.Writer.JsonIndexWriter;

//TraceSource trace; 

IStorageContext storage = new SqlServerStorageContext("Data Source=.\\DEV;Initial Catalog=SSN3DB;Integrated Security=True");
//IStorageContext storage = new SqlServerStorageContext("Data Source=.\\DEV;Initial Catalog=nsw;Integrated Security=True");
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
//await Task.Delay(2000);

if (Directory.Exists(@".\app_data\index"))
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
IJsonDocumentSource jsonStorageDocumentSource = new JsonStorageDocumentSource(new JsonStorageAreaObserverFactory(storage, scheduler, areas));
IJsonIndexManager jsonIndexManager = new JsonIndexManager(
    jsonStorageDocumentSource,
    new JsonIndexSnapshotManager(index, new ZipSnapshotStrategy(".\\app_data\\snapshots"), scheduler, "30m"),
    index
);


DateTime start = DateTime.Now;
TimeSpan completedAfter = TimeSpan.Zero;
jsonStorageDocumentSource.Initialized
    .Where(b => b)
    .FirstAsync(b =>
    {
        completedAfter = DateTime.Now - start;
        Console.WriteLine($"Done after: {completedAfter:g}");
        return b;
    });


Task run = Task.WhenAll(
    jsonIndexManager.InfoStream.ForEachAsync(Reporter.CaptureInfo),
    jsonIndexManager.RunAsync()
    //,genTask
);

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

        case 'I':
            Console.Clear();
            break;

        case 'R':
            jsonIndexManager.ResetIndexAsync();
            break;

        case 'Q':
            int matches = index.Search(new MatchAllDocsQuery()).Count();
            Console.WriteLine($"Matched documents: {matches}");
            break;

        case 'W':
            storage.Area().Insert("test", new JObject { ["name"] = "Foo" });
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
    private static DateTime lastReport = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));

    private static readonly Queue<string> messages = new Queue<string>();
    private static long eventCounter = 0;
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
        if(eventCounter++ % 10000 == 0)
            Console.Write('.');
    }

    private static string CleanLine = new string(' ', Console.BufferWidth);
    private static StringBuilder buffer = new StringBuilder();
    private static Regex nl = new Regex("\n|\r\n", RegexOptions.Compiled);

    public static void Report(bool force = false)
    {
        if(!force && DateTime.Now - lastReport < TimeSpan.FromSeconds(5))
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
        //Console.WriteLine(lastEvent.Message);
        buffer.AppendLine(lastState?.ToString());
        foreach (string message in msgs)
            buffer.AppendLine(message);
        buffer.AppendLine();
        Console.WriteLine(buffer);
    }
}
