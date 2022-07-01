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
    public class ExistsComparator :
        BaseComparator
    {
        const string _display = "Exists";
        const string _value = "ex";

        public ExistsComparator()
            : base(_display, _value)
        {

        }
    }
}
