using System;
using DotJEM.Json.Index2.Configuration;
using DotJEM.Json.Index2.Documents.Info;
using DotJEM.Json.Index2.Results;
using DotJEM.Json.Index2.Searching;
using Lucene.Net.Analysis;

namespace DotJEM.Json.Index2.QueryParsers
{
    public static class IndexSearcherExtensions
    {
        public static IJsonIndexBuilder UseSimplifiedLuceneQueryParser(this IJsonIndexBuilder self) 
            => self.TryWithService<ILuceneQueryParser>(config=>new SimplifiedLuceneQueryParser(config.FieldInformationManager, config.Analyzer));

        public static ISearch Search(this IJsonIndexSearcher self, string query)
        {
            ILuceneQueryParser parser = self.Index.ResolveParser();
            LuceneQueryInfo queryInfo = parser.Parse(query);
            return self.Search(queryInfo.Query).OrderBy(queryInfo.Sort);
        }

        public static ISearch Search(this IJsonIndex self, string query)
        {
            ILuceneQueryParser parser = self.ResolveParser();
            LuceneQueryInfo queryInfo = parser.Parse(query);
            return self.CreateSearcher().Search(queryInfo.Query).OrderBy(queryInfo.Sort);
        }

        private static ILuceneQueryParser ResolveParser(this IJsonIndex self)
        {
            return self.Configuration.Get<ILuceneQueryParser>() ?? throw new Exception("Query parser not configured.");
        }

    }
}
