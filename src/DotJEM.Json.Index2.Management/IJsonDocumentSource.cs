using System;
using System.Threading.Tasks;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management;

public interface IJsonDocumentSource
{
    IInfoStream InfoStream { get; }

    IObservable<IJsonDocumentChange> DocumentChanges { get; }
    IObservableValue<bool> Initialized { get; }
    
    Task RunAsync();
    void UpdateGeneration(string area, long generation);
    Task ResetAsync();
}