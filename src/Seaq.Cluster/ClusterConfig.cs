namespace Seaq.Clusters{
    public class ClusterConfig 
    {
        public ClusterConnection Connection;

        public string ScopeId { get; }

        public ClusterConfig(
            string scopeId,
            ClusterConnection connection)
        {
            ScopeId = scopeId;
            Connection = connection;
        }
    }

}
