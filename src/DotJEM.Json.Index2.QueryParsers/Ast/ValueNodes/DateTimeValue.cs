using System;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class DateTimeValue : Value
{
    public DateTime Value { get; }
    public Kind DateTimeKind { get; }

    public DateTimeValue(DateTime value, Kind dateTimeKind)
    {
        Value = value;
        DateTimeKind = dateTimeKind;
    }

    public enum Kind
    {
        Date, Time, DateTime
    }

    public override string ToString() => Value.ToString();
}