using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Antlr4.Runtime.Tree;
using DotJEM.Json.Index2.QueryParsers.Ast;

namespace DotJEM.Json.Index2.QueryParsers.Simplified.Parser;

public class SimplifiedParserVisitor : SimplifiedBaseVisitor<BaseQuery>
{
    private DateTime now;
        
    public override BaseQuery VisitQuery(SimplifiedParser.QueryContext context)
    {
        now = DateTime.Now;
        BaseQuery query = context.clause.Accept(this);
        BaseQuery order = context.order?.Accept(this);
        return new OrderedQuery(query, order);
    }

    public override BaseQuery VisitDefaultClause(SimplifiedParser.DefaultClauseContext context)
    {
        List<BaseQuery> fragments = Visit(context.children);
        // Note: If we only have a single fragment, just return that.
        return fragments.Count < 2 
            ? fragments.SingleOrDefault() 
            : new ImplicitCompositeQuery(fragments.ToArray());
    }


    public override BaseQuery VisitOrClause(SimplifiedParser.OrClauseContext context)
    {
        List<BaseQuery> fragments = Visit(context.children);
        // Note: If we only have a single fragment, there was no OR, just return the fragment.
        return fragments.Count < 2
            ? fragments.SingleOrDefault() 
            : new OrQuery(fragments.ToArray());
    }

    public override BaseQuery VisitOrFieldValue(SimplifiedParser.OrFieldValueContext context)
    {
        List<BaseQuery> fragments = Visit(context.children);
        // Note: If we only have a single fragment, there was no OR, just return the fragment.
        return fragments.Count < 2
            ? fragments.SingleOrDefault()
            : new OrValue(fragments.Cast<Value>().ToArray());
    }

    public override BaseQuery VisitAndClause(SimplifiedParser.AndClauseContext context)
    {
        List<BaseQuery> fragments = Visit(context.children);
        // Note: If we only have a single fragment, there was no OR, just return the fragment.
        return fragments.Count < 2
            ? fragments.SingleOrDefault()
            : new AndQuery(fragments.ToArray());
    }

    public override BaseQuery VisitAndFieldValue(SimplifiedParser.AndFieldValueContext context)
    {
        List<BaseQuery> fragments = Visit(context.children);
        // Note: If we only have a single fragment, there was no OR, just return the fragment.
        return fragments.Count < 2
            ? fragments.SingleOrDefault()
            : new AndValue(fragments.Cast<Value>().ToArray());
    }

    public override BaseQuery VisitNotClause(SimplifiedParser.NotClauseContext context)
    {
        List<BaseQuery> fragments = Visit(context.children);
        //Note: There was actually no Not Clause.
        if (fragments.Count < 2)
            return fragments.SingleOrDefault();

        for (int i = 1; i < fragments.Count; i++)
            fragments[i] = new NotQuery(fragments[i]);

        return new AndQuery(fragments.ToArray());
    }

    public override BaseQuery VisitWildcardValue(SimplifiedParser.WildcardValueContext context)
    {
        return new WildcardValue(context.GetText());
    }

    public override BaseQuery VisitStarValue(SimplifiedParser.StarValueContext context)
    {
        return new MatchAllValue();
    }

    public override BaseQuery VisitPureValue(SimplifiedParser.PureValueContext context)
    {
        switch (context)
        {
            case SimplifiedParser.DateContext _: return new DateValue(DateTime.ParseExact(context.GetText(), "YYYY-MM-DD", CultureInfo.InvariantCulture));
            case SimplifiedParser.DateTimeContext _: return new DateTimeValue(DateTime.Parse(context.GetText(), CultureInfo.InvariantCulture));
            case SimplifiedParser.DecimalContext _: return new NumberValue(double.Parse(context.GetText(), CultureInfo.InvariantCulture));
            case SimplifiedParser.IntegerContext _: return new IntegerValue(long.Parse(context.GetText(), CultureInfo.InvariantCulture));
            case SimplifiedParser.PhraseContext _: return new PhraseValue(context.GetText());
            case SimplifiedParser.TermContext _: return new StringValue(context.GetText());
        }
        throw new ArgumentOutOfRangeException(nameof(context));
    }

    public override BaseQuery VisitOffsetValue(SimplifiedParser.OffsetValueContext context)
    {
        switch (context)
        {
            case SimplifiedParser.ComplexDateOffsetContext _: return DateTimeOffsetValue.Parse(now, context.GetText());
            case SimplifiedParser.SimpleDateOffsetContext _: return DateTimeOffsetValue.Parse(now, context.GetText());
        }
        throw new ArgumentOutOfRangeException(nameof(context));
    }

    public override BaseQuery VisitAnyClause(SimplifiedParser.AnyClauseContext context)
    {
        return new MatchAnyQuery();
    }

    public override BaseQuery VisitRangeClause(SimplifiedParser.RangeClauseContext context)
    {
        string field = context.TERM().GetText();
        Value from = ParseValue(context.from);
        Value to = ParseValue(context.to);

        return new RangeQuery(field, from, to);
        Value ParseValue(SimplifiedParser.RangeValueContext fieldValue)
        {
            return Visit(fieldValue.children).Cast<Value>().SingleOrDefault();
        }
    }

