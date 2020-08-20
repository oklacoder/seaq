using Nest;
using Seaq.Elasticsearch.Queries.Comparators;
using System;
using System.Linq;

namespace Seaq.Elasticsearch.Queries
{
    public class ComparatorHelpers
    {
        private readonly DefaultQueryBuilder QueryBuilder = new DefaultQueryBuilder();

        public QueryContainer GetQuery<T>(
            AnyWordComparator comparator,
            QueryFilter filter)
            where T : class
        {
            return new QueryContainerDescriptor<T>().Bool(b => b.Must(m => QueryBuilder.GetMatchQueryForFilter<T>(filter)));
        }

        public QueryContainer GetQuery<T>(
            BetweenComparator comparator,
            QueryFilter filter)
            where T : class
        {
            var vals = filter.Value.Split(new string[] { "|||" }, StringSplitOptions.None);
            var start = vals.FirstOrDefault();
            var end = vals.LastOrDefault();
            var startIsNum = Double.TryParse(start, out Double startNum);
            var endIsNum = Double.TryParse(end, out Double endNum);
            var startIsDate = DateTime.TryParse(start, out DateTime startDate);
            var endIsDate = DateTime.TryParse(end, out DateTime endDate);

            if (startIsNum && endIsNum)
            {
                return new QueryContainerDescriptor<object>().Range(rng => rng.Field(filter.Field).GreaterThanOrEquals(startNum).LessThanOrEquals(endNum));
            }
            else if (startIsDate && endIsDate)
            {
                return new QueryContainerDescriptor<object>().DateRange(rng => rng.Field(filter.Field).GreaterThanOrEquals(startDate).LessThanOrEquals(endDate));
            }
            else
            {
                throw new ArgumentException($"The value {filter.Value} provided for field {filter.Field} is not of a correct type for the provided comparator {filter.Comparator}");
            }
        }

        public QueryContainer GetQuery<T>(
            EqualComparator comparator,
            QueryFilter filter)
            where T : class
        {
            return new QueryContainerDescriptor<object>().Bool(b => b.Must(mn => QueryBuilder.GetMatchQueryForFilter<T>(filter)));
        }

        public QueryContainer GetQuery<T>(
            FullPhraseComparator comparator,
            QueryFilter filter)
            where T : class
        {
            return new QueryContainerDescriptor<object>().Bool(b => b.Must(m => QueryBuilder.GetMatchPhraseQueryForFilter<T>(filter)));
        }

        public QueryContainer GetQuery<T>(
            GreaterThanComparator comparator,
            QueryFilter filter)
            where T : class
        {
            if (double.TryParse(filter.Value, out var gtDoub))
            {
                return new QueryContainerDescriptor<object>().Range(r => r.Field(filter.Field).GreaterThan(gtDoub));
            }
            else if (DateTime.TryParse(filter.Value, out DateTime gtDate))
            {
                return new QueryContainerDescriptor<object>().DateRange(r => r.Field(filter.Field).GreaterThan(gtDate));
            }
            else
            {
                throw new ArgumentException($"The value {filter.Value} provided for field {filter.Field} is not of a correct type for the provided comparator {filter.Comparator}");
            }
        }

        public QueryContainer GetQuery<T>(
            GreaterThanOrEqualComparator comparator,
            QueryFilter filter)
            where T : class
        {
            if (double.TryParse(filter.Value, out var gtDoub))
            {
                return new QueryContainerDescriptor<object>().Range(r => r.Field(filter.Field).GreaterThanOrEquals(gtDoub));
            }
            else if (DateTime.TryParse(filter.Value, out DateTime gtDate))
            {
                return new QueryContainerDescriptor<object>().DateRange(r => r.Field(filter.Field).GreaterThanOrEquals(gtDate));
            }
            else
            {
                throw new ArgumentException($"The value {filter.Value} provided for field {filter.Field} is not of a correct type for the provided comparator {filter.Comparator}");
            }
        }

        public QueryContainer GetQuery<T>(
            LessThanComparator comparator,
            QueryFilter filter)
            where T : class
        {
            if (double.TryParse(filter.Value, out var gtDoub))
            {
                return new QueryContainerDescriptor<object>().Range(r => r.Field(filter.Field).LessThan(gtDoub));
            }
            else if (DateTime.TryParse(filter.Value, out DateTime gtDate))
            {
                return new QueryContainerDescriptor<object>().DateRange(r => r.Field(filter.Field).LessThan(gtDate));
            }
            else
            {
                throw new ArgumentException($"The value {filter.Value} provided for field {filter.Field} is not of a correct type for the provided comparator {filter.Comparator}");
            }
        }

        public QueryContainer GetQuery<T>(
            LessThanOrEqualComparator comparator,
            QueryFilter filter)
            where T : class
        {
            if (double.TryParse(filter.Value, out var gtDoub))
            {
                return new QueryContainerDescriptor<object>().Range(r => r.Field(filter.Field).LessThanOrEquals(gtDoub));
            }
            else if (DateTime.TryParse(filter.Value, out DateTime gtDate))
            {
                return new QueryContainerDescriptor<object>().DateRange(r => r.Field(filter.Field).LessThanOrEquals(gtDate));
            }
            else
            {
                throw new ArgumentException($"The value {filter.Value} provided for field {filter.Field} is not of a correct type for the provided comparator {filter.Comparator}");
            }
        }

        public QueryContainer GetQuery<T>(
            NotAnyWordComparator comparator,
            QueryFilter filter)
            where T : class
        {
            return new QueryContainerDescriptor<object>().Bool(b => b.MustNot(m => QueryBuilder.GetMatchQueryForFilter<T>(filter)));
        }

        public QueryContainer GetQuery<T>(
            NotEqualComparator comparator,
            QueryFilter filter)
            where T : class
        {
            return new QueryContainerDescriptor<object>().Bool(b => b.MustNot(mn => QueryBuilder.GetMatchQueryForFilter<T>(filter)));
        }

        public QueryContainer GetQuery<T>(
            PartialPhraseComparator comparator,
            QueryFilter filter)
            where T : class
        {
            return new QueryContainerDescriptor<object>().Bool(b => b.Must(m => QueryBuilder.GetMatchPhrasePrefixQueryForFilter<T>(filter)));
        }

        public QueryContainer GetQuery<T>(
            Comparator comparator,
            QueryFilter filter)
            where T : class
        {
            throw new NotImplementedException($"GetQuery method not implemented for comparator {comparator.GetType().FullName}");
        }
    }
}
