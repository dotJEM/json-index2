using System.Globalization;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class NumberValue : Value
{
    public double Value { get; }

    public NumberValue(double value)
    {
        Value = value;
    }
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}