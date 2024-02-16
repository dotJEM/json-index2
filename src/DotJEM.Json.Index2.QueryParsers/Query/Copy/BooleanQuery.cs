using System.Collections.Generic;
using System.Text;
using J2N;
using J2N.Collections.Generic.Extensions;
using Lucene.Net.Search;
using Lucene.Net.Util;
using JCG = J2N.Collections.Generic;
using LuceneQuery = Lucene.Net.Search.Query;

namespace DotJEM.Json.Index2.QueryParsers.Query.Copy;

using AtomicReaderContext = Lucene.Net.Index.AtomicReaderContext;
using IBits = IBits;
using IndexReader = Lucene.Net.Index.IndexReader;
using Occur_e = Occur;
using Similarity = Lucene.Net.Search.Similarities.Similarity;
using Term = Lucene.Net.Index.Term;
using ToStringUtils = ToStringUtils;

public sealed class CopyOfBooleanQuery : LuceneQuery
{
    private IList<BooleanClause> clauses = new List<BooleanClause>();
    private IList<Term> terms = new List<Term>();

    public bool CoordDisabled { get; }

    public int MinimumNumberShouldMatch { get; } = 0;

    public CopyOfBooleanQuery(bool disableCoord = false)
    {
        this.CoordDisabled = disableCoord;
    }

    //public void Add(LuceneQuery query, Occur occur) => Add(new BooleanClause(query, occur));
    //public void Add(BooleanClause clause) => clauses.Add(clause);

    public void AddTerm(Term term) => terms.Add(term);

    public sealed class CopyOfBooleanWeight : Weight
    {
        private readonly CopyOfBooleanQuery outerInstance;
        private readonly Similarity similarity;
        private readonly IList<Weight> weights;
        private readonly bool disableCoord;

        public int MaxCoord { get; }

        public override LuceneQuery Query => outerInstance;

        public CopyOfBooleanWeight(CopyOfBooleanQuery outerInstance, IndexSearcher searcher, bool disableCoord)
        {
            this.outerInstance = outerInstance;
            similarity = searcher.Similarity;
            this.disableCoord = disableCoord;
            weights = new List<Weight>(outerInstance.clauses.Count);
            foreach (BooleanClause c in outerInstance.clauses)
            {
                Weight w = c.Query.CreateWeight(searcher);
                weights.Add(w);
                if (!c.IsProhibited)
                {
                    MaxCoord++;
                }
            }
        }

        public override float GetValueForNormalization()
        {
            float sum = 0.0f;
            for (int i = 0; i < weights.Count; i++)
            {
                // call sumOfSquaredWeights for all clauses in case of side effects
                float s = weights[i].GetValueForNormalization(); // sum sub weights
                if (!outerInstance.clauses[i].IsProhibited)
                {
                    // only add to sum for non-prohibited clauses
                    sum += s;
                }
            }
            sum *= outerInstance.Boost * outerInstance.Boost; // boost each sub-weight
            return sum;
        }

        public float Coord(int overlap, int maxOverlap)
        {
            // LUCENE-4300: in most cases of maxOverlap=1, BQ rewrites itself away,
            // so coord() is not applied. But when BQ cannot optimize itself away
            // for a single clause (minNrShouldMatch, prohibited clauses, etc), its
            // important not to apply coord(1,1) for consistency, it might not be 1.0F
            return maxOverlap == 1 ? 1F : similarity.Coord(overlap, maxOverlap);
        }

        public override void Normalize(float norm, float topLevelBoost)
        {
            topLevelBoost *= outerInstance.Boost; // incorporate boost
            foreach (Weight w in weights)
            {
                // normalize all clauses, (even if prohibited in case of side affects)
                w.Normalize(norm, topLevelBoost);
            }
        }

