namespace seaq
{
    public class AnyWordComparator :
        DefaultComparator
    {
        const string _display = "Match Any Word";
        const string _value = "anyWord";

        public AnyWordComparator()
            : base(_display, _value)
        {

        }
    }
}
