using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Management.Source;

public record JsonDocumentChange(string Area, JsonChangeType Type, JObject Entity, int Size, GenerationInfo Generation)
    : IJsonDocumentChange
{
    public GenerationInfo Generation { get; } = Generation;
    public JsonChangeType Type { get; } = Type;
    public string Area { get; } = Area;
    public JObject Entity { get; } = Entity;
    public int Size { get; } = Size;
}