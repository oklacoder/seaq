namespace seaq
{
    public interface IDocument
    {
        string Id { get; }
        string IndexName { get; }
        string? IndexAsType { get; }
        string Type { get; }
    }
}
