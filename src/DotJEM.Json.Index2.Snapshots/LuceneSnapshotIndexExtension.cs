using System.Threading.Tasks;
using DotJEM.Json.Index2.Configuration;

namespace DotJEM.Json.Index2.Snapshots
{
    public static class LuceneSnapshotIndexExtension
    {
        private static readonly IndexSnapshotHandler defaultHandler = new IndexSnapshotHandler();

        public static async Task<ISnapshot> TakeSnapshotAsync(this IJsonIndex self, ISnapshotTarget target)
        {
            IIndexSnapshotHandler handler = self.Configuration.Get<IIndexSnapshotHandler>() ?? defaultHandler;
            return await handler.TakeSnapshotAsync(self, target);
        }

        public static async Task<ISnapshot> RestoreSnapshotAsync(this IJsonIndex self, ISnapshotSource source)
        {
            IIndexSnapshotHandler handler = self.Configuration.Get<IIndexSnapshotHandler>() ?? defaultHandler;
            return await handler.RestoreSnapshotAsync(self, source);
        }
    }
}
