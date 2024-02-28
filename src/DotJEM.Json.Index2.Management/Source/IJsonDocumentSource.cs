using System;
using System.Threading.Tasks;
using DotJEM.Json.Index2.Management.Observables;
using DotJEM.ObservableExtensions.InfoStreams;

namespace DotJEM.Json.Index2.Management.Source;

/// <summary>
/// 
/// </summary>
public interface IJsonDocumentSource
{
    /// <summary>
    /// 
    /// </summary>
    IInfoStream InfoStream { get; }

    /// <summary>
    /// 
    /// </summary>
    IObservable<IJsonDocumentSourceEvent> DocumentChanges { get; }
    
    /// <summary>
    /// 
    /// </summary>
    IObservableValue<bool> Initialized { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task StartAsync();
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task StopAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="area"></param>
    /// <param name="generation"></param>
    void UpdateGeneration(string area, long generation);
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task ResetAsync();
}

