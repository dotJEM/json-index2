using System.Collections.Generic;
using DotJEM.Json.Index2.QueryParsers.Simplified.Parser;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class OrderBy : BaseQuery
{
    private readonly OrderField[] orderFields;
    public int Count => orderFields.Length;
    public IEnumerable<OrderField> OrderFields => orderFields;

    public OrderBy(OrderField[] orderFields) 
    {
        this.orderFields = orderFields;
    }

    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}