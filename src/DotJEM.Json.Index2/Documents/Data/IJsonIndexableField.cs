using System;
using System.Collections.Generic;
using System.Linq;
using J2N.Collections.ObjectModel;
using Lucene.Net.Index;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Documents.Data
{
    public interface IIndexableJsonField
    {
        string SourcePath { get; }
        Type Strategy { get; }
        Type SourceType { get; }
        JTokenType TokenType { get; }
        IReadOnlyList<IIndexableField> LuceneFields { get; }
    }

    public class IndexableJsonField<T> : IIndexableJsonField
    {
        public string SourcePath { get; }
        public Type Strategy { get; }
        public Type SourceType { get; } = typeof(T);
        public JTokenType TokenType { get; }

        public IReadOnlyList<IIndexableField> LuceneFields { get; }

        public IndexableJsonField(string sourcePath, JTokenType tokenType, IIndexableField field, Type strategy)
            : this(sourcePath, tokenType, new[] { field }, strategy)
        {
        }

        public IndexableJsonField(string sourcePath, JTokenType tokenType, IEnumerable<IIndexableField> fields, Type strategy)
        {
            SourcePath = sourcePath;
            Strategy = strategy;
            TokenType = tokenType;
            LuceneFields = new ReadOnlyList<IIndexableField>(fields.ToList());
        }
    }

  
}