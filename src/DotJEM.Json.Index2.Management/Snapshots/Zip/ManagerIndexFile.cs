using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Management.Info;
using DotJEM.Json.Index2.Management.Snapshots.Zip.Meta;
using DotJEM.Json.Index2.Snapshots;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management.Snapshots.Zip;

public class ManagerIndexFile : IIndexFile
{
    private readonly InfoStream<ManagerIndexFile> infoStream = new();
    public IInfoStream InfoStream => infoStream;

    private readonly ZipArchiveEntry entry;

    public string Name { get; init; }

    public ManagerIndexFile(ZipArchiveEntry entry)
    {
        this.entry = entry;
        this.Name = entry.Name;
    }

    public Stream Open()
    {
        ZipStreamWrapper wrapper = new ZipStreamWrapper(entry, this);
        wrapper.InfoStream.Subscribe(infoStream);
        infoStream.WriteFileProgressEvent(this, $"Restoring file {Name}.", new FileProgress(entry.Length, 0));
        return wrapper;
    }

    private class ZipStreamWrapper : Stream
    {
        private readonly InfoStream<ZipStreamWrapper> infoStream = new();
        public IInfoStream InfoStream => infoStream;

        private readonly ZipArchiveEntry entry;
        private readonly Stream inner;
        private readonly ManagerIndexFile file;

        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanTimeout => inner.CanTimeout;
        public override bool CanWrite => inner.CanWrite;
        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => inner.Position = value;
        }

        public override int ReadTimeout
        {
            get => inner.ReadTimeout;
            set => inner.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => inner.WriteTimeout;
            set => inner.WriteTimeout = value;
        }

        public ZipStreamWrapper(ZipArchiveEntry entry, ManagerIndexFile file)
        {
            this.entry = entry;
            this.file = file;
            this.inner = entry.Open();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            inner.Dispose();
            infoStream.WriteFileCloseEvent(file, $"File {file.Name} restored.", new FileProgress(entry.Length, entry.Length));
        }

        public override void Close()
        {
            inner.Close();
            base.Close();
        }

        public override object InitializeLifetimeService() => inner.InitializeLifetimeService();
        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            long size = entry.Length;
            long copied = 0;

            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = await ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                copied += bytesRead;
                infoStream.WriteFileProgressEvent(file, $"File {file.Name} is being restored.", new FileProgress(size, copied));
            }
        }

        public override void Flush() => inner.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => inner.FlushAsync(cancellationToken);
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => inner.BeginRead(buffer, offset, count, callback, state);
        public override int EndRead(IAsyncResult asyncResult) => inner.EndRead(asyncResult);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => inner.ReadAsync(buffer, offset, count, cancellationToken);
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => inner.BeginWrite(buffer, offset, count, callback, state);
        public override void EndWrite(IAsyncResult asyncResult) => inner.EndWrite(asyncResult);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => inner.WriteAsync(buffer, offset, count, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
        public override void SetLength(long value) => inner.SetLength(value);
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override int ReadByte() => inner.ReadByte();
        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
        public override void WriteByte(byte value) => inner.WriteByte(value);
    }
}