namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class PhraseValue : StringValue
{
    public PhraseValue(string value) 
        : base(value)
    {
    }

    public override string ToString()
    {
        return $"\"{Value}\"";
    }
}