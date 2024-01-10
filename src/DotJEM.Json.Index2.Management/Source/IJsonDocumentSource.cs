using System;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Management.Observables;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management.Source;

public interface IJsonDocumentSource
{
    IInfoStream InfoStream { get; }

    IObservable<IJsonDocumentChange> DocumentChanges { get; }
    IObservableValue<bool> Initialized { get; }

    Task RunAsync();
    void UpdateGeneration(string area, long generation);
    Task ResetAsync();
}