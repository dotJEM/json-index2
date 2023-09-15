namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class IntegerValue : Value
{
    public long Value { get; }

    public IntegerValue(long value)
    {
        Value = value;
    }
    public override string ToString() => Value.ToString();
}