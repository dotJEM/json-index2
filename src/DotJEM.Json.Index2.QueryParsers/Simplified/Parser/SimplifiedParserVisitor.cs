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

    public override BaseQuery VisitAndClause(SimplifiedParser.AndClauseContext context)
    {
        List<BaseQuery> fragments = Visit(context.children);
        // Note: If we only have a single fragment, there was no OR, just return the fragment.
        return fragments.Count < 2
            ? fragments.SingleOrDefault()
            : new AndQuery(fragments.ToArray());
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

    public override BaseQuery VisitMatchAll(SimplifiedParser.MatchAllContext context)
    {
        return new MatchAnyQuery();
    }




    public override BaseQuery VisitWildcardValue(SimplifiedParser.WildcardValueContext context)
    {
        //wildcardValue : WILDCARD_TERM       #Wildcard
        return new WildcardValue(context.GetText());
    }

    public override BaseQuery VisitStarValue(SimplifiedParser.StarValueContext context)
    {
        //starValue : STAR                #MatchAll
        return new MatchAllValue();
    }

    public override BaseQuery VisitPureValue(SimplifiedParser.PureValueContext context)
    {
        //pureValue : TERM                #Term
        //          | DATE                #Date
        //          | DATE_TIME           #DateTime
        //          | INTEGER             #Integer
        //          | DECIMAL             #Decimal
        //          | PHRASE              #Phrase
        //          ;
        throw new NotImplementedException();
    }

    public override BaseQuery VisitOffsetValue(SimplifiedParser.OffsetValueContext context)
    {
        //offsetValue : SIMPLE_DATE_OFFSET  #SimpleDateOffset
        //            | COMPLEX_DATE_OFFSET #ComplexDateOffset
        //            ;
        throw new NotImplementedException();
    }


    public override BaseQuery VisitBasicClause(SimplifiedParser.BasicClauseContext context)
    {
        return base.VisitBasicClause(context);
    }


    public override BaseQuery VisitAtom(SimplifiedParser.AtomContext context)
    {
        throw new NotImplementedException();
    }

    public override BaseQuery VisitAnyClause(SimplifiedParser.AnyClauseContext context)
    {
        throw new NotImplementedException();
    }

    public override BaseQuery VisitRangeClause(SimplifiedParser.RangeClauseContext context)
    {
        throw new NotImplementedException();
    }

    public override BaseQuery VisitOrderingClause(SimplifiedParser.OrderingClauseContext context)
    {
        throw new NotImplementedException();
    }

    public override BaseQuery VisitOrderingField(SimplifiedParser.OrderingFieldContext context)
    {
        throw new NotImplementedException();
    }

    public override BaseQuery VisitOrderingDirection(SimplifiedParser.OrderingDirectionContext context)
    {
        throw new NotImplementedException();
    }

    public override BaseQuery VisitField(SimplifiedParser.FieldContext context)
    {
        throw new NotImplementedException();
    }

    public override BaseQuery VisitWildcard(SimplifiedParser.WildcardContext context)
    {
        return base.VisitWildcard(context);
    }

    public override BaseQuery VisitTerm(SimplifiedParser.TermContext context)
    {
        return base.VisitTerm(context);
    }

    public override BaseQuery VisitDate(SimplifiedParser.DateContext context)
    {
        return base.VisitDate(context);
    }

    public override BaseQuery VisitDateTime(SimplifiedParser.DateTimeContext context)
    {
        return base.VisitDateTime(context);
    }

    public override BaseQuery VisitInteger(SimplifiedParser.IntegerContext context)
    {
        return base.VisitInteger(context);
    }

    public override BaseQuery VisitDecimal(SimplifiedParser.DecimalContext context)
    {
        return base.VisitDecimal(context);
    }

    public override BaseQuery VisitPhrase(SimplifiedParser.PhraseContext context)
    {
        return base.VisitPhrase(context);
    }

    public override BaseQuery VisitSimpleDateOffset(SimplifiedParser.SimpleDateOffsetContext context)
    {
        return base.VisitSimpleDateOffset(context);
    }

    public override BaseQuery VisitComplexDateOffset(SimplifiedParser.ComplexDateOffsetContext context)
    {
        return base.VisitComplexDateOffset(context);
    }

    public override BaseQuery VisitEquals(SimplifiedParser.EqualsContext context)
    {
        return base.VisitEquals(context);
    }

    public override BaseQuery VisitNotEquals(SimplifiedParser.NotEqualsContext context)
    {
        return base.VisitNotEquals(context);
    }

    public override BaseQuery VisitGreaterThan(SimplifiedParser.GreaterThanContext context)
    {
        return base.VisitGreaterThan(context);
    }

    public override BaseQuery VisitGreaterThanOrEquals(SimplifiedParser.GreaterThanOrEqualsContext context)
    {
        return base.VisitGreaterThanOrEquals(context);
    }

    public override BaseQuery VisitLessThan(SimplifiedParser.LessThanContext context)
    {
        return base.VisitLessThan(context);
    }

    public override BaseQuery VisitLessThanOrEquals(SimplifiedParser.LessThanOrEqualsContext context)
    {
        return base.VisitLessThanOrEquals(context);
    }

    public override BaseQuery VisitSimilar(SimplifiedParser.SimilarContext context)
    {
        return base.VisitSimilar(context);
    }

    public override BaseQuery VisitNotSimilar(SimplifiedParser.NotSimilarContext context)
    {
        return base.VisitNotSimilar(context);
    }

    public override BaseQuery VisitRangeValue(SimplifiedParser.RangeValueContext context)
    {
        return base.VisitRangeValue(context);
    }

    public override BaseQuery VisitInClause(SimplifiedParser.InClauseContext context)
    {
        return base.VisitInClause(context);
    }

    public override BaseQuery VisitNotInClause(SimplifiedParser.NotInClauseContext context)
    {
        return base.VisitNotInClause(context);
    }

    public override BaseQuery VisitName(SimplifiedParser.NameContext context)
    {
        return base.VisitName(context);
    }


    public override BaseQuery VisitAndOperator(SimplifiedParser.AndOperatorContext context)
    {
        return base.VisitAndOperator(context);
    }

    public override BaseQuery VisitOrOperator(SimplifiedParser.OrOperatorContext context)
    {
        return base.VisitOrOperator(context);
    }

    public override BaseQuery VisitNotOperator(SimplifiedParser.NotOperatorContext context)
    {
        return base.VisitNotOperator(context);
    }

    public override BaseQuery VisitOperator(SimplifiedParser.OperatorContext context)
    {
        return base.VisitOperator(context);
    }

    public override BaseQuery VisitTerminal(ITerminalNode node)
    {
        return base.VisitTerminal(node);
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
