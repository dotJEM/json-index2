using System;

namespace DotJEM.Json.Index2.Util;

public class Disposable : IDisposable
{
    protected bool disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        disposed = true;
    }

    ~Disposable()
    {
        Dispose(false);
    }

    protected void EnsureNotDisposed()
    {
        if(disposed) throw new ObjectDisposedException(GetType().FullName);
    }
}