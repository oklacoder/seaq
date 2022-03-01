namespace seaq
{
    public class DefaultFilterField :
       IFilterField
    {
        public string FieldName { get; set; }

        public string Value { get; set; }

        public IComparator Comparator { get; set; }

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
