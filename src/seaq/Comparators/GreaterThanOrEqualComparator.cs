namespace seaq
{
    public class GreaterThanOrEqualComparator :
        BaseComparator
    {
        const string _display = "Greater Than Or Equal";
        const string _value = "gte";

        public GreaterThanOrEqualComparator()
            : base(_display, _value)
        {

        }
    }
}
