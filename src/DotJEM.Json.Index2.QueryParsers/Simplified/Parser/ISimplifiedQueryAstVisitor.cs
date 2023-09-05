using DotJEM.Json.Index2.QueryParsers.Ast;

namespace DotJEM.Json.Index2.QueryParsers.Simplified.Parser
{
    public interface ISimplifiedQueryAstVisitor<out TResult, in TContext>
    {
        TResult Visit(BaseQuery ast, TContext context);
        TResult Visit(NotQuery ast, TContext context);

        TResult Visit(OrderedQuery ast, TContext context);

        TResult Visit(OrderBy ast, TContext context);
        TResult Visit(OrderField ast, TContext context);

        TResult Visit(CompositeQuery ast, TContext context);
        TResult Visit(OrQuery ast, TContext context);
        TResult Visit(AndQuery ast, TContext context);
        TResult Visit(ImplicitCompositeQuery ast, TContext context);
        TResult Visit(FieldQuery ast, TContext context);
    }
}