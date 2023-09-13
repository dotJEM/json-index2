using System.Collections.Generic;
using System.Diagnostics;
using Lucene.Net.Search;
using JCG = J2N.Collections.Generic;

namespace DotJEM.Json.Index2.QueryParsers.Query.Copy;

internal abstract class CopyOfDisjunctionScorer : Scorer
{
    protected readonly Scorer[] m_subScorers;

    protected int m_doc = -1;
    protected int m_numScorers;

    protected CopyOfDisjunctionScorer(Weight weight, Scorer[] subScorers)
        : base(weight)
    {
        this.m_subScorers = subScorers;
        this.m_numScorers = subScorers.Length;
        Heapify();
    }

    /// <summary>
    /// Organize subScorers into a min heap with scorers generating the earliest document on top.
    /// </summary>
    protected void Heapify()
    {
        for (int i = (m_numScorers >> 1) - 1; i >= 0; i--)
        {
            HeapAdjust(i);
        }
    }

    protected void HeapAdjust(int root)
    {
        Scorer scorer = m_subScorers[root];
        int doc = scorer.DocID;
        int i = root;
        while (i <= (m_numScorers >> 1) - 1)
        {
            int lchild = (i << 1) + 1;
            Scorer lscorer = m_subScorers[lchild];
            int ldoc = lscorer.DocID;
            int rdoc = int.MaxValue, rchild = (i << 1) + 2;
            Scorer rscorer = null;
            if (rchild < m_numScorers)
            {
                rscorer = m_subScorers[rchild];
                rdoc = rscorer.DocID;
            }
            if (ldoc < doc)
            {
                if (rdoc < ldoc)
                {
                    m_subScorers[i] = rscorer;
                    m_subScorers[rchild] = scorer;
                    i = rchild;
                }
                else
                {
                    m_subScorers[i] = lscorer;
                    m_subScorers[lchild] = scorer;
                    i = lchild;
                }
            }
            else if (rdoc < doc)
            {
                m_subScorers[i] = rscorer;
                m_subScorers[rchild] = scorer;
                i = rchild;
            }
            else
            {
                return;
            }
        }
    }

    protected void HeapRemoveRoot()
    {
        if (m_numScorers == 1)
        {
            m_subScorers[0] = null;
            m_numScorers = 0;
        }
        else
        {
            m_subScorers[0] = m_subScorers[m_numScorers - 1];
            m_subScorers[m_numScorers - 1] = null;
            --m_numScorers;
            HeapAdjust(0);
        }
    }

    public sealed override ICollection<ChildScorer> GetChildren()
    {
        IList<ChildScorer> children = new JCG.List<ChildScorer>(m_numScorers);
        for (int i = 0; i < m_numScorers; i++)
        {
            children.Add(new ChildScorer(m_subScorers[i], "SHOULD"));
        }
        return children;
    }

    public override long GetCost()
    {
        long sum = 0;
        for (int i = 0; i < m_numScorers; i++)
        {
            sum += m_subScorers[i].GetCost();
        }
        return sum;
    }

    public override int DocID => m_doc;

    public override int NextDoc()
    {
        Debug.Assert(m_doc != NO_MORE_DOCS);
        while (true)
        {
            if (m_subScorers[0].NextDoc() != NO_MORE_DOCS)
            {
                HeapAdjust(0);
            }
            else
            {
                HeapRemoveRoot();
                if (m_numScorers == 0)
                {
                    return m_doc = NO_MORE_DOCS;
                }
            }
            if (m_subScorers[0].DocID != m_doc)
            {
                AfterNext();
                return m_doc;
            }
        }
    }

    public override int Advance(int target)
    {
        while (true)
        {
            if (m_subScorers[0].Advance(target) != NO_MORE_DOCS)
            {
                HeapAdjust(0);
            }
            else
            {
                HeapRemoveRoot();
                if (m_numScorers == 0)
                {
                    return m_doc = NO_MORE_DOCS;
                }
            }
            if (m_subScorers[0].DocID >= target)
            {
                AfterNext();
                return m_doc;
            }
        }
    }

    protected abstract void AfterNext();
}