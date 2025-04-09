using DotJEM.Json.Index2.Documents.Data;
using Lucene.Net.Index;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotJEM.Json.Index2.Documents.Meta;

public static class IndexableJsonFieldExt
{
    public static IIndexableJsonFieldInfo Info(this IIndexableJsonField field)
    {
        return new IndexableJsonFieldInfo(field.SourcePath, field.TokenType, field.SourceType, field.Strategy, field.LuceneFields.Select(f => f.Info()));
    }

    public static IIndexableFieldInfo Info(this IIndexableField field)
    {
        return new IndexableFieldInfo(field.Name, field.GetType(), field.IndexableFieldType);
    }
}

public interface IContentTypeInfo
{
    string Name { get; }
    IEnumerable<IIndexableJsonFieldInfo> FieldInfos { get; }
    //IContentTypeInfo Merge(IContentTypeInfo other);
    //IIndexableJsonFieldInfo Lookup(string fieldName);
    IContentTypeInfo Add(IIndexableJsonFieldInfo field);
}

public class ContentTypeInfo : IContentTypeInfo
{
    private readonly Dictionary<string, IIndexableJsonFieldInfo> fields = new();
    private Dictionary<string, string> indexedFields = new();

    public IEnumerable<IIndexableJsonFieldInfo> FieldInfos => fields.Values;

    public string Name { get; }

    public ContentTypeInfo(string name)
    {
        Name = name;
    }

    public IContentTypeInfo Add(IIndexableJsonFieldInfo field)
    {
        lock (fields)
        {
            if (!fields.TryGetValue(field.SourcePath, out IIndexableJsonFieldInfo existing))
                fields.Add(field.SourcePath, field);
            else
            {
            }
        }

        lock (indexedFields)
        {
            foreach (IIndexableFieldInfo info in field.LuceneFieldInfos)
            {
                indexedFields[info.FieldName] = field.SourcePath;
            }
        }

        return this;
    }
}

public interface IIndexableJsonFieldInfo
{
    string SourcePath { get; }
    Type SourceType { get; }
    Type Strategy { get; }
    JTokenType TokenType { get; }

    IEnumerable<IIndexableFieldInfo> LuceneFieldInfos { get; }
}

public sealed class IndexableJsonFieldInfo : IIndexableJsonFieldInfo
{
    public string SourcePath { get; }
    public Type SourceType { get; }
    public Type Strategy { get; }
    public JTokenType TokenType { get; }
    public IEnumerable<IIndexableFieldInfo> LuceneFieldInfos { get; }

    public IndexableJsonFieldInfo(string sourcePath, JTokenType tokenType, Type sourceType, Type strategy, IEnumerable<IIndexableFieldInfo> luceneFieldInfos)
    {
        SourcePath = sourcePath;
        TokenType = tokenType;
        SourceType = sourceType;
        Strategy = strategy;
        LuceneFieldInfos = luceneFieldInfos;
    }
}

public interface IIndexableFieldInfo
{
    string FieldName { get; }
    Type Type { get; }
    IIndexableFieldType FieldType { get; }
}

public sealed class IndexableFieldInfo : IIndexableFieldInfo
{
    public string FieldName { get; }
    public Type Type { get; }
    public IIndexableFieldType FieldType { get; }

    public IndexableFieldInfo(string fieldName, Type type, IIndexableFieldType fieldType)
    {
        FieldName = fieldName;
        Type = type;
        FieldType = fieldType;
    }
}