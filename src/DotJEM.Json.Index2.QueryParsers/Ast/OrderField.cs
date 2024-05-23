using System;
using DotJEM.Json.Index2.QueryParsers.Simplified.Parser;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class OrderField : BaseQuery
{
    public string Name { get; }

    public FieldOrder SpecifiedOrder { get; }

    public OrderField(string name, FieldOrder order)
    {
        Name = name;
        SpecifiedOrder = order;
    }
    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString()
    {
        return SpecifiedOrder switch
        {
            FieldOrder.None => Name,
            FieldOrder.Ascending => $"{Name} ASC",
            FieldOrder.Descending => $"{Name} DESC",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}