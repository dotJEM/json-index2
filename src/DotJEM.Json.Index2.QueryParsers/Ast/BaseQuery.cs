using System.Collections.Generic;
using System.Linq;
using DotJEM.Json.Index2.QueryParsers.Simplified.Parser;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public abstract class BaseQuery
{
    private readonly Dictionary<string, object> metaData = new Dictionary<string, object>();

    public IEnumerable<(string, object)> MetaData => metaData.Select(kv => (kv.Key, kv.Value));

    public object Get(string key) => metaData[key];
    public bool TryGetValue(string key, out object value) => metaData.TryGetValue(key, out value);

    public TData Add<TData>(string key, TData value)
    {
        metaData.Add(key, value);
        return value;
    }

    public bool ContainsKey(string key) => metaData.ContainsKey(key);


    public TData GetAs<TData>(string key) => (TData)metaData[key];
    public bool TryGetAs<TData>(string key, out TData value)
    {
        if (metaData.TryGetValue(key, out object val))
        {
            value = (TData) val;
            return true;
        }
        value = default;
        return false;
    }

    public abstract TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context);
}

