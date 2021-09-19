using Nest;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public static class QueryHelper
    {
        public static SortDescriptor<T> GetSortDescriptor<T>(
            this IEnumerable<ISortField> sortFields)
            where T : class, IDocument
        {
            var desc = new SortDescriptor<T>();

            if (sortFields == null)
                return desc;

            foreach (var s in sortFields.OrderBy(x => x.Ordinal))
            {
                if (s.SortAscending)
                    desc.Ascending(s.FieldName);
                else
                    desc.Descending(s.FieldName);
            }

            return desc;
        }

        public static QueryContainerDescriptor<T> GetQueryDesctiptor<T>(
            this IEnumerable<IFilterField> filters)
            where T : class, IDocument
        {
            var desc = new QueryContainerDescriptor<T>();

            if (filters == null || filters.Any() == false)
            {
                desc.MatchAll();
                return desc;
            }

            var q = new List<QueryContainer>();

            foreach (var f in filters)
            {
                q.Add(ComparatorCache.GetQueryContainer(f));
            }

            desc.Bool(b => b.Must(q.ToArray()));

            return desc;
        }

        public static SourceFilterDescriptor<T> GetSourceFilterDescriptor<T>(
            this IEnumerable<IReturnField> fields)
            where T : class, IDocument
        {
            var sf = new SourceFilterDescriptor<T>();
            if (fields == null)
                sf.IncludeAll();
            else if (fields?.Any() == true)
                sf.Includes(i =>
                    i.Fields(fields.Select(x => x.FieldName).ToArray()));
            else
                sf.ExcludeAll();
            return sf;
        }

        public static AggregationContainerDescriptor<T> GetBucketAggreagationDescriptor<T>(
            this IEnumerable<IBucketField> fields)
            where T : class, IDocument
        {
            var res = new AggregationContainerDescriptor<T>();

            if (fields == null)
                return res;

            foreach (var f in fields)
            {
                res.Terms(f.FieldName, t => t
                    .Field(f.FieldName)
                    .MinimumDocumentCount(2)
                    .Order(o => o.CountDescending().KeyAscending()));
            }

            return res;
        }
        public static IEnumerable<IBucketResult> BuildBucketResult(
            this Nest.AggregateDictionary aggs)
        {
            var res = new List<DefaultBucketResult>();

            if (aggs != null)
            {
                foreach (var a in aggs.Keys)
                {
                    var terms = aggs.Terms(a);
                    if (terms != null)
                    {
                        var buckets = terms.Buckets;
                        if (buckets != null)
                        {
                            foreach (var b in buckets)
                            {
                                if (b?.Any() != true && b.DocCount < 2)
                                    continue;

                                res.Add(new DefaultBucketResult(b.Key, b.DocCount));
                            }
                        }
                    }
                }
            }

            return res;
        }
    }
}
