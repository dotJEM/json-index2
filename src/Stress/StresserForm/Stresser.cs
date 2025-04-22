using System;
using System.Reactive.Linq;
using System.Text;
using DotJEM.Json.Index2;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Management;
using DotJEM.Json.Index2.Management.Snapshots;
using DotJEM.Json.Index2.Management.Snapshots.Zip;
using DotJEM.Json.Index2.Management.Source;
using DotJEM.Json.Index2.Snapshots;
using DotJEM.Json.Storage;
using DotJEM.Json.Storage.Configuration;
using DotJEM.ObservableExtensions.InfoStreams;
using DotJEM.Web.Scheduler;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Stress.Adapter;

namespace StresserForm;

public partial class Stresser : Form
{
    private readonly IndexWorker worker = new IndexWorker();

    public Stresser()
    {
        InitializeComponent();

        worker.Updated += WorkerOnUpdated; 
    }

    private DateTime LastUpdate = DateTime.Now;
    private void WorkerOnUpdated(object sender, EventArgs e)
    {
        TimeSpan diff = DateTime.Now - LastUpdate;
        if (diff < TimeSpan.FromMilliseconds(1000))
            return;
        LastUpdate = DateTime.Now;

        if (InvokeRequired)
        {
            Invoke(UpdateUI);
            return;
        }
        UpdateUI();
    }


    private void UpdateUI()
    {
        this.textBox1.Text = worker.Log;
        this.ctrlLinesCounter.Text = worker.LogLines;
    }

    private void ctrlStartButton_Click_1(object sender, EventArgs e)
    {
        worker.Start();

    }
}

public class IndexWorker
{
    public event EventHandler<EventArgs> Updated; 

    private bool started = false;

    private readonly IJsonIndex index;
    private readonly IStorageContext storage;
    private readonly IJsonDocumentSource source;
    private readonly IJsonIndexManager manager;

    private readonly LogBuffer logBuffer = new LogBuffer();

    public IndexWorker()
    {
        IStorageContext storage = new SqlServerStorageContext("Data Source=.\\DEV;Initial Catalog=SSN3DB;Integrated Security=True");
        storage.Configure.MapField(JsonField.Id, "id");
        storage.Configure.MapField(JsonField.ContentType, "contentType");
        storage.Configure.MapField(JsonField.Version, "$version");
        storage.Configure.MapField(JsonField.Created, "$created");
        storage.Configure.MapField(JsonField.Updated, "$updated");
        storage.Configure.MapField(JsonField.SchemaVersion, "$schemaVersion");

        if (Directory.Exists(@".\app_data\index"))
            Directory.Delete(@".\app_data\index", true);
        Directory.CreateDirectory(@".\app_data\index");
        Directory.CreateDirectory(@".\app_data\snapshots");
        
        index = new JsonIndexBuilder("main")
            .UsingSimpleFileStorage(@".\app_data\index")
            .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version, CharArraySet.Empty))
            .WithFieldResolver(new FieldResolver("id", "contentType"))
            .WithSnapshoting()
            .Build();

        IWebTaskScheduler scheduler = new WebTaskScheduler();

        string[] areas = ["content", "settings", "diagnostic", "emsaqueue", "statistic"];

        source = new JsonStorageDocumentSource(new JsonStorageAreaObserverFactory(storage, scheduler, areas));
        manager = new JsonIndexManager(
            source,
            new JsonIndexSnapshotManager(index, new ZipSnapshotStrategy(".\\app_data\\snapshots"), scheduler, "30m"),
            index
        );

        manager.InfoStream.ForEachAsync(CaptureInfoEvent);
    }

    public string Log => logBuffer.ToString();
    public string LogLines => logBuffer.ReceivedLines.ToString();

    private void CaptureInfoEvent(IInfoStreamEvent obj)
    {
        logBuffer.Append(obj.ToString());

        Updated?.Invoke(this, EventArgs.Empty);
    }

    public void Start()
    {
        if (started)
            return;

        manager.RunAsync();
        started = true;
    }

    public void Stop()
    {
        if (!started)
            return;


        started = false;
    }

}

public class LogBuffer
{
    private readonly string[] lines = new string[4096];

    private int h;
    
    public int ReceivedLines => h;

    public LogBuffer Append(string line)
    {
        lock (lines)
        {
            int i = h++ % lines.Length;
            lines[i] = line;
            return this;
        }
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        if (h >= lines.Length)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                int b = (h + lines.Length - i) % lines.Length;
                builder.AppendLine(lines[b]);
            }
        }
        else
        {
            for (int i = h-1; i >=0; i--)
                builder.AppendLine(lines[i]);
        }

        return builder.ToString();
    }
}
