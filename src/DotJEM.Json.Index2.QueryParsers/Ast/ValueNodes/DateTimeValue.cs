using System;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class DateTimeValue : Value
{
    public DateTime Value { get; }

    public DateTimeValue(DateTime value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString("s");
}
public class DateValue : DateTimeValue
{
    public DateValue(DateTime value) : base(value)
    {
    }
}
public class TimeValue : DateTimeValue
{
    public TimeValue(DateTime value) : base(value)
    {
    }
}