using DotJEM.Json.Visitor;

namespace DotJEM.Json.Index2.Documents.Builder
{

    public interface IPathContext : IJsonVisitorContext<IPathContext>
    {
        string Path { get; }
    }
}