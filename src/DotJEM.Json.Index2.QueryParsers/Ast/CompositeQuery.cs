using System.Collections.Generic;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public abstract class CompositeQuery : BaseQuery
{
    private readonly BaseQuery[] queries;

    public int Count => queries.Length;
    public IEnumerable<BaseQuery> Queries => queries;

    protected CompositeQuery(BaseQuery[] queries)
    {
        this.queries = queries;
    }
}