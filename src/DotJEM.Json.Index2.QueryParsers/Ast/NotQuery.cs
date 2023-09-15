using DotJEM.Json.Index2.QueryParsers.Simplified.Parser;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class NotQuery : BaseQuery
{
    public BaseQuery Not { get; }

    public NotQuery(BaseQuery not)
    {
        Not = not;
    }

    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}