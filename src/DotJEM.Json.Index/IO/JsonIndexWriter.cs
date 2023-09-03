using System;
using System.Collections.Generic;
using System.Linq;
using DotJEM.Json.Index.Documents;
using DotJEM.Json.Index.Util;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index.IO
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
    }

    public class JsonIndexWriter : Disposable, IJsonIndexWriter
    {
        private readonly IIndexWriterManager manager;
        private readonly ILuceneDocumentFactory factory;

        public IJsonIndex Index { get; }
        public IndexWriter UnderlyingWriter => manager.Writer;

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
            UnderlyingWriter.AddDocuments(documents);
        }

        public void Update(JObject doc) => Update(new[] { doc });
        public void Update(IEnumerable<JObject> docs)
        {
            IEnumerable<LuceneDocumentEntry> documents = factory.Create(docs);
            foreach ((Term key, Document doc) in documents)
                UnderlyingWriter.UpdateDocument(key, doc);
        }

        public void Delete(JObject doc) => Delete(new[] { doc });
        public void Delete(IEnumerable<JObject> docs)
        {
            IEnumerable<LuceneDocumentEntry> documents = factory.Create(docs);
            foreach ((Term key, Document _) in documents)
                UnderlyingWriter.DeleteDocuments(key);
        }

        public void ForceMerge(int maxSegments)
        {
            UnderlyingWriter.ForceMerge(maxSegments);
        }

        public void ForceMerge(int maxSegments, bool wait)
        {
            UnderlyingWriter.ForceMerge(maxSegments, wait);
        }

        public void ForceMergeDeletes()
        {
            UnderlyingWriter.ForceMergeDeletes();
        }

        public void ForceMergeDeletes(bool wait)
        {
            UnderlyingWriter.ForceMergeDeletes(wait);
        }

        public void Rollback()
        {
            UnderlyingWriter.Rollback();
        }

        public void Flush(bool triggerMerge, bool applyDeletes)
        {
            UnderlyingWriter.Flush(triggerMerge, applyDeletes);
        }

        public void Commit()
        {
            UnderlyingWriter.Commit();
        }

        public void PrepareCommit()
        {
            UnderlyingWriter.PrepareCommit();
        }

        public void SetCommitData(IDictionary<string, string> commitUserData)
        {
            UnderlyingWriter.SetCommitData(commitUserData);
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