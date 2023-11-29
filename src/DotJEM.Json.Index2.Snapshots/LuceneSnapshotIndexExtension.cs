using System;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Configuration;

namespace DotJEM.Json.Index2.Snapshots;

public static class LuceneSnapshotIndexExtension
{
    public static async Task<ISnapshot> TakeSnapshotAsync(this IJsonIndex self, ISnapshotStorage storage, bool leaveOpen = false)
    {
        IIndexSnapshotHandler handler = self.ResolveSnapshotHandler();
        return await handler.TakeSnapshotAsync(self, storage, leaveOpen);
    }

    public static async Task<ISnapshot> RestoreSnapshotAsync(this IJsonIndex self, ISnapshot snapshot, bool leaveOpen = false)
    {
        IIndexSnapshotHandler handler = self.ResolveSnapshotHandler();
        return await handler.RestoreSnapshotAsync(self, snapshot, leaveOpen);
    }

    public static IJsonIndexBuilder WithSnapshoting(this IJsonIndexBuilder self)
        => self.WithSnapshoting< IndexSnapshotHandler>();

    public static IJsonIndexBuilder WithSnapshoting<T>(this IJsonIndexBuilder self) where T : IIndexSnapshotHandler, new()
        => self.WithSnapshoting(new T());

    public static IJsonIndexBuilder WithSnapshoting(this IJsonIndexBuilder self, IIndexSnapshotHandler handler) 
        => self.TryWithService(handler);


    private static IIndexSnapshotHandler ResolveSnapshotHandler(this IJsonIndex self)
    {
        return self.Configuration.Get<IIndexSnapshotHandler>() ?? 
               throw new InvalidOperationException("Snapshot handler not configured.");
    }
}