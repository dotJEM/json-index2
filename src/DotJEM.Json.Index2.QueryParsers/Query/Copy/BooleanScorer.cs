using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Search;

namespace DotJEM.Json.Index2.QueryParsers.Query.Copy;

using AtomicReaderContext = Lucene.Net.Index.AtomicReaderContext;
using BooleanWeight = BooleanQuery.BooleanWeight;


internal sealed class CopyOfBooleanScorer : BulkScorer
{
    private sealed class BooleanScorerCollector : ICollector
    {
        private readonly BucketTable bucketTable; // LUCENENET: marked readonly
        private readonly int mask; // LUCENENET: marked readonly
        private Scorer scorer;

        public BooleanScorerCollector(int mask, BucketTable bucketTable)
        {
            this.mask = mask;
            this.bucketTable = bucketTable;
        }

        public void Collect(int doc)
        {
            BucketTable table = bucketTable;
            int i = doc & BucketTable.MASK;
            Bucket bucket = table.buckets[i];

            if (bucket.Doc != doc) // invalid bucket
            {
                bucket.Doc = doc; // set doc
                bucket.Score = scorer.GetScore(); // initialize score
                bucket.Bits = mask; // initialize mask
                bucket.Coord = 1; // initialize coord

                bucket.Next = table.first; // push onto valid list
                table.first = bucket;
            } // valid bucket
            else
            {
                bucket.Score += scorer.GetScore(); // increment score
                bucket.Bits |= mask; // add bits in mask
                bucket.Coord++; // increment coord
            }
        }

        public void SetNextReader(AtomicReaderContext context)
        {
            // not needed by this implementation
        }

        public void SetScorer(Scorer scorer)
        {
            this.scorer = scorer;
        }

        public bool AcceptsDocsOutOfOrder => true;
    }

    internal sealed class Bucket
    {
        internal int Doc { get; set; } // tells if bucket is valid
        internal double Score { get; set; } // incremental score

        // TODO: break out bool anyProhibited, int
        // numRequiredMatched; then we can remove 32 limit on
        // required clauses
        internal int Bits { get; set; } // used for bool constraints

        internal int Coord { get; set; } // count of terms in score
        internal Bucket Next { get; set; } // next valid bucket

        public Bucket()
        {
            // Initialize properties
            Doc = -1;
        }
    }

    /// <summary>
    /// A simple hash table of document scores within a range. </summary>
    internal sealed class BucketTable
    {
        public const int SIZE = 1 << 11;
        public const int MASK = SIZE - 1;

        internal readonly Bucket[] buckets = new Bucket[SIZE];
        internal Bucket first = null; // head of valid list

        public BucketTable()
        {
            // Pre-fill to save the lazy init when collecting
            // each sub:
            for (int idx = 0; idx < SIZE; idx++)
            {
                buckets[idx] = new Bucket();
            }
        }

        public ICollector NewCollector(int mask)
        {
            return new BooleanScorerCollector(mask, this);
        }

        public static int Count => SIZE; // LUCENENET NOTE: This was size() in Lucene. // LUCENENET: CA1822: Mark members as static
    }

    internal sealed class SubScorer
    {
        public BulkScorer Scorer { get; set; }

        // TODO: re-enable this if BQ ever sends us required clauses
        //public boolean required = false;
        public bool Prohibited { get; set; }

        public ICollector Collector { get; set; }
        public SubScorer Next { get; set; }
        public bool More { get; set; }

        public SubScorer(BulkScorer scorer, bool required, bool prohibited, ICollector collector, SubScorer next)
        {
            if (required)
            {
                throw new ArgumentException("this scorer cannot handle required=true");
            }
            Scorer = scorer;
            More = true;
            // TODO: re-enable this if BQ ever sends us required clauses
            //this.required = required;
            Prohibited = prohibited;
            Collector = collector;
            Next = next;
        }
    }

    private readonly SubScorer scorers = null; // LUCENENET: marked readonly
    private readonly BucketTable bucketTable = new BucketTable(); // LUCENENET: marked readonly
    private readonly float[] coordFactors;

    // TODO: re-enable this if BQ ever sends us required clauses
    //private int requiredMask = 0;
    private readonly int minNrShouldMatch;