    public override BaseQuery VisitInClause(SimplifiedParser.InClauseContext context)
    {
        string name = context.TERM().GetText();
        Value[] values = context.children.OfType<SimplifiedParser.PureValueContext>()
            .Select(ctx => ctx.Accept(this)).Cast<Value>().ToArray();
        return new FieldQuery(name, context.NOT() == null ? FieldOperator.In : FieldOperator.NotIn, new ListValue(values));
    }

    public override BaseQuery VisitOrderingClause(SimplifiedParser.OrderingClauseContext context)
    {
        OrderField[] orders = context.children
            .Select(Visit)
            .OfType<OrderField>()
            .ToArray();

        return new OrderBy(orders);
    }

    public override BaseQuery VisitOrderingField(SimplifiedParser.OrderingFieldContext context)
    {
        string field = context.fieldName.Text;
        FieldOrder order = ExtractFieldOrder(context.direction);
        return new OrderField(field, order);

        FieldOrder ExtractFieldOrder(SimplifiedParser.OrderingDirectionContext direction)
        {
            if (direction == null)
                return FieldOrder.None;
            return direction.DESC() != null ? FieldOrder.Descending : FieldOrder.Ascending;
        }
    }

    public override BaseQuery VisitField(SimplifiedParser.FieldContext context)
    {
        string name = context.TERM().GetText();
        switch (context.@operator())
        {
            case SimplifiedParser.EqualsContext _: return new FieldQuery(name, FieldOperator.Equals, ParseValue(context.orFieldValue()));
            case SimplifiedParser.GreaterThanContext _: return new FieldQuery(name, FieldOperator.GreaterThan, ParseValue(context.orFieldValue()));
            case SimplifiedParser.GreaterThanOrEqualsContext _: return new FieldQuery(name, FieldOperator.GreaterThanOrEquals, ParseValue(context.orFieldValue()));
            case SimplifiedParser.LessThanContext _: return new FieldQuery(name, FieldOperator.LessThan, ParseValue(context.orFieldValue()));
            case SimplifiedParser.LessThanOrEqualsContext _: return new FieldQuery(name, FieldOperator.LessThanOrEquals, ParseValue(context.orFieldValue()));
            case SimplifiedParser.NotEqualsContext _: return new FieldQuery(name, FieldOperator.NotEquals, ParseValue(context.orFieldValue()));
            case SimplifiedParser.SimilarContext _: return new FieldQuery(name, FieldOperator.Similar, ParseValue(context.orFieldValue()));
            case SimplifiedParser.NotSimilarContext _: return new FieldQuery(name, FieldOperator.NotSimilar, ParseValue(context.orFieldValue()));
        }

        throw new Exception("Invalid operator for field context: " + context.@operator());

        Value ParseValue(SimplifiedParser.OrFieldValueContext fieldValue)
        {
            return Visit(fieldValue.children).Cast<Value>().Single();
        }
    }

    public override BaseQuery VisitWildcard(SimplifiedParser.WildcardContext context)
    {
        return new WildcardValue(context.GetText());
    }

    public override BaseQuery VisitMatchAll(SimplifiedParser.MatchAllContext context)
    {
        return new MatchAllValue();
    }
    public override BaseQuery VisitTerm(SimplifiedParser.TermContext context)
    {
        return new StringValue(context.GetText());
    }

    public override BaseQuery VisitDate(SimplifiedParser.DateContext context)
    {
        return new DateValue(DateTime.ParseExact(context.GetText(), "yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    public override BaseQuery VisitDateTime(SimplifiedParser.DateTimeContext context)
    {
        return new DateTimeValue(ParseDate(context.GetText()));
    }
    private static DateTime ParseDate(string input)
    {
        return DateTime.ParseExact(input, new[] {
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-ddTHH:mmK",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssK",
        }, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }

    public override BaseQuery VisitInteger(SimplifiedParser.IntegerContext context)
    {
        return new IntegerValue(long.Parse(context.GetText(), CultureInfo.InvariantCulture));
    }

    public override BaseQuery VisitDecimal(SimplifiedParser.DecimalContext context)
    {
        return new NumberValue(double.Parse(context.GetText(), CultureInfo.InvariantCulture));
    }

    public override BaseQuery VisitPhrase(SimplifiedParser.PhraseContext context)
    {
        string phrase = context.GetText();
        return new PhraseValue(phrase.Substring(1, phrase.Length-2));
    }

    public override BaseQuery VisitSimpleDateOffset(SimplifiedParser.SimpleDateOffsetContext context)
    {
        return DateTimeOffsetValue.Parse(now, context.GetText());
    }

    public override BaseQuery VisitComplexDateOffset(SimplifiedParser.ComplexDateOffsetContext context)
    {
        return DateTimeOffsetValue.Parse(now, context.GetText());
    }

    private List<BaseQuery> Visit(IList<IParseTree> items) => items
        .Select(Visit)
        .Where(ast => ast != null)
        .ToList();
    
    protected override BaseQuery AggregateResult(BaseQuery aggregate, BaseQuery nextResult)
    {
        // TODO: what should we actually do here!?
        if (aggregate != null && nextResult != null)
        {
            throw new NotSupportedException("Tried to run aggregate");
        }
        return aggregate ?? nextResult;
    }
}
