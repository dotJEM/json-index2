﻿using System;
using System.Collections.Generic;
using System.Linq;
using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Documents.Builder;
using DotJEM.Json.Index2.Documents.Data;
using DotJEM.Json.Index2.Documents.Info;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Documents
{
    public interface ILuceneDocumentFactory
    {
        IEnumerable<LuceneDocumentEntry> Create(JObject entity);
        IEnumerable<LuceneDocumentEntry> Create(IEnumerable<JObject> entities);
    }

    public class LuceneDocumentFactory : ILuceneDocumentFactory
    {
        private readonly IFieldInformationManager fieldsInfo;
        private readonly IFactory<ILuceneDocumentBuilder> builderFactory;

        public LuceneDocumentFactory(IFieldInformationManager fieldsInformationManager)
            : this(fieldsInformationManager, new FuncFactory<ILuceneDocumentBuilder>(() => new LuceneDocumentBuilder()))
        {
        }

        public LuceneDocumentFactory(IFieldInformationManager fieldsInformationManager, IFactory<ILuceneDocumentBuilder> builderFactory)
        {
            this.fieldsInfo = fieldsInformationManager ?? throw new ArgumentNullException(nameof(fieldsInformationManager));
            this.builderFactory = builderFactory ?? throw new ArgumentNullException(nameof(builderFactory));
        }

        public IEnumerable<LuceneDocumentEntry> Create(JObject entity)
        {
            //TODO: (jmd 2020-08-10) Make Async implementation later on.
            ILuceneDocumentBuilder builder = builderFactory.Create();
            string contentType = fieldsInfo.Resolver.ContentType(entity);

            IIndexableJsonDocument doc = builder.Build(entity);
            fieldsInfo.Merge(contentType, doc.Info);

            return [new LuceneDocumentEntry(fieldsInfo.Resolver.Identity(entity), contentType, doc.Document)];
        }

        public IEnumerable<LuceneDocumentEntry> Create(IEnumerable<JObject> entities)
        {
            //TODO: (jmd 2020-08-10) Make Async implementation later on.
            return entities.SelectMany(Create);
        }


    }
}