        public override Explanation Explain(AtomicReaderContext context, int doc)
        {
            int minShouldMatch = outerInstance.MinimumNumberShouldMatch;
            ComplexExplanation sumExpl = new ComplexExplanation();
            sumExpl.Description = "sum of:";
            int coord = 0;
            float sum = 0.0f;
            bool fail = false;
            int shouldMatchCount = 0;

            using (IEnumerator<BooleanClause> cIter = outerInstance.clauses.GetEnumerator())
            {
                foreach (Weight w in weights)
                {
                    cIter.MoveNext();
                    BooleanClause c = cIter.Current;
                    if (w.GetScorer(context, context.AtomicReader.LiveDocs) is null)
                    {
                        if (c.IsRequired)
                        {
                            fail = true;
                            Explanation r = new Explanation(0.0f, "no match on required clause (" + c.Query.ToString() + ")");
                            sumExpl.AddDetail(r);
                        }
                        continue;
                    }
                    Explanation e = w.Explain(context, doc);
                    if (e.IsMatch)
                    {
                        if (!c.IsProhibited)
                        {
                            sumExpl.AddDetail(e);
                            sum += e.Value;
                            coord++;
                        }
                        else
                        {
                            Explanation r = new Explanation(0.0f, "match on prohibited clause (" + c.Query.ToString() + ")");
                            r.AddDetail(e);
                            sumExpl.AddDetail(r);
                            fail = true;
                        }
                        if (c.Occur == Occur_e.SHOULD)
                        {
                            shouldMatchCount++;
                        }
                    }
                    else if (c.IsRequired)
                    {
                        Explanation r = new Explanation(0.0f, "no match on required clause (" + c.Query.ToString() + ")");
                        r.AddDetail(e);
                        sumExpl.AddDetail(r);
                        fail = true;
                    }
                }
            }
            if (fail)
            {
                sumExpl.Match = false;
                sumExpl.Value = 0.0f;
                sumExpl.Description = "Failure to meet condition(s) of required/prohibited clause(s)";
                return sumExpl;
            }
            else if (shouldMatchCount < minShouldMatch)
            {
                sumExpl.Match = false;
                sumExpl.Value = 0.0f;
                sumExpl.Description = "Failure to match minimum number " + "of optional clauses: " + minShouldMatch;
                return sumExpl;
            }

            sumExpl.Match = 0 < coord ? true : false;
            sumExpl.Value = sum;

            float coordFactor = disableCoord ? 1.0f : Coord(coord, MaxCoord);
            if (coordFactor == 1.0f)
            {
                return sumExpl; // eliminate wrapper
            }
            else
            {
                ComplexExplanation result = new ComplexExplanation(sumExpl.IsMatch, sum * coordFactor, "product of:");
                result.AddDetail(sumExpl);
                result.AddDetail(new Explanation(coordFactor, "coord(" + coord + "/" + MaxCoord + ")"));
                return result;
            }
        }

        public override BulkScorer GetBulkScorer(AtomicReaderContext context, bool scoreDocsInOrder, IBits acceptDocs)
        {
            if (scoreDocsInOrder || outerInstance.MinimumNumberShouldMatch > 1)
            {
                // TODO: (LUCENE-4872) in some cases BooleanScorer may be faster for minNrShouldMatch
                // but the same is even true of pure conjunctions...
                return base.GetBulkScorer(context, scoreDocsInOrder, acceptDocs);
            }

            IList<BulkScorer> prohibited = new JCG.List<BulkScorer>();
            IList<BulkScorer> optional = new JCG.List<BulkScorer>();
            using (IEnumerator<BooleanClause> cIter = outerInstance.clauses.GetEnumerator())
            {
                foreach (Weight w in weights)
                {
                    cIter.MoveNext();
                    BooleanClause c = cIter.Current;
                    BulkScorer subScorer = w.GetBulkScorer(context, false, acceptDocs);
                    if (subScorer is null)
                    {
                        if (c.IsRequired)
                        {
                            return null;
                        }
                    }
                    else if (c.IsRequired)
                    {
                        // TODO: there are some cases where BooleanScorer
                        // would handle conjunctions faster than
                        // BooleanScorer2...
                        return base.GetBulkScorer(context, scoreDocsInOrder, acceptDocs);
                    }
                    else if (c.IsProhibited)
                    {
                        prohibited.Add(subScorer);
                    }
                    else
                    {
                        optional.Add(subScorer);
                    }
                }
            }

            // Check if we can and should return a BooleanScorer
            return new CopyOfBooleanScorer(this, disableCoord, outerInstance.MinimumNumberShouldMatch, optional, prohibited, MaxCoord);
        }

