namespace seaq
{
    public class EqualComparator :
        DefaultComparator
    {
        const string _display = "Equal";
        const string _value = "eq";

        public EqualComparator()
            : base(_display, _value)
        {

        }
    }
}
