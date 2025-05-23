﻿using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Documents.Strategies;
using DotJEM.Json.Index2.Serialization;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Documents.Builder
{
    public class LuceneDocumentBuilder : AbstractLuceneDocumentBuilder
    {
        private readonly IFieldStrategyCollection strategies;

        public LuceneDocumentBuilder(
            IFieldResolver resolver = null, 
            IFieldStrategyCollection strategies = null,
            IJsonDocumentSerializer documentSerializer = null) 
            : base(resolver, documentSerializer)
        {
            this.strategies = strategies ?? new NullFieldStrategyCollection();
        }

        protected IFieldStrategy ResolveStrategy(IPathContext context, JTokenType type) => strategies.Resolve(context.Path, type);

        protected override void Visit(JArray json, IPathContext context)
        {
            IFieldStrategy strategy = ResolveStrategy(context, JTokenType.Array)
                                      ?? new ArrayFieldStrategy();
            Add(strategy.CreateFields(json, context));
            base.Visit(json, context);
        }

        protected override void VisitInteger(JValue json, IPathContext context)
        {
            IFieldStrategy strategy = ResolveStrategy(context, JTokenType.Integer)
                                      ?? new Int64FieldStrategy();
            Add(strategy.CreateFields(json, context));
        }

        protected override void VisitFloat(JValue json, IPathContext context)
        {
            IFieldStrategy strategy = ResolveStrategy(context, JTokenType.Float)
                                      ?? new DoubleFieldStrategy();
            Add(strategy.CreateFields(json, context));
        }

        protected override void VisitString(JValue json, IPathContext context)
        {
            //TODO: Certain fields should probably work as Identity. So there is cases where this is not good enough.
            IFieldStrategy strategy = ResolveStrategy(context, JTokenType.String)
                                      ?? new TextFieldStrategy();
            Add(strategy.CreateFields(json, context));
        }

        protected override void VisitBoolean(JValue json, IPathContext context)
        {
            IFieldStrategy strategy = ResolveStrategy(context, JTokenType.Boolean)
                                      ?? new BooleanFieldStrategy();
            Add(strategy.CreateFields(json, context));
        }

        protected override void VisitNull(JValue json, IPathContext context)
        {
            IFieldStrategy strategy = ResolveStrategy(context, JTokenType.Null)
                                      ?? new NullFieldStrategy("$$NULL$$");
            Add(strategy.CreateFields(json, context));
        }

        protected override void VisitUndefined(JValue json, IPathContext context)
        {
            IFieldStrategy strategy = ResolveStrategy(context, JTokenType.Undefined)
                                      ?? new NullFieldStrategy("$$UNDEFINED$$");
            Add(strategy.CreateFields(json, context));
        }

        protected override void VisitDate(JValue json, IPathContext context)
        {
            IFieldStrategy strategy = ResolveStrategy(context, JTokenType.Date)
                                      ?? new ExpandedDateTimeFieldStrategy();
            Add(strategy.CreateFields(json, context));
        }

        protected override void VisitGuid(JValue json, IPathContext context)
        {
            IFieldStrategy strategy = ResolveStrategy(context, JTokenType.Guid)
                                      ?? new IdentityFieldStrategy();
            Add(strategy.CreateFields(json, context));
        }

        protected override void VisitUri(JValue json, IPathContext context)
        {
            IFieldStrategy strategy = ResolveStrategy(context, JTokenType.Uri)
                                      ?? new TextFieldStrategy();
            Add(strategy.CreateFields(json, context));
        }

        protected override void VisitTimeSpan(JValue json, IPathContext context)
        {
            IFieldStrategy strategy = ResolveStrategy(context, JTokenType.TimeSpan)
                                      ?? new ExpandedTimeSpanFieldStrategy();
            Add(strategy.CreateFields(json, context));
        }
    }
}
