namespace Seaq.Elasticsearch.Queries.Comparators
{
    public class FullPhraseComparator : Comparator
    {
        public FullPhraseComparator()
            : base("fullPhrase", "Match Full Phrase") { }
    }
}
