using System;
using System.IO;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Searching;
using DotJEM.Json.Index2.Snapshots.Streams;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Directory = Lucene.Net.Store.Directory;

namespace DotJEM.Json.Index2.Snapshots.Test;

public class SnapshotsTest
{
       
    [Test]

    public async Task WriteContext_MakesDocumentsAvailable()
    {      IJsonIndex index = new JsonIndexBuilder("myIndex")
            .UsingMemmoryStorage()
            .WithAnalyzer(cfg => new StandardAnalyzer(cfg.Version))
            .WithFieldResolver(new FieldResolver("Id", "Type"))
            .Build();

            
        IJsonIndexWriter writer = index.CreateWriter();

        writer.Create(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000001"), Type = "Person", Name = "John", LastName = "Doe", Area = "Foo" }));
        writer.Create(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000002"), Type = "Person", Name = "Peter", LastName = "Pan", Area = "Foo" }));
        writer.Create(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000003"), Type = "Person", Name = "Alice", Area = "Foo" }));
        writer.Create(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000004"), Type = "Car", Brand = "Ford", Model = "Mustang", Number = 5, Area = "Foo" }));
        writer.Create(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000005"), Type = "Car", Brand = "Dodge", Model = "Charger", Number = 10, Area = "Foo" }));
        writer.Create(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000006"), Type = "Car", Brand = "Chevrolet", Model = "Camaro", Number = 15, Area = "Foo" }));
        writer.Create(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000007"), Type = "Flower", Name = "Lilly", Meaning = "Majesty", Number = 5, Area = "Foo" }));
        writer.Create(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000008"), Type = "Flower", Name = "Freesia", Meaning = "Innocence", Number = 10, Area = "Foo" }));
        writer.Create(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000009"), Type = "Flower", Name = "Aster", Meaning = "Patience", Number = 15, Area = "Foo" }));
        writer.Commit();

        FakeSnapshotTarget target = new FakeSnapshotTarget();
        await index.TakeSnapshotAsync(target);

        ISnapshotSource source = target.LastCreatedWriter.GetSource();
        Assert.That(await index.RestoreSnapshotAsync(source), Is.Not.Null);

        Assert.That(index.Search(new MatchAllDocsQuery()).Count(), Is.EqualTo(9));
    }
}

  public class FakeSnapshotTarget : ISnapshotTarget
    {
        public FakeSnapshotWriter LastCreatedWriter { get; private set; }

        public IReadOnlyCollection<ISnapshot> Snapshots { get; }

        public ISnapshotWriter Open(IndexCommit commit)
        {
            return LastCreatedWriter= new FakeSnapshotWriter(commit.Generation);
        }
    }

    public class FakeSnapshotWriter : ISnapshotWriter
    {
        private readonly long generation;
        private readonly List<ISnapshotFile> files = new();

        public ISnapshot Snapshot { get; }

        public FakeSnapshotWriter(long generation)
        {
            this.generation = generation;
        }

        public async Task WriteFileAsync(string fileName, Directory dir)
        {
            IndexInputStream? stream = dir.OpenInputStream(fileName, IOContext.READ_ONCE);
            FakeFile file = new FakeFile(stream.FileName);
            await stream.CopyToAsync(file.Stream);
            file.Stream.Flush();
            files.Add(file);
        }

        public class FakeFile : ISnapshotFile
        {

            public string Name { get; }
            public long Length { get; }
            public MemoryStream Stream { get; } = new MemoryStream();

            public FakeFile(string name)
            {
                Name = name;
            }
            public Stream Open()
            {
                Stream.Seek(0, SeekOrigin.Begin);
                return Stream;
            }

        }

        public ISnapshotSource GetSource()
        {
            return new FakeSnapshotSource(new FakeSnapshot(generation,  files));
        }

        public void Dispose()
        {
        }
    }

    public class FakeSnapshotSource : ISnapshotSource
    {
        private readonly FakeSnapshot snapshot;

        public FakeSnapshotSource(FakeSnapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        public IReadOnlyCollection<ISnapshot> Snapshots { get; }


        public ISnapshotReader Open()
        {
            return new FakeSnapshotReader(snapshot);
        }
    }

    public class FakeSnapshotReader : ISnapshotReader
    {
        private readonly FakeSnapshot snapshot;
        public ISnapshot Snapshot => snapshot;

        public FakeSnapshotReader(FakeSnapshot snapshot)
        {
            this.snapshot = snapshot;
        }
        public IEnumerable<ISnapshotFile> ReadFiles()
        {
            return snapshot.Files;
        }

        public void Dispose()
        {
        }

    }

    public class FakeSnapshot : ISnapshot
    {
        public long Generation { get; }
        public ICollection<ISnapshotFile> Files { get; }

        public FakeSnapshot(long generation,  ICollection<ISnapshotFile> files)
        {
            Generation = generation;
            Files = files;

        }

        public void Dispose()
        {
        }
    }