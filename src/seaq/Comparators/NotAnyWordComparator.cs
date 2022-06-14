namespace seaq
{
    public class NotAnyWordComparator :
        BaseComparator
    {
        const string _display = "Not Any Words";
        const string _value = "notAnyWord";

        public NotAnyWordComparator()
            : base(_display, _value)
        {

        }
    }
}
