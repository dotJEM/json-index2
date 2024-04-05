using System;
using System.Text.RegularExpressions;
using DotJEM.AdvParsers;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class DateTimeOffsetValue : Value
{
    public static Regex pattern = new Regex("^(?'r'NOW|TODAY)?(?'s'[+-])(?'v'.*)", RegexOptions.Compiled);

    public string Raw { get; }
    public DateTime Now { get; }
    public TimeSpan Offset { get; }
    public DateTime Value { get; }

    private DateTimeOffsetValue(string raw, TimeSpan offset, DateTime now)
    {
        Raw = raw;
        Offset = offset;
        Now = now;
        Value = now.Add(offset);
    }

    public static DateTimeOffsetValue Parse(DateTime now, string text)
    {
        Match match = pattern.Match(text.Trim());

        if (!match.Success)
            throw new ArgumentException($"Could not parse OffsetDateTime: {text}");

        string r = match.Groups["r"]?.Value;
        string s = match.Groups["s"]?.Value;
        string v = match.Groups["v"]?.Value;

        TimeSpan offset = AdvParser.ParseTimeSpan(v);
        offset = s == "+" ? offset : offset.Negate();
        now = r?.ToLower() == "now" ? now : now.Date;
                
        return new DateTimeOffsetValue(text, offset, now);
    }
    public override string ToString() => Value.ToString();
}