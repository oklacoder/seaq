namespace seaq
{
    public class PartialPhraseComparator :
        DefaultComparator
    {
        const string _display = "Partial Phrase";
        const string _value = "partialPhrase";

        public PartialPhraseComparator()
            : base(_display, _value)
        {

        }
    }
}
