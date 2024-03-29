﻿using System;
using System.Text.RegularExpressions;
using DotJEM.AdvParsers;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class OffsetDateTime : Value
{
    public static Regex pattern = new Regex("^(?'r'NOW|TODAY)?(?'s'[+-])(?'v'.*)", RegexOptions.Compiled);

    public string Raw { get; }
    public DateTime Now { get; }
    public TimeSpan Offset { get; }
    public DateTime Value { get; }

    private OffsetDateTime(string raw, TimeSpan offset, DateTime now)
    {
        Raw = raw;
        Offset = offset;
        Now = now;

        Value = now.Add(offset);
    }

    public static OffsetDateTime Parse(DateTime now, string text)
    {
        TimeSpanParser parser = new TimeSpanParser();
        Match match = pattern.Match(text.Trim());

        if (!match.Success)
            throw new ArgumentException($"Could not parse OffsetDateTime: {text}");

        string r = match.Groups["r"]?.Value;
        string s = match.Groups["s"]?.Value;
        string v = match.Groups["v"]?.Value;

        TimeSpan offset = parser.Parse(v);
        offset = s == "+" ? offset : offset.Negate();
        now = r?.ToLower() == "now" ? now : now.Date;
                
        return new OffsetDateTime(text, offset, now);
    }
    public override string ToString() => Value.ToString();
}