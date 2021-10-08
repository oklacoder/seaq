namespace seaq
{
    public class BetweenComparator :
        DefaultComparator
    {
        const string _display = "Between";
        const string _value = "between";

        public BetweenComparator()
            : base(_display, _value)
        {

        }
    }
}
