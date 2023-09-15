namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class StringValue : Value
{
    public string Value { get; }

    public StringValue(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
}