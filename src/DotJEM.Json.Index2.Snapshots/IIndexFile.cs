using System;
using System.IO;

namespace DotJEM.Json.Index2.Snapshots;

public interface IIndexFile
{
    string Name { get; }
    Stream Open();
}

public class IndexFile : IIndexFile
{
    private readonly Func<Stream> streamProvider;

    public string Name { get; }
        
    public IndexFile(string name, Func<Stream> streamProvider)
    {
        this.streamProvider = streamProvider;
        Name = name;
    }

    public Stream Open() => streamProvider.Invoke();
}