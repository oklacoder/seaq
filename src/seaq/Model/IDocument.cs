namespace seaq
{
    public interface IDocument
    {
        string Id { get; }
        string IndexName { get; }
        string Type { get; }
    }
}
