namespace DotJEM.Json.Index2.QueryParsers.Simplified.Ast.Scanner.Matchers
{
    public class NullMatcher : IValueMatcher
    {
        public bool Matches(string value) => false;
    }
}