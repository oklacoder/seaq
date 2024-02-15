using seaq;
using System;
using System.Collections.Generic;
using System.Linq;

public static class CriteriaExtensions
{
    private static (IEnumerable<string> indices, IEnumerable<string> deprecatedIndices) GetIndicesForCriteria(
        ICluster cluster,
        string typeName,
        IEnumerable<string> criteriaIndices)
    {
        IEnumerable<string> indices = new List<string>();
        IEnumerable<string> deprecatedIndices = new List<string>();
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return (indices, deprecatedIndices);
        }

        if (criteriaIndices?.Any() is true)
        {
            if (criteriaIndices.Any(z => cluster.DeprecatedIndices.Any(x => x.Name.Equals(z, StringComparison.OrdinalIgnoreCase))))
            {
                var deps = cluster.DeprecatedIndices.Where(x => criteriaIndices.Any(z => z.Equals(x.Name, StringComparison.OrdinalIgnoreCase)));

                deprecatedIndices = deps.Select(x => $"{x.Name} is deprecated - {x.DeprecationMessage}");
            }
            indices = indices.Concat(criteriaIndices);
            return (indices, deprecatedIndices);
        }
        IEnumerable<seaq.Index> idx;
        if (string.IsNullOrWhiteSpace(typeName))
        {
            idx = cluster.Indices.Where(x =>
                x.IsHidden is not true &&
                x.ReturnInGlobalSearch is true);
        }
        else
        {
            idx = cluster.IndicesByType[typeName];

            var hasChecked = new HashSet<string>();

            Func<string, IEnumerable<seaq.Index>> recurseCheckIndices = null;
            recurseCheckIndices = (string type) =>
            {
                if (hasChecked.Contains(type))
                {
                    return Array.Empty<seaq.Index>();
                }
                hasChecked.Add(type);
                if (cluster.IndicesByType.Contains(type))
                {
                    var i = cluster.IndicesByType[type];
                    if (i.Any(x => x.IsHidden is not true))
                    {
                        return i;
                    }
                    return i.Select(x => x.IndexAsType).SelectMany(recurseCheckIndices);
                }
                else
                {
                    return Array.Empty<seaq.Index>();
                }
            };
            if (idx?.Any() is not true)
            foreach(var i in idx)
            {
                if (!string.IsNullOrWhiteSpace(i.IndexAsType))
                    idx = idx.Concat(recurseCheckIndices(i.IndexAsType));
                //if (!string.IsNullOrWhiteSpace(i.IndexAsType))
                //{
                //    idx = idx.Concat(cluster.IndicesByType[i.IndexAsType]);
                //}
            }
            deprecatedIndices = idx.Where(x => x.IsDeprecated).Select(x => $"{x.Name} is deprecated - {x.DeprecationMessage}");
            idx = idx.Where(x => x.IsHidden is not true);
        }

        indices = idx.Select(x => x.Name).ToArray();
        if (indices?.Any() is not true)
        {
            throw new InvalidOperationException($"No indices could be identified for type {typeName}.  Query could not be processed.  " +
                $"Ensure that either an index exists on your seaq cluster fo this type, or that you have specified an explicit type in your query definition.");
        }


        return (indices, deprecatedIndices);
    }
    public static ISeaqQueryCriteria ApplyClusterIndices(
        this ISeaqQueryCriteria criteria,
        ICluster cluster)
    {
        (criteria.Indices, criteria.DeprecatedIndexTargets) = GetIndicesForCriteria(cluster, criteria.Type, criteria.Indices);

        return criteria;
    }
    public static ISeaqQueryCriteria<T> ApplyClusterIndices<T>(
        this ISeaqQueryCriteria<T> criteria,
        ICluster cluster)
        where T : BaseDocument
    {
        (criteria.Indices, criteria.DeprecatedIndexTargets) = GetIndicesForCriteria(cluster, typeof(T).FullName, criteria.Indices);

        return criteria;
    }
}