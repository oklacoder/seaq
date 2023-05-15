namespace seaq
{
    public interface IFilterField
    {
        public string FieldName { get; }
        public string Value { get; }
        public IComparator Comparator { get; }
    }
}
