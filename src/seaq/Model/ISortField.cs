namespace seaq
{
    public interface ISortField
    {
        string FieldName { get; }
        bool SortAscending { get; }
        int Ordinal { get; }
    }
}
