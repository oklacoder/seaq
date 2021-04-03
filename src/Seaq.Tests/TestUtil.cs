using Seaq.Clusters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Seaq.Tests
{
    internal static class TestUtil
    {

        const string SampleIndex = "kibana_sample_data_ecommerce";
        const string ClusterUrl = "http://localhost:9200";
        const string ClusterUsername = null;
        const string ClusterPassword = null;
        internal static Nest.ElasticClient Client => new Nest.ElasticClient(new Uri(ClusterUrl));
        internal static string[] SampleIndices => new[] { SampleIndex };

        internal static ClusterConnection ClusterConnection => new ClusterConnection(ClusterUrl, ClusterUsername, ClusterPassword);
        internal static ClusterConfig ClusterConfig => new ClusterConfig(Guid.NewGuid().ToString("N"), ClusterConnection);
        internal static Cluster Cluster => new Cluster(ClusterConfig);

        internal static CollectionConfig CollectionConfig => new CollectionConfig("test_collection", typeof(SampleResult), forceRefreshOnDocumentCommit: true);
        internal static Collection Collection => new Collection(CollectionConfig);

        internal static Cluster BuildCluster(string scopeId)
        {
            var config = new ClusterConfig(scopeId, ClusterConnection);
            return new Cluster(config);
        }

        internal static IEnumerable<string> GetObjectFields(object obj)
        {
            return obj.GetType().GetProperties().Where(x => !Constants.Fields.AlwaysReturnedFields.Contains(x.Name)).Select(x => x.Name);
        }
        internal static bool DoAllTheseFieldsHaveValues(object obj, params string[] fields)
        {
            var res = true;
            var props = obj.GetType().GetProperties().Where(x => !Constants.Fields.AlwaysReturnedFields.Contains(x.Name));

            foreach (var p in props.Where(x => 
                fields.Any(z => 
                    z.Equals(x.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var val = p.GetValue(obj)?.ToString();
                res = res && !p.IsPropertyEmpty(val);
            }

            return res;
        }
        internal static bool DoAllTheseFieldsHaveNoValues(object obj, params string[] fields)
        {
            var res = true;
            var props = obj.GetType().GetProperties().Where(x => !Constants.Fields.AlwaysReturnedFields.Contains(x.Name));

            foreach (var p in props.Where(x =>
                 fields.Any(z =>
                     z.Equals(x.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var val = p.GetValue(obj)?.ToString();
                res = res && p.IsPropertyEmpty(val);
            }

            return res;
        }
        internal static bool DoAllButTheseFieldsHaveValues(object obj, params string[] fields)
        {
            var res = true;
            var props = obj.GetType().GetProperties().Where(x => !Constants.Fields.AlwaysReturnedFields.Contains(x.Name));

            foreach (var p in props.Where(x =>
                 fields.Any(z =>
                     !z.Equals(x.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var val = p.GetValue(obj)?.ToString();
                res = res && p.IsPropertyEmpty(val);
            }
            foreach (var p in props.Where(x =>
                 fields.Any(z =>
                     z.Equals(x.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var val = p.GetValue(obj)?.ToString();
                res = res && p.IsPropertyEmpty(val);
            }

            return res;
        }
        internal static bool DoOnlyTheseFieldsHaveValues(object obj, params string[] fields)
        {
            var res = true;
            var props = obj.GetType().GetProperties().Where(x => !Constants.Fields.AlwaysReturnedFields.Contains(x.Name));

            foreach (var p in props.Where(x =>
                 fields.Any(z =>
                     !z.Equals(x.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var val = p.GetValue(obj)?.ToString();
                res = res && p.IsPropertyEmpty(val);                
            }
            foreach (var p in props.Where(x =>
                 fields.Any(z =>
                     z.Equals(x.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var val = p.GetValue(obj)?.ToString();
                res = res && !p.IsPropertyEmpty(val);
            }

            return res;
        }

        private static bool IsPropertyEmpty(this PropertyInfo p, string val)
        {
            var res = true;
            if (val == null)
                return true;

            var t = p.PropertyType;

            if (t == typeof(DateTime))
            {
                res = DateTime.Parse(val) == DateTime.MinValue;
            }
            else if (t == typeof(DateTime?))
            {
                res = DateTime.Parse(val) == DateTime.MinValue;
            }
            else
            {
                res = val == null;
            }

            return res;
        }
    }
}
