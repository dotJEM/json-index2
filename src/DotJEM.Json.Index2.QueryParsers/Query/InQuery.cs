using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
namespace DotJEM.Json.Index2.QueryParsers.Query;

public class InQuery: MultiTermQuery
{
    private readonly BytesRef[] values;

    public InQuery(string field, params string[] values) 
        : base(field)
    {
        this.values = values
            .OrderBy(v => v)
            .Select(x => new BytesRef( x )).ToArray();
    }

    public override string ToString(string field)
    {
        StringBuilder buffer = new StringBuilder();
        if (!Field.Equals(field, StringComparison.Ordinal))
        {
            buffer.Append(Field);
            buffer.Append(':');
        }
        buffer.Append('(');
        for (int i = 0; i < values.Length; i++)
        {
            BytesRef bytesRef = values[i];
            if(i != 0) buffer.Append(", ");
            buffer.Append(bytesRef.Utf8ToString());
        }
        buffer.Append(')');
        buffer.Append(ToStringUtils.Boost(Boost));
        return buffer.ToString();
    }

    protected override TermsEnum GetTermsEnum(Terms terms, AttributeSource atts)
    {
        TermsEnum termsEnum = terms.GetEnumerator();
        return new InTermsEnum(termsEnum, this.values);
    }
}

public class InTermsEnum : TermsEnum
{
    private readonly TermsEnum inner;
    private readonly BytesRef[] terms;
    private int index = 0;
    private BytesRef current;

    public override IComparer<BytesRef> Comparer => inner.Comparer;
    public override BytesRef Term => inner.Term;
    public override long Ord => inner.Ord;
    public override int DocFreq => inner.DocFreq;
    public override long TotalTermFreq => inner.TotalTermFreq;
        
    public InTermsEnum(TermsEnum inner, BytesRef[] terms)
    {
        this.inner = inner;
        this.terms = terms;
    }

    public override BytesRef Next()
    {
        if (MoveNext())
            return current;
        return null;
    }

    public override bool MoveNext()
    {
        while (true)
        {
            if (index >= terms.Length)
                return false;

            BytesRef next = terms[index++];
            if (inner.SeekCeil(next) == SeekStatus.END)
                return false;

            if(!next.BytesEquals(inner.Term))
                continue;

            current = inner.Term;
            return true;
        }
    }

    public override TermState GetTermState() => inner.GetTermState();
    public override SeekStatus SeekCeil(BytesRef text) => throw new NotSupportedException();
    public override void SeekExact(long ord) => throw new NotSupportedException();
    public override bool SeekExact(BytesRef text) => throw new NotSupportedException();
    public override void SeekExact(BytesRef term, TermState state) => throw new NotSupportedException();
    public override DocsEnum Docs(IBits liveDocs, DocsEnum reuse, DocsFlags flags) => inner.Docs(liveDocs, reuse, flags);
    public override DocsAndPositionsEnum DocsAndPositions(IBits liveDocs, DocsAndPositionsEnum reuse, DocsAndPositionsFlags flags) => inner.DocsAndPositions(liveDocs, reuse, flags);
}