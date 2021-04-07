using System;
using Elasticsearch.Net;
using Nest;
using Serilog;

namespace Seaq.Clusters
{
    public static class ClusterExtensions
    {
        public static ConnectionSettings GetConnectionSettings(this Cluster cluster, IClusterConnection connection)
        {
            var url = connection.ClusterUrl;
            var username = connection.Username;
            var password = connection.Password;
            var serializer = connection.Serializer;

            var uri = new Uri(url);

            var connectionPool = new SingleNodeConnectionPool(uri);

            var connectionSettings =
                serializer == null ?
                new ConnectionSettings(connectionPool) :
                new ConnectionSettings(
                    connectionPool,
                    sourceSerializer: (builtin, settings) => serializer
                    );

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                connectionSettings.BasicAuthentication(username, password);
                ///this is the great big hammer to break out when things aren't working - 
                ///it bypasses certificate validation entirely, which is necessary for local self-signed certs,
                ///but is a GIANT security risk otherwise.  Only used for debugging for a reason.
#if DEBUG
                Log.Debug("DEBUG mode enabled.  Ignoring server certificate validation and enabling additional Elasticsearch debug messages.");
                connectionSettings.ServerCertificateValidationCallback((a, b, c, d) => true);
                connectionSettings.EnableDebugMode();
#endif
            }
            return connectionSettings;
        }    
            
    }

}
