using System;

namespace DotJEM.Json.Index2.Leases;

public interface ILease<out T> : IDisposable
{
    event EventHandler<EventArgs> Terminated; 

    T Value { get; }
    bool IsExpired { get; }
    bool IsTerminated { get; }
    bool TryRenew();

    TOut WithLock<TOut>(Func<T, TOut> func);
    void WithLock(Action<T> action);
}


public interface ILessor<out T>
{
    ILease<T> Lease();
}
