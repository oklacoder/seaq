namespace seaq
{
    public class FullPhraseComparator :
        DefaultComparator
    {
        const string _display = "Match Full Phrase";
        const string _value = "full";

        public FullPhraseComparator()
            : base(_display, _value)
        {

        }
    }
}