        public override Scorer GetScorer(AtomicReaderContext context, IBits acceptDocs)
        {
            IList<Scorer> required = new JCG.List<Scorer>();
            IList<Scorer> prohibited = new JCG.List<Scorer>();
            IList<Scorer> optional = new JCG.List<Scorer>();
            IEnumerator<BooleanClause> cIter = outerInstance.clauses.GetEnumerator();
            foreach (Weight w in weights)
            {
                cIter.MoveNext();
                BooleanClause c = cIter.Current;
                Scorer subScorer = w.GetScorer(context, acceptDocs);
                if (subScorer is null)
                {
                    if (c.IsRequired)
                    {
                        return null;
                    }
                }
                else if (c.IsRequired)
                {
                    required.Add(subScorer);
                }
                else if (c.IsProhibited)
                {
                    prohibited.Add(subScorer);
                }
                else
                {
                    optional.Add(subScorer);
                }
            }

            // no required and optional clauses.
            if (required.Count == 0 && optional.Count == 0)
                return null;

            // either >1 req scorer, or there are 0 req scorers and at least 1
            // optional scorer. Therefore if there are not enough optional scorers
            // no documents will be matched by the query
            if (optional.Count < outerInstance.MinimumNumberShouldMatch)
                return null;

            // simple conjunction
            if (optional.Count == 0 && prohibited.Count == 0)
            {
                float coord = disableCoord 
                    ? 1.0f 
                    : Coord(required.Count, MaxCoord);
                return new CopyOfConjunctionScorer(this, required.ToArray(), coord);
            }

            // simple disjunction
            if (required.Count == 0 && prohibited.Count == 0 && outerInstance.MinimumNumberShouldMatch <= 1 && optional.Count > 1)
            {
                var coord = new float[optional.Count + 1];
                for (int i = 0; i < coord.Length; i++)
                {
                    coord[i] = disableCoord ? 1.0f : Coord(i, MaxCoord);
                }
                return new CopyOfDisjunctionSumScorer(this, optional.ToArray(), coord);
            }

            // Return a BooleanScorer2
            return new CopyOfBooleanScorer2(this, disableCoord, outerInstance.MinimumNumberShouldMatch, required, prohibited, optional, MaxCoord);
        }

        public override bool ScoresDocsOutOfOrder
        {
            get
            {
                if (outerInstance.MinimumNumberShouldMatch > 1)
                {
                    // BS2 (in-order) will be used by scorer()
                    return false;
                }
                foreach (BooleanClause c in outerInstance.clauses)
                {
                    if (c.IsRequired)
                    {
                        // BS2 (in-order) will be used by scorer()
                        return false;
                    }
                }

                // scorer() will return an out-of-order scorer if requested.
                return true;
            }
        }
    }

    public override Weight CreateWeight(IndexSearcher searcher)
    {
        return new CopyOfBooleanWeight(this, searcher, CoordDisabled);
    }

