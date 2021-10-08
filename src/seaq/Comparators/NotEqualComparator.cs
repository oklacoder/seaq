namespace seaq
{
    public class NotEqualComparator :
        DefaultComparator
    {
        const string _display = "Not Equal";
        const string _value = "notEqual";

        public NotEqualComparator()
            : base(_display, _value)
        {

        }
    }
}
