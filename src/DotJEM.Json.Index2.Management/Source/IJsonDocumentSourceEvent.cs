using Newtonsoft.Json.Linq;
using System;

namespace DotJEM.Json.Index2.Management.Source;

public interface IJsonDocumentSourceEvent
{
    string Area { get; }
}

//public interface IJsonDocumentSourceChange
//{
//    string Area { get; }
//}

public readonly record struct JsonDocumentCreated(string Area, JObject Document, int Size, GenerationInfo Generation) : IJsonDocumentSourceEvent;

public readonly record struct JsonDocumentUpdated(string Area, JObject Document, int Size, GenerationInfo Generation): IJsonDocumentSourceEvent;

public readonly record struct JsonDocumentDeleted(string Area, JObject Document, int Size, GenerationInfo Generation): IJsonDocumentSourceEvent;

public readonly record struct JsonDocumentSourceDigestCompleted(string Area) : IJsonDocumentSourceEvent;
public readonly record struct JsonDocumentSourceReset(string Area) : IJsonDocumentSourceEvent;

