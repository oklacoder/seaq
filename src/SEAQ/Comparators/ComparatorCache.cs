using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public static class ComparatorCache
    {

        private static Dictionary<string, Func<IComparator, IFilterField, QueryContainer>> comparatorQueries;

        public static QueryContainer GetQueryContainer(IFilterField filter)
        {
            return comparatorQueries[filter.Comparator.Value]?.Invoke(filter.Comparator, filter);
        }
        public static bool CacheComparator(IComparator comparator, Func<IComparator, IFilterField, QueryContainer> function)
        {
            return comparatorQueries.TryAdd(comparator.Value, function);
        }

        static ComparatorCache()
        {
            Func<IComparator, IFilterField, QueryContainer> getQuery = GetQuery;

            comparatorQueries = DefaultComparator.DefaultComparators.ToDictionary(x => x.Value, x => getQuery);
        }

        private static QueryContainer GetQuery(
            IComparator c,
            IFilterField filter)
        {
            return c switch
            {
                AnyWordComparator aw => GetQuery(aw, filter),
                BetweenComparator bw => GetQuery(bw, filter),
                EqualComparator eq => GetQuery(eq, filter),
                FullPhraseComparator full => GetQuery(full, filter),
                GreaterThanComparator gt => GetQuery(gt, filter),
                GreaterThanOrEqualComparator gte => GetQuery(gte, filter),
                LessThanComparator lt => GetQuery(lt, filter),
                LessThanOrEqualComparator lte => GetQuery(lte, filter),
                NotAnyWordComparator naw => GetQuery(naw, filter),
                NotEqualComparator neq => GetQuery(neq, filter),
                PartialPhraseComparator part => GetQuery(part, filter),

                _ => throw new TypeAccessException($"Type {c.GetType().FullName} not recognized as a valid comparator.")
            };
        }
        private static QueryContainer GetQuery(AnyWordComparator c, IFilterField filter)
        {
            return new QueryContainerDescriptor<BaseDocument>().Match(m => m.Field(filter.FieldName).Query(filter.Value));
        }
        private static QueryContainer GetQuery(BetweenComparator c, IFilterField filter)
        {
            var parts = filter.Value.Split(Constants.TextPartSeparator, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new ArgumentException($"The provided value {filter.Value} is not valid for the comparator {c.Display}.");

            var start = parts.First();
            var end = parts.Last();

            var startIsNum = Double.TryParse(start, out Double startNum);
            var endIsNum = Double.TryParse(end, out Double endNum);
            var startIsDate = DateTime.TryParse(start, out DateTime startDate);
            var endIsDate = DateTime.TryParse(end, out DateTime endDate);

            if (startIsNum && endIsNum)
            {
                return new QueryContainerDescriptor<BaseDocument>().Range(rng => rng.Field(filter.FieldName).GreaterThanOrEquals(startNum).LessThanOrEquals(endNum));
            }
            else if (startIsDate && endIsDate)
            {
                return new QueryContainerDescriptor<BaseDocument>().DateRange(rng => rng.Field(filter.FieldName).GreaterThanOrEquals(startDate).LessThanOrEquals(endDate));
            }
            else
            {
                throw new ArgumentException($"The value {filter.Value} provided for field {filter.FieldName} is not of a correct type for the provided comparator {filter.Comparator}");
            }
        }
        private static QueryContainer GetQuery(EqualComparator c, IFilterField filter)
        {
            return new QueryContainerDescriptor<BaseDocument>().Bool(b => b.Must(m => m.Match(ma => ma.Field(filter.FieldName).Query(filter.Value))));
        }
        private static QueryContainer GetQuery(FullPhraseComparator c, IFilterField filter)
        {
            return new QueryContainerDescriptor<BaseDocument>().Bool(b => b.Must(mu => mu.Match(m => m.Field(filter.FieldName).Query(filter.Value))));
        }
        private static QueryContainer GetQuery(GreaterThanComparator c, IFilterField filter)
        {
            if (double.TryParse(filter.Value, out var gtDoub))
            {
                return new QueryContainerDescriptor<object>().Range(r => r.Field(filter.FieldName).GreaterThan(gtDoub));
            }
            else if (DateTime.TryParse(filter.Value, out DateTime gtDate))
            {
                return new QueryContainerDescriptor<object>().DateRange(r => r.Field(filter.FieldName).GreaterThan(gtDate));
            }
            else
            {
                throw new ArgumentException($"The value {filter.Value} provided for field {filter.FieldName} is not of a correct type for the provided comparator {filter.Comparator}");
            }
        }
        private static QueryContainer GetQuery(GreaterThanOrEqualComparator c, IFilterField filter)
        {
            if (double.TryParse(filter.Value, out var gtDoub))
            {
                return new QueryContainerDescriptor<object>().Range(r => r.Field(filter.FieldName).GreaterThanOrEquals(gtDoub));
            }
            else if (DateTime.TryParse(filter.Value, out DateTime gtDate))
            {
                return new QueryContainerDescriptor<object>().DateRange(r => r.Field(filter.FieldName).GreaterThanOrEquals(gtDate));
            }
            else
            {
                throw new ArgumentException($"The value {filter.Value} provided for field {filter.FieldName} is not of a correct type for the provided comparator {filter.Comparator}");
            }
        }
        private static QueryContainer GetQuery(LessThanComparator c, IFilterField filter)
        {
            if (double.TryParse(filter.Value, out var gtDoub))
            {
                return new QueryContainerDescriptor<object>().Range(r => r.Field(filter.FieldName).LessThan(gtDoub));
            }
            else if (DateTime.TryParse(filter.Value, out DateTime gtDate))
            {
                return new QueryContainerDescriptor<object>().DateRange(r => r.Field(filter.FieldName).LessThan(gtDate));
            }
            else
            {
                throw new ArgumentException($"The value {filter.Value} provided for field {filter.FieldName} is not of a correct type for the provided comparator {filter.Comparator}");
            }
        }
        private static QueryContainer GetQuery(LessThanOrEqualComparator c, IFilterField filter)
        {
            if (double.TryParse(filter.Value, out var gtDoub))
            {
                return new QueryContainerDescriptor<object>().Range(r => r.Field(filter.FieldName).LessThanOrEquals(gtDoub));
            }
            else if (DateTime.TryParse(filter.Value, out DateTime gtDate))
            {
                return new QueryContainerDescriptor<object>().DateRange(r => r.Field(filter.FieldName).LessThanOrEquals(gtDate));
            }
            else
            {
                throw new ArgumentException($"The value {filter.Value} provided for field {filter.FieldName} is not of a correct type for the provided comparator {filter.Comparator}");
            }
        }
        private static QueryContainer GetQuery(NotAnyWordComparator c, IFilterField filter)
        {
            return new QueryContainerDescriptor<BaseDocument>().Bool(b => b.MustNot(mn => mn.Match(m => m.Field(filter.FieldName).Query(filter.Value))));
        }
        private static QueryContainer GetQuery(NotEqualComparator c, IFilterField filter)
        {
            return new QueryContainerDescriptor<BaseDocument>().Bool(b => b.MustNot(mn => mn.Match(m => m.Field(filter.FieldName).Query(filter.Value))));
        }
        private static QueryContainer GetQuery(PartialPhraseComparator c, IFilterField filter)
        {
            return new QueryContainerDescriptor<BaseDocument>().Bool(b => b.Must(m => m.MatchPhrasePrefix(ma => ma.Field(filter.FieldName).Query(filter.Value))));
        }
    }
}