    private int end;
    private Bucket current;

    // Any time a prohibited clause matches we set bit 0:
    private const int PROHIBITED_MASK = 1;

    //private readonly Weight weight; // LUCENENET: Never read

    internal CopyOfBooleanScorer(CopyOfBooleanQuery.CopyOfBooleanWeight weight, bool disableCoord, int minNrShouldMatch, IList<BulkScorer> optionalScorers, IList<BulkScorer> prohibitedScorers, int maxCoord)
    {
        this.minNrShouldMatch = minNrShouldMatch;
        //this.weight = weight; // LUCENENET: Never read

        foreach (BulkScorer scorer in optionalScorers)
        {
            scorers = new SubScorer(scorer, false, false, bucketTable.NewCollector(0), scorers);
        }

        foreach (BulkScorer scorer in prohibitedScorers)
        {
            scorers = new SubScorer(scorer, false, true, bucketTable.NewCollector(PROHIBITED_MASK), scorers);
        }

        coordFactors = new float[optionalScorers.Count + 1];
        for (int i = 0; i < coordFactors.Length; i++)
        {
            coordFactors[i] = disableCoord ? 1.0f : weight.Coord(i, maxCoord);
        }
    }

    public override bool Score(ICollector collector, int max)
    {
        bool more;
        Bucket tmp;
        CopyOfFakeScorer fs = new CopyOfFakeScorer();

        // The internal loop will set the score and doc before calling collect.
        collector.SetScorer(fs);
        do
        {
            bucketTable.first = null;

            while (current != null) // more queued
            {
                // check prohibited & required
                if ((current.Bits & PROHIBITED_MASK) == 0)
                {
                    // TODO: re-enable this if BQ ever sends us required
                    // clauses
                    //&& (current.bits & requiredMask) == requiredMask) {
                    // NOTE: Lucene always passes max =
                    // Integer.MAX_VALUE today, because we never embed
                    // a BooleanScorer inside another (even though
                    // that should work)... but in theory an outside
                    // app could pass a different max so we must check
                    // it:
                    if (current.Doc >= max)
                    {
                        tmp = current;
                        current = current.Next;
                        tmp.Next = bucketTable.first;
                        bucketTable.first = tmp;
                        continue;
                    }

                    if (current.Coord >= minNrShouldMatch)
                    {
                        fs.score = (float)(current.Score * coordFactors[current.Coord]);
                        fs.doc = current.Doc;
                        fs.freq = current.Coord;
                        collector.Collect(current.Doc);
                    }
                }

                current = current.Next; // pop the queue
            }

            if (bucketTable.first != null)
            {
                current = bucketTable.first;
                bucketTable.first = current.Next;
                return true;
            }

            // refill the queue
            more = false;
            end += BucketTable.SIZE;
            for (SubScorer sub = scorers; sub != null; sub = sub.Next)
            {
                if (sub.More)
                {
                    sub.More = sub.Scorer.Score(sub.Collector, end);
                    more |= sub.More;
                }
            }
            current = bucketTable.first;
        } while (current != null || more);

        return false;
    }

    public override string ToString()
    {
        StringBuilder buffer = new StringBuilder();
        buffer.Append("boolean(");
        for (SubScorer sub = scorers; sub != null; sub = sub.Next)
        {
            buffer.Append(sub.Scorer.ToString());
            buffer.Append(' ');
        }
        buffer.Append(')');
        return buffer.ToString();
    }
}

internal sealed class CopyOfFakeScorer : Scorer
{
    internal float score;
    internal int doc = -1;
    internal int freq = 1;

    public CopyOfFakeScorer()
        : base(null)
    {
    }

    public override int Advance(int target)
    {
        throw new InvalidOperationException("FakeScorer doesn't support advance(int)");
    }

    public override int DocID => doc;

    public override int Freq => freq;

    public override int NextDoc()
    {
        throw new InvalidOperationException("FakeScorer doesn't support nextDoc()");
    }

    public override float GetScore()
    {
        return score;
    }

    public override long GetCost()
    {
        return 1;
    }

    public override Weight Weight => throw new InvalidOperationException();

    public override ICollection<ChildScorer> GetChildren() => throw new InvalidOperationException();
}