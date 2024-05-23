using System;

namespace DotJEM.Json.Index2.Util;

public class Disposable : IDisposable
{
    protected bool IsDisposed { get; private set; }

    public void Dispose()
    {
        if(IsDisposed)
            return;

        Dispose(true);
        GC.SuppressFinalize(this);
        IsDisposed = true;
    }

    protected virtual void Dispose(bool disposing)
    {
        IsDisposed = true;
    }

    ~Disposable()
    {
        Dispose(false);
    }

    protected void EnsureNotDisposed()
    {
        if(IsDisposed) throw new ObjectDisposedException(GetType().FullName);
    }
}