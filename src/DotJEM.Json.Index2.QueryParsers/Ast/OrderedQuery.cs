using DotJEM.Json.Index2.QueryParsers.Simplified.Parser;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class OrderedQuery : BaseQuery
{
    public BaseQuery Query { get; }
    public BaseQuery Ordering { get; }

    public OrderedQuery(BaseQuery query, BaseQuery order)
    {
        this.Query = query;
        this.Ordering = order;
    }

    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString()
    {
        if (Ordering != null)
            return $"{Query} ORDER BY {Ordering}";
        return Query.ToString();
    }
}