using System;
using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.Results;
using DotJEM.Json.Index2.Searching;
using Lucene.Net.Analysis;

namespace DotJEM.Json.Index2.QueryParsers;

public static class IndexQueryParserExtensions
{
    public static IJsonIndexBuilder WithSimplifiedLuceneQueryParser(this IJsonIndexBuilder self) 
        => self.TryWithService<ILuceneQueryParser>(config=>new SimplifiedLuceneQueryParser(config.FieldInformationManager, config.Analyzer));

    public static ISearch Search(this IJsonIndexSearcher self, string query)
    {
        ILuceneQueryParser parser = self.Index.Configuration.ResolveParser();
        LuceneQueryInfo queryInfo = parser.Parse(query);
        return self.Search(queryInfo.Query).OrderBy(queryInfo.Sort);
    }

    public static ISearch Search(this IJsonIndex self, string query)
    {
        ILuceneQueryParser parser = self.Configuration.ResolveParser();
        LuceneQueryInfo queryInfo = parser.Parse(query);
        return self.CreateSearcher().Search(queryInfo.Query).OrderBy(queryInfo.Sort);
    }

    private static ILuceneQueryParser ResolveParser(this IJsonIndexConfiguration self)
    {
        return self.Get<ILuceneQueryParser>() ?? throw new Exception("Query parser not configured.");
    }

}