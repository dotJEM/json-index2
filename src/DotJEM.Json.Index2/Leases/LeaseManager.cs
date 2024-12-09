using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DotJEM.Json.Index2.Util;

namespace DotJEM.Json.Index2.Leases;

public interface ILeaseManager<T>
{
    int Count { get; }
    ILease<T> Create(T value);
    ILease<T> Create(T value, TimeSpan limit);
    void RecallAll();
}

public class LeaseManager<T> : ILeaseManager<T>
{
    //TODO: Optimizied collection for this.
    private readonly List<IRecallableLease<T>> leases = new();
    private readonly object leasesPadLock = new();

    public int Count => leases.Count;

    public ILease<T> Create(T value)
    {
        return Add(new Lease<T>(value, OnReturned));
    }

    public ILease<T> Create(T value, TimeSpan limit)
    {
        return Add(new TimeLimitedLease<T>(value, OnReturned, limit));
    }

    public void RecallAll()
    {
        lock (leasesPadLock)
        {
            IRecallableLease<T>[] copy = leases.ToArray();
            leases.Clear();

            foreach (IRecallableLease<T> lease in copy)
                lease.Terminate();
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

    private class Lease<T> : Disposable, IRecallableLease<T>
    {
        public event EventHandler<EventArgs> Terminated;

        private readonly T value;
        private readonly Action<Lease<T>> onReturned;

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

        public Lease(T value, Action<Lease<T>> onReturned)
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
            IsTerminated = true;
            Terminated?.Invoke(this, EventArgs.Empty);
            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            onReturned(this);
            base.Dispose(disposing);
        }
    }

    private class TimeLimitedLease<T> : Disposable, IRecallableLease<T>
    {
        public event EventHandler<EventArgs> Terminated;

        private readonly T value;
        private readonly Action<TimeLimitedLease<T>> onReturned;
        private readonly TimeSpan timeLimit;
        private readonly long leaseTime = Stopwatch.GetTimestamp();
        private readonly AutoResetEvent handle = new(false);

        public bool IsExpired => (ElapsedMs > timeLimit.Milliseconds) || IsDisposed;
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
                if (IsExpired)
                {
                    throw new LeaseExpiredException($"Lease is expired either because the time limit '{timeLimit}' has exceeded or the lease was returned.");
                }
                return value;
            }
        }

        public TimeLimitedLease(T value, Action<TimeLimitedLease<T>> onReturned, TimeSpan timeLimit)
        {
            this.value = value;
            this.onReturned = onReturned;
            this.timeLimit = timeLimit;
        }

        public bool TryRenew()
        {
            return false;
        }

        public void Terminate()
        {
            IsTerminated = true;
            Terminated?.Invoke(this, EventArgs.Empty);
            Wait();
            Dispose();
        }

        public void Wait()
        {
            if (IsExpired)
                return;

            handle.WaitOne(TimeSpan.FromSeconds(6) - TimeSpan.FromMilliseconds(ElapsedMs));
        }

        protected override void Dispose(bool disposing)
        {
            onReturned(this);
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