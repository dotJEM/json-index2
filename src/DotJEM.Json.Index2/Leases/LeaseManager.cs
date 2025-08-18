using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DotJEM.Json.Index2.Util;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Leases;

public interface ILeaseManager<T>
{
    IInfoStream InfoStream { get; }

    int Count { get; }
    ILease<T> Create(T value);
    IEnumerable<T> RecallAll();
}

public class LeaseManager<T> : ILeaseManager<T>
{
    //TODO: Optimizied collection for this.
    private readonly List<Lease> leases = [];
    private readonly object leasesPadLock = new();
    private readonly InfoStream<LeaseManager<T>> infoStream = new();
    public IInfoStream InfoStream => infoStream;

    /// <summary>
    /// 
    /// </summary>
    public int Count
    {
        get
        {
            lock (leasesPadLock)
            {
                return leases.Count;
            }
        }
    }

    public ILease<T> Create(T value)
    {
        lock (leasesPadLock)
        {
            Lease lease = new Lease(value, OnReturned);
            leases.Add(lease);
            infoStream.WriteDebug($"Adding new lease, total leases = {leases.Count}");
            return lease;
        }
    }

    public IEnumerable<T> RecallAll()
    {
        lock (leasesPadLock)
        {
            Lease[] copy = leases.ToArray();
            leases.Clear();

            infoStream.WriteDebug($"Recalling lease, total leases = {copy.Length}");

            T[] values = Array.ConvertAll(copy, x => x.Value);
            foreach (Lease lease in copy)
                lease.Terminate();
            return values;
        }

    }


    private void OnReturned(Lease obj)
    {
        lock (leasesPadLock)
        {
            leases.Remove(obj);
            infoStream.WriteDebug($"Returning lease, total leases = {leases.Count}");
        }
    }

    private class Lease : Disposable, ILease<T>
    {
        public event EventHandler<EventArgs> Terminated;

        private readonly T value;
        private readonly Action<Lease> onReturned;
        private readonly ManualResetEventSlim returned = new ManualResetEventSlim();

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
                return value;
            }
        }

        public Lease(T value, Action<Lease> onReturned)
        {
            this.value = value;
            this.onReturned = onReturned;
        }

        public void Terminate()
        {
            returned.Wait(500);
            IsTerminated = true;
            Terminated?.Invoke(this, EventArgs.Empty);
            Dispose();
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