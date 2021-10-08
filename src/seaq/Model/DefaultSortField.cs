namespace seaq
{
    public class DefaultSortField :
        ISortField
    {
        public string FieldName { get; set; }
        public int Ordinal { get; set; }
        public bool SortAscending { get; set; }

        public DefaultSortField()
        {

        }
        public DefaultSortField(
            string fieldName,
            int ordinal,
            bool sortAscending = true)
        {
            FieldName = fieldName;
            Ordinal = ordinal;
            SortAscending = sortAscending;
        }
    }
}
