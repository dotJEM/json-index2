using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Management.Source;

public interface IJsonDocumentSourceEvent
{
    string Area { get; }
}

public interface IJsonDocumentChangeEvent : IJsonDocumentSourceEvent
{
    int Size { get; }
    JObject Document { get; }
    GenerationInfo Generation { get; }
}

public readonly record struct JsonDocumentCreated(string Area, JObject Document, int Size, GenerationInfo Generation) : IJsonDocumentChangeEvent;

public readonly record struct JsonDocumentUpdated(string Area, JObject Document, int Size, GenerationInfo Generation): IJsonDocumentChangeEvent;

public readonly record struct JsonDocumentDeleted(string Area, JObject Document, int Size, GenerationInfo Generation): IJsonDocumentChangeEvent;

public readonly record struct JsonDocumentSourceDigestCompleted(string Area) : IJsonDocumentSourceEvent;
public readonly record struct JsonDocumentSourceReset(string Area) : IJsonDocumentSourceEvent;