    public override LuceneQuery Rewrite(IndexReader reader)
    {
        if (MinimumNumberShouldMatch == 0 && clauses.Count == 1) // optimize 1-clause queries
        {
            BooleanClause c = clauses[0];
            if (!c.IsProhibited) // just return clause
            {
                Lucene.Net.Search.Query query = c.Query.Rewrite(reader); // rewrite first

                if (Boost != 1.0f) // incorporate boost
                {
                    if (query == c.Query) // if rewrite was no-op
                    {
                        query = (Lucene.Net.Search.Query)query.Clone(); // then clone before boost
                    }
                    // Since the BooleanQuery only has 1 clause, the BooleanQuery will be
                    // written out. Therefore the rewritten Query's boost must incorporate both
                    // the clause's boost, and the boost of the BooleanQuery itself
                    query.Boost = Boost * query.Boost;
                }

                return query;
            }
        }

        CopyOfBooleanQuery clone = null; // recursively rewrite
        for (int i = 0; i < clauses.Count; i++)
        {
            BooleanClause c = clauses[i];
            LuceneQuery query = c.Query.Rewrite(reader);
            if (query != c.Query) // clause rewrote: must clone
            {
                if (clone is null)
                {
                    // The BooleanQuery clone is lazily initialized so only initialize
                    // it if a rewritten clause differs from the original clause (and hasn't been
                    // initialized already).  If nothing differs, the clone isn't needlessly created
                    clone = (CopyOfBooleanQuery)Clone();
                }
                clone.clauses[i] = new BooleanClause(query, c.Occur);
            }
        }
        return clone ?? // some clauses rewrote
               this; // no clauses rewrote
    }

    public override void ExtractTerms(ISet<Term> terms)
    {
        foreach (BooleanClause clause in clauses)
        {
            if (clause.Occur != Occur_e.MUST_NOT)
            {
                clause.Query.ExtractTerms(terms);
            }
        }
    }

    public override object Clone()
    {
        CopyOfBooleanQuery clone = (CopyOfBooleanQuery)base.Clone();
        clone.clauses = new JCG.List<BooleanClause>(clauses);
        return clone;
    }

    /// <summary>
    /// Prints a user-readable version of this query. </summary>
    public override string ToString(string field)
    {
        StringBuilder buffer = new StringBuilder();
        bool needParens = Boost != 1.0 || MinimumNumberShouldMatch > 0;
        if (needParens)
        {
            buffer.Append('(');
        }

        for (int i = 0; i < clauses.Count; i++)
        {
            BooleanClause c = clauses[i];
            if (c.IsProhibited)
            {
                buffer.Append('-');
            }
            else if (c.IsRequired)
            {
                buffer.Append('+');
            }

            Lucene.Net.Search.Query subQuery = c.Query;
            if (subQuery != null)
            {
                if (subQuery is BooleanQuery) // wrap sub-bools in parens
                {
                    buffer.Append('(');
                    buffer.Append(subQuery.ToString(field));
                    buffer.Append(')');
                }
                else
                {
                    buffer.Append(subQuery.ToString(field));
                }
            }
            else
            {
                buffer.Append("null");
            }

            if (i != clauses.Count - 1)
            {
                buffer.Append(' ');
            }
        }

        if (needParens)
        {
            buffer.Append(')');
        }

        if (MinimumNumberShouldMatch > 0)
        {
            buffer.Append('~');
            buffer.Append(MinimumNumberShouldMatch);
        }

        if (Boost != 1.0f)
        {
            buffer.Append(ToStringUtils.Boost(Boost));
        }

        return buffer.ToString();
    }

    public override bool Equals(object o)
    {
        if (o is not CopyOfBooleanQuery other)
            return false;
        
        // LUCENENET specific - compare bits rather than using equality operators to prevent these comparisons from failing in x86 in .NET Framework with optimizations enabled
        return NumericUtils.SingleToSortableInt32(Boost) == NumericUtils.SingleToSortableInt32(other.Boost)
               && clauses.Equals(other.clauses)
               && MinimumNumberShouldMatch == other.MinimumNumberShouldMatch
               && CoordDisabled == other.CoordDisabled;
    }

    public override int GetHashCode()
    {
        return BitConversion.SingleToInt32Bits(Boost) ^ clauses.GetHashCode()
            + MinimumNumberShouldMatch + (CoordDisabled ? 17 : 0);
    }
}