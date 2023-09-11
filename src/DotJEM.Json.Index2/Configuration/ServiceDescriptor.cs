using System;

namespace DotJEM.Json.Index2.Configuration;

public readonly record struct ServiceDescriptor(Type Type, Func<IJsonIndexConfiguration, object> Factory);