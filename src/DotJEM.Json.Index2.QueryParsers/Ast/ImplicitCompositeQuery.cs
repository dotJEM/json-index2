using DotJEM.Json.Index2.QueryParsers.Simplified.Parser;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class ImplicitCompositeQuery : CompositeQuery
{
    public ImplicitCompositeQuery(BaseQuery[] queries) : base(queries)
    {
    }

    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString()
    {
        return $"( {string.Join(", ", Queries)} )";
    }
}