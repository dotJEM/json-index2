﻿
//public class SimplifiedParserVisitor : SimplifiedBaseVisitor<BaseQuery>
//{
//    private DateTime now;

//    protected override BaseQuery AggregateResult(BaseQuery aggregate, BaseQuery nextResult)
//    {
//        // TODO: what should we actually do here!?
//        if (aggregate != null && nextResult != null)
//        {
//            throw new NotSupportedException("Tried to run aggregate");
//        }
//        return aggregate ?? nextResult;
//    }

//    public override BaseQuery VisitQuery(SimplifiedParser.QueryContext context)
//    {
//        now = DateTime.Now;
//        BaseQuery query = context.clause.Accept(this);
//        BaseQuery order = context.order?.Accept(this);
//        return new OrderedQuery(query, order);
//    }

//    public override BaseQuery VisitOrClause(SimplifiedParser.OrClauseContext context)
//    {
//        List<BaseQuery> fragments = Visit(context.children);
//        // Note: If we only have a single fragment, there was no OR, just return the fragment.
//        if (fragments.Count < 2)
//            return fragments.SingleOrDefault();
//        return new OrQuery(fragments.ToArray());
//    }

//    public override BaseQuery VisitAndClause(SimplifiedParser.AndClauseContext context)
//    {
//        List<BaseQuery> fragments = Visit(context.children);
//        // Note: If we only have a single fragment, there was no AND, just return the fragment.
//        if (fragments.Count < 2)
//            return fragments.SingleOrDefault();
//        return new AndQuery(fragments.ToArray());
//    }

//    public override BaseQuery VisitNotClause(SimplifiedParser.NotClauseContext context)
//    {
//        List<BaseQuery> fragments = Visit(context.children);
//        //Note: There was actually no Not Clause.
//        if (fragments.Count < 2)
//            return fragments.SingleOrDefault();

//        for (int i = 1; i < fragments.Count; i++)
//            fragments[i] = new NotQuery(fragments[i]);

//        return new AndQuery(fragments.ToArray());
//    }

//    public override BaseQuery VisitDefaultClause(SimplifiedParser.DefaultClauseContext context)
//    {
//        List<BaseQuery> fragments = Visit(context.children);
//        // Note: If we only have a single fragment, just return that.
//        if (fragments.Count < 2)
//            return fragments.SingleOrDefault();
//        return new ImplicitCompositeQuery(fragments.ToArray());
//    }

//    public override BaseQuery VisitValue(SimplifiedParser.ValueContext context)
//    {
//        return new FieldQuery(null, FieldOperator.None, Value(context));
//    }

//    public override BaseQuery VisitSimple_value(SimplifiedParser.Simple_valueContext context)
//    {
//        return new FieldQuery(null, FieldOperator.None, Value(context));
//    }

//    public override BaseQuery VisitTerm(SimplifiedParser.TermContext context) => VisitValue(context);
//    public override BaseQuery VisitWildcard(SimplifiedParser.WildcardContext context) => VisitValue(context);
//    public override BaseQuery VisitIntegerNumber(SimplifiedParser.IntegerNumberContext context) => VisitValue(context);
//    public override BaseQuery VisitDecimalNumber(SimplifiedParser.DecimalNumberContext context) => VisitValue(context);
//    public override BaseQuery VisitPhrase(SimplifiedParser.PhraseContext context) => VisitValue(context);
//    public override BaseQuery VisitMatchAll(SimplifiedParser.MatchAllContext context) => VisitValue(context);
//    public override BaseQuery VisitField(SimplifiedParser.FieldContext context)
//    {
//        string name = context.TERM().GetText();
//        switch (context.@operator())
//        {
//            case SimplifiedParser.EqualsContext _:              return new FieldQuery(name, FieldOperator.Equals, Value(context.value()));
//            case SimplifiedParser.GreaterThanContext _:         return new FieldQuery(name, FieldOperator.GreaterThan, Value(context.value()));
//            case SimplifiedParser.GreaterThanOrEqualsContext _: return new FieldQuery(name, FieldOperator.GreaterThanOrEquals, Value(context.value()));
//            case SimplifiedParser.LessThanContext _:            return new FieldQuery(name, FieldOperator.LessThan, Value(context.value()));
//            case SimplifiedParser.LessThanOrEqualsContext _:    return new FieldQuery(name, FieldOperator.LessThanOrEquals, Value(context.value()));
//            case SimplifiedParser.NotEqualsContext _:           return new FieldQuery(name, FieldOperator.NotEquals, Value(context.value()));
//            case SimplifiedParser.SimilarContext _:             return new FieldQuery(name, FieldOperator.Similar, Value(context.value()));
//            case SimplifiedParser.NotSimilarContext _:          return new FieldQuery(name, FieldOperator.NotSimilar, Value(context.value()));
//        }

