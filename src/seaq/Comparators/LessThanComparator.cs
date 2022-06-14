namespace seaq
{
    public class LessThanComparator :
        BaseComparator
    {
        const string _display = "Less Than";
        const string _value = "lt";

        public LessThanComparator()
            : base(_display, _value)
        {

        }
    }
}
