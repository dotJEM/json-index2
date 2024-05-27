using System;
using System.Collections.Generic;
using System.Linq;
using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Leases;
using DotJEM.Json.Index2.Util;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.IO
{
    public interface IJsonIndexWriter : IDisposable
    {
        void Create(JObject doc);
        void Create(IEnumerable<JObject> docs);
        void Update(JObject doc);
        void Update(IEnumerable<JObject> docs);
        void Delete(JObject doc);
        void Delete(IEnumerable<JObject> docs);
        void Commit();
        void Flush(bool triggerMerge, bool applyDeletes);
    }

    public class JsonIndexWriter : Disposable, IJsonIndexWriter
    {
        private readonly IIndexWriterManager manager;
        private readonly ILuceneDocumentFactory factory;

        public IJsonIndex Index { get; }
        //public IndexWriter UnderlyingWriter => manager.Writer;

        public JsonIndexWriter(IJsonIndex index, ILuceneDocumentFactory factory, IIndexWriterManager manager)
        {
            Index = index;
            this.factory = factory;
            this.manager = manager;
        }

        public void Create(JObject doc) => Create(new[] { doc });

        public void Create(IEnumerable<JObject> docs)
        {
            IEnumerable<Document> documents = factory
                .Create(docs)
                .Select(tuple => tuple.Document);
            WithLease(writer => writer.AddDocuments(documents));
        }

        public void Update(JObject doc) => Update(new[] { doc });
        public void Update(IEnumerable<JObject> docs)
        {
            IEnumerable<LuceneDocumentEntry> documents = factory.Create(docs);
            WithLease(writer => {
                foreach ((Term key, Document doc) in documents)
                    writer.UpdateDocument(key, doc);
            });
        }

        public void Delete(JObject doc) => Delete(new[] { doc });
        public void Delete(IEnumerable<JObject> docs)
        {
            IEnumerable<LuceneDocumentEntry> documents = factory.Create(docs);
            WithLease(writer => {
                foreach ((Term key, Document _) in documents)
                    writer.DeleteDocuments(key);
            });
        }

        public void ForceMerge(int maxSegments)
            => WithLease(writer => writer.ForceMerge(maxSegments));

        public void ForceMerge(int maxSegments, bool wait)
            => WithLease(writer => writer.ForceMerge(maxSegments, wait));

        public void ForceMergeDeletes()
            => WithLease(writer => writer.ForceMergeDeletes());

        public void ForceMergeDeletes(bool wait)
            => WithLease(writer => writer.ForceMergeDeletes(wait));

        public void Rollback()
            => WithLease(writer => writer.Rollback());

        public void Flush(bool triggerMerge, bool applyDeletes)
            => WithLease(writer => writer.Flush(triggerMerge, applyDeletes));

        public void Commit()
            => WithLease(writer => writer.Commit());

        public void PrepareCommit()
            => WithLease(writer => writer.PrepareCommit());

        public void SetCommitData(IDictionary<string, string> commitUserData)
            => WithLease(writer => writer.SetCommitData(commitUserData));

        private void WithLease(Action<IndexWriter> action)
        {
            using ILease<IndexWriter> lease =manager.Lease();
            action(lease.Value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Commit();
            }
            base.Dispose(disposing);
        }
    }
}