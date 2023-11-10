using System;
using System.IO;

namespace DotJEM.Json.Index2.Snapshots
{
    
    public interface ILuceneFile
    {
        string Name { get; }
        long Length { get; }
        Stream Open();
    }

    public class LuceneFile : ILuceneFile
    {
        private readonly Func<Stream> streamProvider;
        public string Name { get; }
        public long Length { get; }


        public LuceneFile(string name, Func<Stream> streamProvider)
        {
            this.streamProvider = streamProvider;
            Name = name;
        }
        public Stream Open()
            => streamProvider.Invoke();
    }
}