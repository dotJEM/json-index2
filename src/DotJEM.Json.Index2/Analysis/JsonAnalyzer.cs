using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace DotJEM.Json.Index2.Analysis;

/// <summary>
/// 
/// </summary>
// TODO: This is a copy of https://github.com/apache/lucenenet/blob/b1476aee4fe21131899c1f43b2e06e25971b3ebe/src/Lucene.Net.Analysis.Common/Analysis/Standard/ClassicAnalyzer.cs
// followed by a trimming of unneeded features. But we should do more to make it especially useful for JSON.
public class JsonAnalyzer : Analyzer
{
    public LuceneVersion Version { get; }
    public int MaxTokenLength { get; set; } = 4096;

    public JsonAnalyzer(LuceneVersion version)
    {
        Version = version;
    }

    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        ClassicTokenizer src = new (Version, reader);
        src.MaxTokenLength = MaxTokenLength;
        TokenStream tok = new ClassicFilter(src);
        tok = new LowerCaseFilter(Version, tok);
        return new TokenStreamComponentsAnonymousClass(this, src, tok);
    }

    private sealed class TokenStreamComponentsAnonymousClass : TokenStreamComponents
    {
        private readonly JsonAnalyzer analyzer;

        private readonly ClassicTokenizer src;

        public TokenStreamComponentsAnonymousClass(JsonAnalyzer analyzer, ClassicTokenizer src, TokenStream tok)
            : base(src, tok)
        {
            this.analyzer = analyzer;
            this.src = src;
        }

        protected override void SetReader(TextReader reader)
        {
            src.MaxTokenLength = analyzer.MaxTokenLength;
            base.SetReader(reader);
        }
    }

}