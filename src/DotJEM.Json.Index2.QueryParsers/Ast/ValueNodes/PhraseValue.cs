namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class PhraseValue : Value
{
    public string Value { get; }

    public PhraseValue(string value)
    {
        Value = value;
    }
    public override string ToString() => Value;
}