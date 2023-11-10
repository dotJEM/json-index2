using Lucene.Net.Store;

namespace DotJEM.Json.Index2.Snapshots.Streams;

public static class LuceneDirectoryStreamExtensions
{
    public static IndexInputStream OpenInputStream(this Directory self, string fileName, IOContext context = null)
        => new (fileName, self.OpenInput(fileName, context ?? IOContext.DEFAULT));

    public static IndexOutputStream CreateOutputStream(this Directory self, string fileName, IOContext context = null) 
        => new (fileName, self.CreateOutput(fileName, context ?? IOContext.DEFAULT));
}