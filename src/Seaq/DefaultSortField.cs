namespace Seaq
{
    public class DefaultSortField :
        ISortField
    {
        public string FieldName { get; }

        public bool SortAscending { get; }

        public int Ordinal { get; }

        public DefaultSortField(
            string fieldName,
            bool sortAscending, 
            int ordinal)
        {
            FieldName = fieldName;
            SortAscending = sortAscending;
            Ordinal = ordinal;
        }
    }
}
