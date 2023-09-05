﻿using System;
using System.Collections.Generic;
using DotJEM.Json.Index2.Documents;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.IO;
using DotJEM.Json.Index2.Serialization;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;

namespace DotJEM.Json.Index2.Configuration
{
    //TODO: Split into IServiceCollection/Factory and IServiceCollectionBuilder
    public interface IServiceCollection
    {
        IServiceCollection Use<TService, TImpl>() where TImpl : TService where TService : class;
        IServiceCollection Use(Type service, Type implementation);
        IServiceCollection Use<TService>(Func<TService> factory);
        IServiceCollection Use<TService>(Func<IServiceResolver, TService> factory);
        IServiceCollection Use(Type service, Func<IServiceResolver, object> factory);

        bool Contains<T>();
        bool Contains(Type type);

        Func<IServiceResolver, T> ObtainFactory<T>();
        Func<IServiceResolver, object> ObtainFactory(Type key);

        bool TryObtainFactory<T>(out Func<IServiceResolver, T> value);
        bool TryObtainFactory(Type key, out Func<IServiceResolver, object> value);
    }

    public class ServiceCollection : IServiceCollection
    {
        private readonly IServiceFactory factory;
        private readonly Dictionary<Type, Func<IServiceResolver, object>> factories = new Dictionary<Type, Func<IServiceResolver, object>>();

        public static IServiceCollection CreateDefault()
        {
            return new ServiceCollection()
                .Use<Analyzer>(() => new StandardAnalyzer(LuceneVersion.LUCENE_48, CharArraySet.EMPTY_SET))
                .Use<IFieldResolver, FieldResolver>()
                .Use<IFieldInformationManager, DefaultFieldInformationManager>()
                .Use<ILuceneDocumentFactory, LuceneDocumentFactory>()
                .Use<IJsonDocumentSerializer, GZipJsonDocumentSerialier>()
                .Use<IJsonIndexWriterProvider, SyncJsonIndexWriterProvider>()
                .Use<IInflowCapacity, NullInflowCapacity>();
        }

        public ServiceCollection(IServiceFactory factory = null)
        {
            this.factory = factory ?? new DefaultServiceFactory();
        }

        public bool TryObtainFactory<T>(out Func<IServiceResolver, T> value)
        {
            if (TryObtainFactory(typeof(T), out Func<IServiceResolver, object> fac))
            {
                value = sp => (T)fac(sp);
                return true;
            }
            value = null;
            return false;
        }

        public virtual bool TryObtainFactory(Type key, out Func<IServiceResolver, object> value) => factories.TryGetValue(key, out value);
        public Func<IServiceResolver, T> ObtainFactory<T>() => TryObtainFactory<T>(out var fac) ? fac : null;
        public virtual Func<IServiceResolver, object> ObtainFactory(Type key) => TryObtainFactory(key, out var fac) ? fac : null;

        public virtual IServiceCollection Use<TService, TImpl>() where TImpl : TService where TService : class => Use(typeof(TService), typeof(TImpl));
        public virtual IServiceCollection Use(Type service, Type implementation) => Use(service, provider => factory.Create(provider, implementation));
        public virtual IServiceCollection Use<TService>(Func<TService> factoryMethod) => Use(typeof(TService), sp => factoryMethod());
        public virtual IServiceCollection Use<TService>(Func<IServiceResolver, TService> factoryMethod) => Use(typeof(TService), sp => factoryMethod(sp));

        public virtual IServiceCollection Use(Type service, Func<IServiceResolver, object> factoryMethod)
        {
            factories[service] = factoryMethod;
            return this;
        }

        public virtual bool Contains<TService>() => Contains(typeof(TService));

        public virtual bool Contains(Type type) => factories.ContainsKey(type);
    }
    

    public interface IServices
    {
        IFieldResolver FieldResolver { get; }
        IFieldInformationManager FieldInformationManager { get; }
        ILuceneDocumentFactory DocumentFactory { get; }
        IJsonDocumentSerializer Serializer { get; }
    }

    public class DefaultServices : IServices
    {
        private Lazy<Analyzer> analyzer;
        private Lazy<IFieldResolver> fieldResolver;
        private Lazy<IFieldInformationManager> fieldInformationManager;
        private Lazy<ILuceneDocumentFactory> documentFactory;
        private Lazy<IJsonDocumentSerializer> serializer;

        public Analyzer Analyzer => analyzer.Value;
        public IFieldResolver FieldResolver => fieldResolver.Value;
        public IFieldInformationManager FieldInformationManager => fieldInformationManager.Value;
        public ILuceneDocumentFactory DocumentFactory => documentFactory.Value;
        public IJsonDocumentSerializer Serializer => serializer.Value;

        public DefaultServices()
        {
            this.analyzer = new Lazy<Analyzer>(() => new StandardAnalyzer(LuceneVersion.LUCENE_48, CharArraySet.EMPTY_SET));
            this.serializer = new Lazy<IJsonDocumentSerializer>(() => new GZipJsonDocumentSerialier());
            this.documentFactory = new Lazy<ILuceneDocumentFactory>(() => new LuceneDocumentFactory(FieldInformationManager));
            this.fieldInformationManager = new Lazy<IFieldInformationManager>(() => new DefaultFieldInformationManager(FieldResolver));
            this.fieldResolver = new Lazy<IFieldResolver>(() => new FieldResolver());
        }
    }

}