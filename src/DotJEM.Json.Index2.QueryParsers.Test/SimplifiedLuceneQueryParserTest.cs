using DotJEM.Json.Index2.Analysis;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Documents.Info;
using Lucene.Net.Util;
using NUnit.Framework;

namespace DotJEM.Json.Index2.QueryParsers.Test;

public class SimplifiedLuceneQueryParserTest
{
    [TestCase("*:*", "*:*")]
    [TestCase("simple:value", "+simple:value")]
    [TestCase("name:(Peter OR Anna)", "")]
    //[TestCase("age: [5 TO 9]", "")]
    //[TestCase("age: [5 TO *]", "")]
    //[TestCase("age: [* TO 9]", "")]
    //[TestCase("age: [2345 TO 2110]", "")]
    //[TestCase("height: [1.6 TO 2.0]", "")]
    //[TestCase("height: [* TO 2.0]", "")]
    //[TestCase("height: [1.6 TO *]", "")]
    //[TestCase("born: [2020-03-02 TO 2020-03-25]", "")]
    //[TestCase("born: [2020-03-02T21:21 TO 2020-03-25T21:23]", "")]
    //[TestCase("born: [2020-03-02T21:21:00 TO 2020-03-25T21:23:00]", "")]
    //[TestCase("born: [2020-03-02T21:24:00+01:00 TO 2020-03-25T21:24:00+01:00]", "")]
    //[TestCase("born: [* TO 2020-03-25T21:24:00+01:00]", "")]
    //[TestCase("born: [2020-03-02T21:24:00+01:00 TO *]", "")]
    //[TestCase("born: [-5days TO *]", "")]
    //[TestCase("born: [TODAY+5D TO *]", "")]
    //[TestCase("age: [5 TO 10] ORDER BY foo", "")]
    //[TestCase("  age: [5 TO 10] ORDER BY foo DESC, fax ASC, fox", "")]
    //[TestCase("name: Peter", "")]
    //[TestCase("name: Peter AND age: [10 TO 100]", "")]
    //[TestCase("name: Peter OR name: Anna NOT name: Claus OR name: Hans", "")]
    //[TestCase("name: Peter OR name: Anna NOT name: \"Claus Parsø\" OR name: Hans", "")]
    //[TestCase("name: Peter OR name: Anna NOT name: Claus* OR name: Hans", "")]
    //[TestCase("(name: Peter OR name: Anna) AND (age: 5 OR age: [8 TO 10])", "")]
    //[TestCase("ship.name: FooBarasd AND $version: [6 TO *] AND $created: [2020-03-02T21:24:00+01:00 TO 2020-03-25] AND contentType: notification AND id: 123", "")]
    //[TestCase("$created: [2020-03-02T21:24:00+01:00 TO 2020-03-25T21:24:00+01:00]", "")]
    //[TestCase("$created: [2020-03-02T21:24:00+01:00 TO *]", "")]
    //[TestCase("$created: [22 TO *]", "")]
    //[TestCase("ship.name: FooBarasd AND $version: [6 TO 10] AND $created: [2020-03-02T21:24:00+01:00 TO 2020-03-25] AND contentType: notification AND id: 123", "")]
    //[TestCase("ship.name: FooBarasd AND $created: [2020-03-02T21:24:00+01:00 TO 2020-03-25] AND contentType: notification AND id: 123", "")]
    //[TestCase("$version: [6.6 TO *]", "")]
    //[TestCase("$version: [6 TO * ]", "")]
    //[TestCase("$version: [ 6 TO * ]", "")]
    //[TestCase("name: Peter Hansen", "")]
    public void Parse(string query, string expected)
    {
        ILuceneQueryParser parser = new SimplifiedLuceneQueryParser(
            new DefaultFieldInformationManager(new FieldResolver()), new JsonAnalyzer(LuceneVersion.LUCENE_48)
        );

        LuceneQueryInfo parsed = parser.Parse(query)!;
        Assert.That(parsed.Query.ToString(), Is.EqualTo(expected));
    }
}




