using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Management.Source;

public interface IJsonDocumentChange
{
    string Area { get; }
    GenerationInfo Generation { get; }
    JsonChangeType Type { get; }
    JObject Entity { get; }
    public int Size { get; }
}