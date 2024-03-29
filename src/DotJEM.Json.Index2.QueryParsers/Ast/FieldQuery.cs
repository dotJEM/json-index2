﻿using DotJEM.Json.Index2.QueryParsers.Simplified.Parser;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class FieldQuery : BaseQuery
{
    public string Name { get; }
    public FieldOperator Operator { get; }
    public Value Value { get; }

    public FieldQuery(string name, FieldOperator fieldOperator, Value value)
    {
        Name = name;
        Operator = fieldOperator;
        Value = value;
    }
    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}