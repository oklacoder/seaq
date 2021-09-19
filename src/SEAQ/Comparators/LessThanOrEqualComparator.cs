namespace seaq
{
    public class LessThanOrEqualComparator :
        DefaultComparator
    {
        const string _display = "Less Than Or Equal";
        const string _value = "lte";

        public LessThanOrEqualComparator()
            : base(_display, _value)
        {

        }
    }
}
