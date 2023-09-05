using DotJEM.Json.Index2.QueryParsers.Ast;
using DotJEM.Json.Index2.QueryParsers.Simplified.Parser;

namespace DotJEM.Json.Index2.QueryParsers.Simplified.Ast.Optimizer
{
    public interface ISimplifiedQueryOptimizationVisitor : ISimplifiedQueryAstVisitor<BaseQuery, object>
    {
    }
}