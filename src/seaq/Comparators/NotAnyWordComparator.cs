namespace seaq
{
    public class NotAnyWordComparator :
        DefaultComparator
    {
        const string _display = "Not Any Words";
        const string _value = "notAnyWord";

        public NotAnyWordComparator()
            : base(_display, _value)
        {

        }
    }
}
