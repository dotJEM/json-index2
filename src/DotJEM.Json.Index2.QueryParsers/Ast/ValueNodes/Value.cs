using System.Collections.Generic;
using DotJEM.Json.Index2.QueryParsers.Simplified.Parser;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public abstract class Value : BaseQuery
{
    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

public abstract class CompositeValue : Value
{
    public IReadOnlyList<Value> Values { get; }

    protected CompositeValue(Value[] values)
    {
        this.Values = values;
    }

    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

public class OrValue : CompositeValue
{
    public OrValue(Value[] values) : base(values)
    {
    }

    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString()
    {
        return $"( {string.Join(" OR ", Values)} )";
    }
}

public class AndValue : CompositeValue
{
    public AndValue(Value[] values) : base(values)
    {
    }

    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString()
    {
        return $"( {string.Join(" AND ", Values)} )";
    }
}
