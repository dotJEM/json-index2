﻿namespace DotJEM.Json.Index2.Snapshots
{
    public static class LuceneSnapshotIndexExtension
    {
        public static ISnapshot Snapshot(this IJsonIndex self, ISnapshotTarget target)
        {
            IIndexSnapshotHandler handler = self.Services.Resolve<IIndexSnapshotHandler>() ?? new IndexSnapshotHandler();
            return handler.Snapshot(self, target);
        }

        public static ISnapshot Restore(this IJsonIndex self, ISnapshotSource source)
        {
            IIndexSnapshotHandler handler = self.Services.Resolve<IIndexSnapshotHandler>() ?? new IndexSnapshotHandler();
            return handler.Restore(self, source);
        }
    }
}