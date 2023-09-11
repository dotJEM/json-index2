using System;
using System.Collections.Generic;
using System.Linq;

namespace DotJEM.Json.Index2.Configuration;

public class ServiceCollection : IServiceCollection
{
    private readonly Dictionary<Type, Lazy<object>> factories;

    public ServiceCollection(IJsonIndexConfiguration configuration, IEnumerable<ServiceDescriptor> services)
    {
        this.factories = services
            .ToDictionary(descriptor => descriptor.Type, descriptor => new Lazy<object>(()=>descriptor.Factory(configuration)));
    }

    public bool TryGet<TService>(out TService value)
    {
        if (factories.TryGetValue(typeof(TService), out Lazy<object> val))
        {
            value= (TService)val.Value;
            return true;
        }

        value = default;
        return false;
    }

    public TService Get<TService>()
    {
        return TryGet(out TService service)
            ? service 
            : default;
    }
}