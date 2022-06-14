namespace seaq
{
    public class GreaterThanComparator :
        BaseComparator
    {
        const string _display = "Greater Than";
        const string _value = "gt";

        public GreaterThanComparator()
            : base(_display, _value)
        {

        }
    }
}
