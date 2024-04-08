using System;
using DotJEM.Json.Index2.QueryParsers.Simplified.Parser;
using static Antlr4.Runtime.Atn.SemanticContext;

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

    public override string ToString()
    {
        return $"{Name} {OpString(Operator)} {Value}";

        string OpString(FieldOperator @operator)
        {
            return @operator switch
            {
                FieldOperator.None => "<N/A>",
                FieldOperator.Equals => "=",
                FieldOperator.NotEquals => "!=",
                FieldOperator.GreaterThan => ">",
                FieldOperator.GreaterThanOrEquals => ">=",
                FieldOperator.LessThan => "<",
                FieldOperator.LessThanOrEquals => "<=",
                FieldOperator.In => "IN",
                FieldOperator.NotIn => "NOT IN",
                FieldOperator.Similar => "~",
                FieldOperator.NotSimilar => "!~",
                _ => throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null)
            };
        }
    }
}

public class RangeQuery : BaseQuery
{
    public string Name { get; }
    public Value From { get; }
    public Value To { get; }

    public RangeQuery(string name, Value from, Value to)
    {
        Name = name;
        From = from;
        To = to;
    }

    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString()
    {
        return $"{Name}: [{From} TO {To}]";
    }
}

public class MatchAnyQuery : BaseQuery
{
    public override TResult Accept<TResult, TContext>(ISimplifiedQueryAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString()
    {
        return $"*:*";
    }
}