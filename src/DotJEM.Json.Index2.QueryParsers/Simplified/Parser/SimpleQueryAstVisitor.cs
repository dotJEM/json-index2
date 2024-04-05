using System.Linq;
using DotJEM.Json.Index2.QueryParsers.Ast;

namespace DotJEM.Json.Index2.QueryParsers.Simplified.Parser
{
    public abstract class SimplifiedQueryAstVisitor<TResult, TContext>: ISimplifiedQueryAstVisitor<TResult, TContext> where TResult : class 
    {
        public virtual TResult Visit(BaseQuery ast, TContext context) => default (TResult);
        public virtual TResult Visit(NotQuery ast, TContext context) => Visit((BaseQuery)ast, context);

        public virtual TResult Visit(OrderedQuery ast, TContext context) => Visit((BaseQuery)ast, context);
        public virtual TResult Visit(OrderBy ast, TContext context) => Visit((BaseQuery)ast, context);
        public virtual TResult Visit(OrderField ast, TContext context) => Visit((BaseQuery)ast, context);
        public virtual TResult Visit(FieldQuery ast, TContext context) => Visit((BaseQuery)ast, context);

        public TResult Visit(MatchAnyQuery ast, TContext context) => Visit((BaseQuery)ast, context);

        public TResult Visit(Value ast, TContext context) => Visit((BaseQuery)ast, context);

        public TResult Visit(StringValue ast, TContext context) => Visit((Value)ast, context);

        public TResult Visit(WildcardValue ast, TContext context) => Visit((Value)ast, context);

        public TResult Visit(DateTimeValue ast, TContext context) => Visit((Value)ast, context);

        public TResult Visit(IntegerValue ast, TContext context) => Visit((Value)ast, context);

        public TResult Visit(ListValue ast, TContext context) => Visit((Value)ast, context);

        public TResult Visit(MatchAllValue ast, TContext context) => Visit((Value)ast, context);
        public TResult Visit(NumberValue ast, TContext context) => Visit((Value)ast, context);

        public TResult Visit(DateTimeOffsetValue ast, TContext context) => Visit((Value)ast, context);

        public TResult Visit(PhraseValue ast, TContext context) => Visit((Value)ast, context);

        public virtual TResult Visit(CompositeQuery ast, TContext context) => ast.Queries.Select(q => q.Accept(this, context)).Aggregate(AggregateQuery);

        protected virtual TResult AggregateQuery(TResult result, TResult next) => next ?? result;

        public virtual TResult Visit(OrQuery ast, TContext context) => Visit((CompositeQuery)ast, context);
        public virtual TResult Visit(AndQuery ast, TContext context) => Visit((CompositeQuery)ast, context);
        public virtual TResult Visit(ImplicitCompositeQuery ast, TContext context) => Visit((CompositeQuery)ast, context);


    }
}