﻿using DotJEM.Json.Index2.QueryParsers.Simplified.Parser;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class OrQuery : CompositeQuery {
    public OrQuery(BaseQuery[] queries) 
        : base(queries)
    {
    }

    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString()
    {
        return $"( {string.Join(" OR ", Queries)} )";
    }
}