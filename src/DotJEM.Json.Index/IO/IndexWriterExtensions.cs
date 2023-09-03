using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index.IO
{
    public static class IndexWriterExtensions
    {
        public static IJsonIndex Create(this IJsonIndex self, JObject doc)
        {
            self.CreateWriter().Create(doc);
            return self;
        }

        public static IJsonIndex Create(this IJsonIndex self, IEnumerable<JObject> docs)
        {
            self.CreateWriter().Create(docs);
            return self;
        }

        public static IJsonIndex Update(this IJsonIndex self, JObject doc)
        {
            self.CreateWriter().Update(doc);
            return self;
        }

        public static IJsonIndex Update(this IJsonIndex self, IEnumerable<JObject> docs)
        {
            self.CreateWriter().Update(docs);
            return self;
        }
        
        public static IJsonIndex Delete(this IJsonIndex self, JObject doc)
        {
            self.CreateWriter().Delete(doc);
            return self;
        }

        public static IJsonIndex Delete(this IJsonIndex self, IEnumerable<JObject> docs)
        {
            self.CreateWriter().Delete(docs);
            return self;
        }
        
        public static IJsonIndex Flush(this IJsonIndex self, bool triggerMerge, bool applyDeletes)
        {
            self.CreateWriter().Flush(triggerMerge, applyDeletes);
            return self;
        }

        public static IJsonIndex Commit(this IJsonIndex self)
        {
            self.CreateWriter().Commit();
            return self;
        }
    }
}