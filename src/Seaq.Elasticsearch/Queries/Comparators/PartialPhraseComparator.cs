namespace Seaq.Elasticsearch.Queries.Comparators
{
    public class PartialPhraseComparator : Comparator
    {
        public PartialPhraseComparator() :
            base("fullPhrase", "Match Full Phrase")
        { }
    }
}
