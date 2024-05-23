using DotJEM.Json.Index2.Analysis;
using DotJEM.Json.Index2.Documents.Fields;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.QueryParsers.Ast;
using Lucene.Net.Util;
using NUnit.Framework;

namespace DotJEM.Json.Index2.QueryParsers.Test;

public class SimplifiedQueryAstParserTest
{

    [TestCaseSource(nameof(ParseTestCases))]
    public string Parse(string query)
    {
        SimplifiedQueryAstParser parser = new SimplifiedQueryAstParser();
        BaseQuery parsed = parser.Parse(query)!;
        return parsed.ToString()!;
    }

    public static IEnumerable<object> ParseTestCases
    {
        get
        {
            yield return new TestCaseData("*:*")
                .Returns("*:*");

            yield return new TestCaseData("simple:value")
                .Returns("simple = value");

            yield return new TestCaseData("(Peter OR Anna) ")
                .Returns("( Peter OR Anna )");

            yield return new TestCaseData("name:(Peter OR Anna)")
                .Returns("name = ( Peter OR Anna )");

            yield return new TestCaseData("(name:Peter OR name:Anna)")
                .Returns("( name = Peter OR name = Anna )");

            yield return new TestCaseData("age: [5 TO 9]")
                .Returns("age: [5 TO 9]");

            yield return new TestCaseData("age: [5 TO *]")
                .Returns("age: [5 TO *]");

            yield return new TestCaseData("age: [* TO 9]")
                .Returns("age: [* TO 9]");

            yield return new TestCaseData("age: [2345 TO 2110]")
                .Returns("age: [2345 TO 2110]");

            yield return new TestCaseData("height: [1.6 TO 2.0]")
                .Returns("height: [1.6 TO 2]");

            yield return new TestCaseData("height: [* TO 2.1]")
                .Returns("height: [* TO 2.1]");

            yield return new TestCaseData("height: [1.6 TO *]")
                .Returns("height: [1.6 TO *]");

            yield return new TestCaseData("born: [2020-03-02 TO 2020-03-25]")
                .Returns("born: [2020-03-02T00:00:00 TO 2020-03-25T00:00:00]");

            yield return new TestCaseData("born: [2020-03-02T21:21 TO 2020-03-25T21:23]")
                .Returns("born: [2020-03-02T21:21:00 TO 2020-03-25T21:23:00]");

            yield return new TestCaseData("born: [2020-03-02T21:21:00 TO 2020-03-25T21:23:00]")
                .Returns("born: [2020-03-02T21:21:00 TO 2020-03-25T21:23:00]");

            yield return new TestCaseData("born: [2020-03-02T21:24:00+01:00 TO 2020-03-25T21:24:00+01:00]")
                .Returns("born: [2020-03-02T20:24:00 TO 2020-03-25T20:24:00]");

            yield return new TestCaseData("born: [* TO 2020-03-25T21:24:00+01:00]")
                .Returns("born: [* TO 2020-03-25T20:24:00]");

            yield return new TestCaseData("born: [2020-03-02T21:24:00+01:00 TO *]")
                .Returns("born: [2020-03-02T20:24:00 TO *]");

            yield return new TestCaseData("born: [-5days TO *]")
                .Returns("born: [-5days TO *]");

            yield return new TestCaseData("born: [TODAY+5D TO *]")
                .Returns("born: [TODAY+5D TO *]");

            yield return new TestCaseData("age: [5 TO 10] ORDER BY foo")
                .Returns("age: [5 TO 10] ORDER BY foo");

            yield return new TestCaseData("  age: [5 TO 10] ORDER BY foo DESC, fax ASC, fox")
                .Returns("age: [5 TO 10] ORDER BY foo DESC, fax ASC, fox");

            yield return new TestCaseData("name: Peter")
                .Returns("name = Peter");

            yield return new TestCaseData("name: Peter AND age: [10 TO 100]")
                .Returns("( name = Peter AND age: [10 TO 100] )");

            yield return new TestCaseData("name: Peter OR name: Anna NOT name: Claus OR name: Hans")
                .Returns("( name = Peter OR ( name = Anna AND NOT name = Claus ) OR name = Hans )");

            yield return new TestCaseData("name: Peter OR name: Anna NOT name: \"Claus Parsø\" OR name: Hans")
                .Returns("( name = Peter OR ( name = Anna AND NOT name = \"Claus Parsø\" ) OR name = Hans )");

            yield return new TestCaseData("name: Peter OR name: Anna NOT name: Claus* OR name: Hans")
                .Returns("( name = Peter OR ( name = Anna AND NOT name = Claus* ) OR name = Hans )");

            yield return new TestCaseData("(name: Peter OR name: Anna) AND (age: 5 OR age: [8 TO 10])")
                .Returns("( ( name = Peter OR name = Anna ) AND ( age = 5 OR age: [8 TO 10] ) )");

            yield return new TestCaseData("ship.name: FooBarasd AND $version: [6 TO *] AND $created: [2020-03-02T21:24:00 TO 2020-03-25] AND contentType: notification AND id: 123")
                .Returns("( ship.name = FooBarasd AND $version: [6 TO *] AND $created: [2020-03-02T21:24:00 TO 2020-03-25T00:00:00] AND contentType = notification AND id = 123 )");

            yield return new TestCaseData("$created: [2020-03-02T21:24:00+01:00 TO 2020-03-25T21:24:00+01:00]")
                .Returns("$created: [2020-03-02T20:24:00 TO 2020-03-25T20:24:00]");

            yield return new TestCaseData("$created: [2020-03-02T21:24:00+01:00 TO *]")
                .Returns("$created: [2020-03-02T20:24:00 TO *]");

            yield return new TestCaseData("$created: [22 TO *]")
                .Returns("$created: [22 TO *]");

            yield return new TestCaseData("ship.name: FooBarasd AND $version: [6 TO 10] AND $created: [2020-03-02T21:24:00+01:00 TO 2020-03-25] AND contentType: notification AND id: 123")
                    .Returns("( ship.name = FooBarasd AND $version: [6 TO 10] AND $created: [2020-03-02T20:24:00 TO 2020-03-25T00:00:00] AND contentType = notification AND id = 123 )");

            yield return new TestCaseData("ship.name: FooBarasd AND $created: [2020-03-02T21:24:00+01:00 TO 2020-03-25] AND contentType: notification AND id: 123")
                    .Returns("( ship.name = FooBarasd AND $created: [2020-03-02T20:24:00 TO 2020-03-25T00:00:00] AND contentType = notification AND id = 123 )");

            yield return new TestCaseData("$version: [6.6 TO *]")
                .Returns("$version: [6.6 TO *]");

            yield return new TestCaseData("$version: [6 TO * ]")
                .Returns("$version: [6 TO *]");

            yield return new TestCaseData("$version: [ 6 TO * ]")
                .Returns("$version: [6 TO *]");

            yield return new TestCaseData("name: Peter Hansen")
                .Returns("( name = Peter, Hansen )");


            yield return new TestCaseData("title: \"Lucene tutorial\" AND content:\"search engine\" AND author:John Smith AND category:programming AND tags: java")
                .Returns("");
            yield return new TestCaseData("title:\"Indexing Techniques\" AND content:\"Best Practices\" AND author:Jane Doe AND category:IT AND tags: (search optimization OR indexing)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene OR Solr) AND content:\"Query Parsing\" AND author:\"John Smith\" AND category:(Programming OR Computer Science) AND tags:(Java OR Python)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene AND Elasticsearch) AND content:\"Search Relevance\" AND author:(Expert OR Guru) AND category:(Information Retrieval OR Search Engines) AND tags:(Algorithms OR Performance)")
                .Returns("");
            yield return new TestCaseData("title: \"Search Optimization\" AND content:\"Advanced Techniques\" AND author:\"Alice Johnson\" AND category:(Data Science OR Web Development) AND tags:(Machine Learning OR Big Data)")
                .Returns("");
            yield return new TestCaseData("title: (Indexing Strategies) AND content:\"Index Maintenance\" AND author:\"Bob White\" AND category:(Software Engineering OR Database Systems) AND tags:(Index Optimization OR Performance Tuning)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene Basics) AND content:\"Full-Text Search\" AND author:(Emily Brown OR David Smith) AND category:(Web Development OR Information Technology) AND tags:(Query Performance OR Information Retrieval)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene AND Solr) AND content:\"Performance Tuning\" AND author:(Expert OR Consultant) AND category:(Search Engines OR Software Development) AND tags:(Indexing Techniques OR Search Optimization)")
                .Returns("");
            yield return new TestCaseData("title: \"Query Processing\" AND content:\"Index Segmentation\" AND author:(George Wilson OR Sarah Jones) AND category:(Computer Science OR Programming Languages) AND tags:(Search Algorithms OR Query Optimization)")
                .Returns("");
            yield return new TestCaseData("title: \"Introduction to Lucene\" AND content:\"Search Architecture\" AND author:(Hannah Martinez OR Kevin Evans) AND category:(Data Analysis OR Software Architecture) AND tags:(Indexing Strategies OR Relevance Scoring)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene OR Solr) AND content:\"Index Optimization\" AND author:(Ian Davis OR Jessica Parker) AND category:(Database Systems OR Web Programming) AND tags:(Index Compression OR Index Merging)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene Tutorial) AND content:\"Index Structure\" AND author:(John Smith OR Linda Thompson) AND category:(Information Retrieval OR Software Engineering) AND tags:(Index Maintenance OR Search Performance)")
                .Returns("");
            yield return new TestCaseData("title: \"Advanced Search Techniques\" AND content:\"Query Optimization\" AND author:(Michael Robinson OR Nancy Lee) AND category:(Software Development OR Programming Paradigms) AND tags:(Search Relevance OR Query Processing)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene AND Elasticsearch) AND content:\"Index Merging\" AND author:(Oscar King OR Patrick Harris) AND category:(Web Development OR Data Analysis) AND tags:(Indexing Techniques OR Index Compression)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene Basics) AND content:\"Index Optimization\" AND author:(Quincy Adams OR Rachel Miller) AND category:(Computer Science OR Software Architecture) AND tags:(Query Performance OR Index Maintenance)")
                .Returns("");
            yield return new TestCaseData("title: \"Introduction to Lucene\" AND content:\"Search Relevance\" AND author:(Sarah Johnson OR Thomas Wilson) AND category:(Software Engineering OR Information Retrieval) AND tags:(Search Optimization OR Ranking Algorithms)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene OR Solr) AND content:\"Index Compression\" AND author:(Ursula Adams OR Victor Lee) AND category:(Database Systems OR Web Programming) AND tags:(Index Merging OR Index Optimization)")
                .Returns("");
            yield return new TestCaseData("title: \"Query Processing\" AND content:\"Index Segmentation\" AND author:(Wendy Davis OR Xavier Harris) AND category:(Programming Languages OR Data Analysis) AND tags:(Search Algorithms OR Indexing Techniques)")
                .Returns("");
            yield return new TestCaseData("title: \"Lucene Overview\" AND content:\"Search Architecture\" AND author:(Yvonne Miller OR Zack Taylor) AND category:(Software Development OR Computer Science) AND tags:(Search Relevance OR Relevance Scoring)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene AND Elasticsearch) AND content:\"Index Optimization\" AND author:(Alice Johnson OR Bob White) AND category:(Software Engineering OR Database Systems) AND tags:(Index Compression OR Index Merging)")
                .Returns("");
            yield return new TestCaseData("title: \"Search Optimization\" AND content:\"Advanced Techniques\" AND author:(Chris Brown OR Diana Lopez) AND category:(Data Science OR Web Development) AND tags:(Machine Learning OR Big Data)")
                .Returns("");
            yield return new TestCaseData("title: (Indexing Strategies) AND content:\"Index Maintenance\" AND author:(Emily Parker OR Frank Adams) AND category:(Software Engineering OR Database Systems) AND tags:(Index Optimization OR Performance Tuning)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene Basics) AND content:\"Full-Text Search\" AND author:(George Wilson OR Hannah Martinez) AND category:(Web Development OR Information Technology) AND tags:(Query Performance OR Information Retrieval)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene AND Solr) AND content:\"Performance Tuning\" AND author:(Ian Davis OR Jessica Parker) AND category:(Search Engines OR Software Development) AND tags:(Indexing Techniques OR Search Optimization)")
                .Returns("");
            yield return new TestCaseData("title: \"Query Processing\" AND content:\"Index Segmentation\" AND author:(Kevin Evans OR Linda Thompson) AND category:(Computer Science OR Programming Languages) AND tags:(Search Algorithms OR Query Optimization)")
                .Returns("");
            yield return new TestCaseData("title: \"Introduction to Lucene\" AND content:\"Search Architecture\" AND author:(Michael Robinson OR Nancy Lee) AND category:(Data Analysis OR Software Architecture) AND tags:(Indexing Strategies OR Relevance Scoring)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene OR Solr) AND content:\"Index Optimization\" AND author:(Oscar King OR Patrick Harris) AND category:(Web Development OR Data Analysis) AND tags:(Indexing Techniques OR Index Compression)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene Basics) AND content:\"Index Structure\" AND author:(Quincy Adams OR Rachel Miller) AND category:(Computer Science OR Software Engineering) AND tags:(Index Maintenance OR Search Performance)")
                .Returns("");
            yield return new TestCaseData("title: \"Advanced Search Techniques\" AND content:\"Query Optimization\" AND author:(Sarah Johnson OR Thomas Wilson) AND category:(Software Development OR Programming Paradigms) AND tags:(Search Relevance OR Query Processing)")
                .Returns("");
            yield return new TestCaseData("title: (Lucene AND Elasticsearch) AND content:\"Index Merging\" AND author:(Ursula Adams OR Victor Lee) AND category:(Database Systems OR Web Programming) AND tags:(Indexing Techniques OR Index Compression)")
                .Returns("");

        }
    }
}

public class SimplifiedLuceneQueryParserTest
{
    [TestCase("*:*", "*:*")]
    [TestCase("simple:value", "simple:value")]
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




