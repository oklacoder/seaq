using Elasticsearch.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Seaq.Elasticsearch.Clusters
{
    public class ClusterSettings
    {
        const bool _forceRefreshOnCommitDefault = true;
        const bool _eagerlyPersistStoreMetaDefault = true;
        
        public string Url { get; }
        public string Username { get; }
        public string Password { get; }
        public string ScopeId { get; }
        /// <summary>
        /// See https://www.elastic.co/guide/en/elasticsearch/reference/current/docs-refresh.html
        /// TL;DR This setting ensures a synchronous write operation,
        /// at the cost of a potentially sizable performance hit to the server
        /// </summary>
        public bool ForceRefreshOnCommit { get; }
        public bool EagerlyCreateIndexMetadataRecord { get; }
        public ISeaqElasticsearchSerializer Serializer { get; }

        public ClusterSettings(
            string url,
            string username,
            string password,
            string scopeId,
            bool? forceRefreshOnCommit = _forceRefreshOnCommitDefault,
            bool? eagerlyPersistStoreMeta = _eagerlyPersistStoreMetaDefault,
            ISeaqElasticsearchSerializer serializer = null)
        {
            Url = url;
            Username = username;
            Password = password;
            ScopeId = scopeId;
            ForceRefreshOnCommit = forceRefreshOnCommit ?? _forceRefreshOnCommitDefault;
            EagerlyCreateIndexMetadataRecord = eagerlyPersistStoreMeta ?? _eagerlyPersistStoreMetaDefault;
            Serializer = serializer;
        }
    }
}
