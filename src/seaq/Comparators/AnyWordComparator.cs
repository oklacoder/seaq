namespace seaq
{
    public class AnyWordComparator :
        BaseComparator
    {
        const string _display = "Match Any Word";
        const string _value = "anyWord";

        public AnyWordComparator()
            : base(_display, _value)
        {

        }
    }
}
