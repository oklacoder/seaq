namespace seaq
{
    public class MultiMatchComparator :
        BaseComparator
    {
        const string _display = "Multi Match";
        const string _value = "multiMatch";

        public MultiMatchComparator()
            : base(_display, _value)
        {

        }
    }

    public class ORComparator : 
        BaseComparator
    {
        const string _display = "OR";
        const string _value = "or";

        public ORComparator()
            : base(_display, _value)
        {                
        }
    }
}
