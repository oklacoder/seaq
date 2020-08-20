using System;
using System.Collections.Generic;
using System.Text;

namespace Seaq.Elasticsearch.Queries.Comparators
{
    public class Comparator
    {
        public string Value { get; }
        public string Display { get; }


        public Comparator(string value, string display)
        {
            Value = value;
            Display = display;
        }

        public static FullPhraseComparator FullPhrase => new FullPhraseComparator();
        public static PartialPhraseComparator PartialPhrase => new PartialPhraseComparator();
        public static AnyWordComparator AnyWord => new AnyWordComparator();
        public static NotAnyWordComparator NotAnyWord => new NotAnyWordComparator();
        public static GreaterThanComparator GreaterThan => new GreaterThanComparator();
        public static GreaterThanOrEqualComparator GreaterThanEqual => new GreaterThanOrEqualComparator();
        public static LessThanComparator LessThan => new LessThanComparator();
        public static LessThanOrEqualComparator LessThanEqual => new LessThanOrEqualComparator();
        public static NotEqualComparator NotEqual => new NotEqualComparator();
        public static EqualComparator Equal => new EqualComparator();
        public static BetweenComparator Between => new BetweenComparator();

        public static IEnumerable<Comparator> All = new Comparator[]
        {
            FullPhrase,
            PartialPhrase,
            AnyWord,
            NotAnyWord,
            GreaterThan,
            GreaterThanEqual,
            LessThan,
            LessThanEqual,
            NotEqual,
            Equal,
            Between
        };
    }
}
