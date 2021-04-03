namespace Seaq.Clusters{
    public interface IClusterConfig
    {
        IClusterConnection Connection { get; }
        string ScopeId { get; }
    }

}