//        throw new Exception("Invalid operator for field context: " + context.@operator());
//    }

//    public Value Value(SimplifiedParser.ValueContext context)
//    {
//        switch (context)
//        {

//            case SimplifiedParser.DateContext _: return new DateTimeValue(DateTime.ParseExact(context.GetText(), "YYYY-MM-DD", CultureInfo.InvariantCulture), DateTimeValue.Kind.Date);
//            case SimplifiedParser.DateTimeContext _: return new DateTimeValue(DateTime.Parse(context.GetText(), CultureInfo.InvariantCulture), DateTimeValue.Kind.DateTime);
//            case SimplifiedParser.DateOffsetContext _: return DateTimeOffsetValue.Parse(now, context.GetText());
//            case SimplifiedParser.MatchAllContext _:      return new MatchAllValue();
//            case SimplifiedParser.DecimalNumberContext _: return new NumberValue(double.Parse(context.GetText(), CultureInfo.InvariantCulture));
//            case SimplifiedParser.IntegerNumberContext _: return new IntegerValue(long.Parse(context.GetText(), CultureInfo.InvariantCulture));
//            case SimplifiedParser.PhraseContext _:        return new PhraseValue(context.GetText());
//            case SimplifiedParser.TermContext _:          return new StringValue(context.GetText());
//            case SimplifiedParser.WildcardContext _:      return new WildcardValue(context.GetText());
//        }
//        throw new Exception("Unknown value type.");
//    }

//    public override BaseQuery VisitAnyClause(SimplifiedParser.AnyClauseContext context)
//    {
//        return new MatchAnyQuery();
//    }

//    public override BaseQuery VisitRangeClause(SimplifiedParser.RangeClauseContext context)
//    {
//        BaseQuery from = context.from.Accept(this);

//        return base.VisitRangeClause(context);
//    }

//    public override BaseQuery VisitInClause(SimplifiedParser.InClauseContext context)
//    {
//        string name = context.TERM().GetText();
//        Value[] values = context.children.OfType<SimplifiedParser.ValueContext>()
//            .Select(Value).ToArray();
//        return new FieldQuery(name, FieldOperator.In, new ListValue(values));
//    }

//    public override BaseQuery VisitNotInClause(SimplifiedParser.NotInClauseContext context)
//    {
//        string name = context.TERM().GetText();
//        Value[] values = context.children.OfType<SimplifiedParser.ValueContext>()
//            .Select(Value).ToArray();
//        return new FieldQuery(name, FieldOperator.NotIt, new ListValue(values));
//    }

//    public override BaseQuery VisitOrderingClause(SimplifiedParser.OrderingClauseContext context)
//    {
//        OrderField[] orders = context.children
//            .Select(Visit)
//            .OfType<OrderField>()
//            .ToArray();

//        return new OrderBy(orders);
//    }

//    public override BaseQuery VisitOrderingField(SimplifiedParser.OrderingFieldContext context)
//    {
//        string field = context.fieldName.GetText();
//        FieldOrder order = ExtractFieldOrder(context.direction);
//        return new OrderField(field, order);

//        FieldOrder ExtractFieldOrder(SimplifiedParser.OrderingDirectionContext direction)
//        {
//            if (direction == null)
//                return FieldOrder.None;
//            return direction.DESC() != null ? FieldOrder.Descending : FieldOrder.Ascending;
//        }
//    }

//    private List<BaseQuery> Visit(IList<IParseTree> items) => items
//        .Select(Visit)
//        .Where(ast => ast != null)
//        .ToList();

//}
