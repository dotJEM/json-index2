using System;

namespace DotJEM.Json.Index2.Management;

public interface IObservableValue<T> : IObservable<T>
{
    T Value { get; set; }
}