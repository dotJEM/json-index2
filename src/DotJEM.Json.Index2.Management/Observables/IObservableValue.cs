using System;

namespace DotJEM.Json.Index2.Management.Observables;

public interface IObservableValue<T> : IObservable<T>
{
    T Value { get; set; }
}