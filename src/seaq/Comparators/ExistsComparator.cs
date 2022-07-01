namespace seaq
{
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
