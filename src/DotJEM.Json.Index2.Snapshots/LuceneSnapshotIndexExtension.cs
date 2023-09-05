using DotJEM.Json.Index2.Configuration;

namespace DotJEM.Json.Index2.Snapshots
{
    public static class LuceneSnapshotIndexExtension
    {
        private static readonly IndexSnapshotHandler defaultHandler = new IndexSnapshotHandler();

        public static ISnapshot Snapshot(this IJsonIndex self, ISnapshotTarget target)
        {
            IIndexSnapshotHandler handler = self.Configuration.Get<IIndexSnapshotHandler>() ?? defaultHandler;
            return handler.Snapshot(self, target);
        }

        public static ISnapshot Restore(this IJsonIndex self, ISnapshotSource source)
        {
            IIndexSnapshotHandler handler = self.Configuration.Get<IIndexSnapshotHandler>() ?? defaultHandler;
            return handler.Restore(self, source);
        }
    }
}
