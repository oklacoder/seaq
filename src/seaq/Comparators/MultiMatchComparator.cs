namespace seaq
{
    public class MultiMatchComparator :
        DefaultComparator
    {
        const string _display = "Multi Match";
        const string _value = "multiMatch";

        public MultiMatchComparator()
            : base(_display, _value)
        {

        }
    }
}
