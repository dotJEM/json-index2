using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DotJEM.Json.Index2.Util;

namespace DotJEM.Json.Index2.Leases;

public interface ILeaseManager<T>
{
    int Count { get; }
    ILease<T> Create(T value);
    ILease<T> Create(T value, TimeSpan limit);
    IEnumerable<T> RecallAll();
}

public class LeaseManager<T> : ILeaseManager<T>
{
    //TODO: Optimizied collection for this.
    private readonly List<IRecallableLease<T>> leases = new();
    private readonly object leasesPadLock = new();

    public int Count => leases.Count;

    public ILease<T> Create(T value)
    {
        return Add(new Lease(value, OnReturned));
    }

    public ILease<T> Create(T value, TimeSpan limit)
    {
        return Add(new TimeLimitedLease(value, OnReturned, limit));
    }

    public IEnumerable<T> RecallAll()
    {
        IRecallableLease<T>[] copy = CopyLeases();
        T[] values = Array.ConvertAll(copy, x => x.Value);
        foreach (IRecallableLease<T> lease in copy)
            lease.Terminate();
        return values;

        IRecallableLease<T>[] CopyLeases()
        {
            lock (leasesPadLock)
            {
                IRecallableLease<T>[] copy = leases.ToArray();
                leases.Clear();
                return copy;
            }
        }
    }


    private ILease<T> Add(IRecallableLease<T> lease)
    {
        lock (leasesPadLock)
        {
            leases.Add(lease);
        }
        return lease;
    }

    private void OnReturned(IRecallableLease<T> obj)
    {
        lock (leasesPadLock)
        {
            leases.Remove(obj);
        }
    }

    private interface IRecallableLease<T> : ILease<T>
    {
        void Terminate();
    }

    private class Lease : Disposable, IRecallableLease<T>
    {
        public event EventHandler<EventArgs> Terminated;

        private readonly T value;
        private readonly Action<Lease> onReturned;
        private readonly ManualResetEventSlim returned = new ManualResetEventSlim();

        public bool IsExpired => IsDisposed;
        public bool IsTerminated { get; private set; }

        public T Value
        {
            get
            {
                if (IsTerminated)
                {
                    throw new LeaseTerminatedException($"This lease has been terminated.");
                }
                if (IsDisposed)
                {
                    throw new LeaseDisposedException($"This lease has been disposed.");
                }
                if (IsExpired)
                {
                    throw new LeaseExpiredException("Lease is expired as it was returned.");
                }
                return value;
            }
        }

        public Lease(T value, Action<Lease> onReturned)
        {
            this.value = value;
            this.onReturned = onReturned;
        }

        public bool TryRenew()
        {
            return false;
        }

        public void Terminate()
        {
            returned.Wait(500);
            IsTerminated = true;
            Terminated?.Invoke(this, EventArgs.Empty);
            Dispose(false);
            onReturned(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                returned.Set();
                onReturned(this);
            }
            returned.Dispose();
            base.Dispose(disposing);
        }
    }

    private class TimeLimitedLease : Disposable, IRecallableLease<T>
    {
        public event EventHandler<EventArgs> Terminated;

        private readonly T value;
        private readonly Action<TimeLimitedLease> onReturned;
        private readonly long timeLimitMilliseconds;
        private readonly long leaseTime = Stopwatch.GetTimestamp();
        private readonly AutoResetEvent returned = new(false);

        public bool IsExpired => (ElapsedMs > timeLimitMilliseconds) || IsDisposed;
        public bool IsTerminated { get; private set; }
        private long ElapsedMs => (long)((Stopwatch.GetTimestamp() - leaseTime) / (Stopwatch.Frequency / (double)1000));

        public T Value
        {
            get
            {
                if (IsTerminated)
                {
                    throw new LeaseTerminatedException($"This lease has been terminated.");
                }
                if (IsDisposed)
                {
                    throw new LeaseDisposedException($"This lease has been disposed.");
                }
                long elapsed = ElapsedMs;
                if (ElapsedMs > timeLimitMilliseconds)
                {
                    throw new LeaseExpiredException($"Lease is expired either because the time limit '{timeLimitMilliseconds}'" +
                                                    $" was exceeded by: '{elapsed - timeLimitMilliseconds}'.");
                }
                return value;
            }
        }

        public TimeLimitedLease(T value, Action<TimeLimitedLease> onReturned, TimeSpan timeLimit)
        {
            this.value = value;
            this.onReturned = onReturned;
            this.timeLimitMilliseconds = (long)timeLimit.TotalMilliseconds;
        }
        
        public bool TryRenew()
        {
            return false;
        }

        public void Terminate()
        {
            Wait();
            IsTerminated = true;
            Terminated?.Invoke(this, EventArgs.Empty);
            Dispose(false);
            onReturned(this);
        }

        private void Wait()
        {
            if (IsExpired)
                return;

            TimeSpan remaining = TimeSpan.FromSeconds(6) - TimeSpan.FromMilliseconds(ElapsedMs);
            if(remaining > TimeSpan.Zero)
                returned.WaitOne(TimeSpan.FromSeconds(6) - TimeSpan.FromMilliseconds(ElapsedMs));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                returned.Set();
                onReturned(this);
            }
            returned.Dispose();
            base.Dispose(disposing);
        }
    }
}
public class LeaseExpiredException : Exception
{
    public LeaseExpiredException(string message) : base(message)
    {
    }

    public LeaseExpiredException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class LeaseTerminatedException : Exception
{
    public LeaseTerminatedException(string message) : base(message)
    {
    }

    public LeaseTerminatedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class LeaseDisposedException : ObjectDisposedException
{
    public LeaseDisposedException(string message) : base(message)
    {
    }

    public LeaseDisposedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}