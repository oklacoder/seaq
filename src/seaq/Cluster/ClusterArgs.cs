﻿namespace seaq
{
    public class ClusterArgs
    {
        public string ClusterScope { get; }
        public string Url { get; }
        public string Username { get; }
        public string Password { get; }
        public bool BypassCertificateValidation { get; }
        public ISeaqElasticsearchSerializer Serializer { get; } = null;
        public bool AllowAutomaticIndexCreation { get; }
        public bool EnableVersionCompatabilityHeader { get; }
        public IAggregationCache AggregationCache { get; }

        public ClusterArgs(
            string clusterScope,
            string url,
            string username,
            string password,
            bool bypassCertificateValidation = false,
            ISeaqElasticsearchSerializer serializer = null,
            bool allowAutomaticIndexCreation = true, 
            bool enableVersionCompatabilityHeader = true,
            IAggregationCache aggregationCache = null)
        {
            ClusterScope = clusterScope;
            Url = url;
            Username = username;
            Password = password;
            BypassCertificateValidation = bypassCertificateValidation;
            Serializer = serializer;
            AllowAutomaticIndexCreation = allowAutomaticIndexCreation;
            EnableVersionCompatabilityHeader = enableVersionCompatabilityHeader;
            aggregationCache = aggregationCache;
        }
    }
}
