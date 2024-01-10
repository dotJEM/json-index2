using System.Collections.Generic;
using DotJEM.ObservableExtensions;

namespace DotJEM.Json.Index2.Management;

public class ObservableValue<T> : BasicSubject<T>, IObservableValue<T>
{
    private T value;

    public T Value
    {
        get => value;
        set
        {
            if(EqualityComparer<T>.Default.Equals(this.value, value))
                return;

            Publish(this.value = value);
        }
    }
}