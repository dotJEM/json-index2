using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.QueryParsers.Ast;

namespace DotJEM.Json.Index2.QueryParsers.Simplified.Ast.Scanner
{
    public static class SimplifiedQueryOptimizationExtensions
    {
        public static BaseQuery DecorateWithContentTypes(this BaseQuery ast, IFieldInformationManager fields)
            => ast.DecorateWithContentTypes(new ContentTypesDecorator(fields));

        public static BaseQuery DecorateWithContentTypes(this BaseQuery ast, IContentTypesDecorator decorator)
        {
            ast.Accept(decorator, null);
            return ast;
        }
    }
}

