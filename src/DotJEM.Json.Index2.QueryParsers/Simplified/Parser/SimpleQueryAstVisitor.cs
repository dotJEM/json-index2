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

        public virtual TResult Visit(CompositeQuery ast, TContext context) => ast.Queries.Select(q => q.Accept(this, context)).Aggregate(AggregateQuery);

        protected virtual TResult AggregateQuery(TResult result, TResult next) => next ?? result;

        public virtual TResult Visit(OrQuery ast, TContext context) => Visit((CompositeQuery)ast, context);
        public virtual TResult Visit(AndQuery ast, TContext context) => Visit((CompositeQuery)ast, context);
        public virtual TResult Visit(ImplicitCompositeQuery ast, TContext context) => Visit((CompositeQuery)ast, context);


    }
}