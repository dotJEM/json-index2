using System;
using System.IO;

namespace DotJEM.Json.Index2.Snapshots
{
    
    public interface ISnapshotFile
    {
        string Name { get; }
        Stream Open();
    }

    public class SnapshotFile : ISnapshotFile
    {
        private readonly Func<Stream> streamProvider;

        public string Name { get; }
        
        public SnapshotFile(string name, Func<Stream> streamProvider)
        {
            this.streamProvider = streamProvider;
            Name = name;
        }

        public Stream Open() => streamProvider.Invoke();
    }
}