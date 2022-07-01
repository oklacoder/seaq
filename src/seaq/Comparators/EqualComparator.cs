namespace seaq
{
    public class EqualComparator :
        BaseComparator
    {
        const string _display = "Equal";
        const string _value = "eq";

        public EqualComparator()
            : base(_display, _value)
        {

        }
    }
}
