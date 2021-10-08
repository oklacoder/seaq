namespace seaq
{
    public class DefaultFilterField :
       IFilterField
    {
        public string FieldName { get; }

        public string Value { get; }

        public IComparator Comparator { get; }

        public DefaultFilterField()
        {

        }
        public DefaultFilterField(
            IComparator comparator,
            string value,
            string fieldName)
        {
            Comparator = comparator;
            Value = value;
            FieldName = fieldName;
        }
    }
}
