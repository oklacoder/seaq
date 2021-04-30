using Seaq.Clusters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Seaq.Tests
{
    public class ClusterTests
    {
        [Fact]
        public void CanCreateCluster()
        {
            var cluster = TestUtil.Cluster;
            var scope = cluster.ScopeId;

            Assert.NotNull(cluster);
            Assert.NotNull(scope);            
        }

        [Fact]
        public void CanPingCluster()
        {
            var canPing = TestUtil.Cluster.CanPing();

            Assert.True(canPing);
        }

        [Fact]
        public void CreatingTwoClustersFromTestUtilCreatesDifferentScopes()
        {
            var cluster1 = TestUtil.Cluster;
            var cluster2 = TestUtil.Cluster;

            Assert.NotEqual(cluster1.ScopeId, cluster2.ScopeId);
        }

        [Fact]
        public void CanCreateCollection()
        {
            var cluster = TestUtil.Cluster;

            var config = TestUtil.CollectionConfig;

            cluster.TryAddCollection(config, out var collection);

            cluster.TryDeleteCollection(collection.CollectionName, true);
            Assert.NotEmpty(cluster.Collections);
        }

        [Fact]
        public void CanGetCollectionSchema()
        {
            var cluster = TestUtil.Cluster;

            var config = TestUtil.CollectionConfig;

            cluster.TryAddCollection(config, out var collection);

            var schema = collection.Schema;
            var f = new CollectionField("Test Field", "Test Type", null, null, null, null);
            schema.AddField(f);

            var canUpdate = cluster.TryUpdateCollectionSchema(collection.CollectionName, schema, out var msgs);


            cluster.TryDeleteCollection(collection.CollectionName, true);
            Assert.True(canUpdate);
            Assert.True(cluster.Collections.FirstOrDefault().Schema.Fields.Any(x => x.Name.Equals(f.Name, StringComparison.OrdinalIgnoreCase)));

            Assert.NotNull(schema);
        }

        [Fact]
        public void CanUpdateCollectionSchema()
        {
            var cluster = TestUtil.Cluster;

            var config = TestUtil.CollectionConfig;

            cluster.TryAddCollection(config, out var collection);

            var schema = collection.Schema;
            var f = new CollectionField("Test Field", "Test Type", null, null, null, null);
            schema.AddField(f);

            var canUpdate = cluster.TryUpdateCollectionSchema(collection.CollectionName, schema, out var msgs);


            cluster.TryDeleteCollection(collection.CollectionName, true);
            Assert.True(canUpdate);
            Assert.True(cluster.Collections.FirstOrDefault().Schema.Fields.Any(x => x.Name.Equals(f.Name, StringComparison.OrdinalIgnoreCase)));

            Assert.NotNull(schema);
        }

        [Fact]
        public void CanDeleteCollectionLocally()
        {
            var cluster = TestUtil.Cluster;

            var config = TestUtil.CollectionConfig;

            cluster.TryAddCollection(config, out var collection);

            cluster.TryDeleteCollection(collection.CollectionName);

            cluster.TryDeleteCollection(collection.CollectionName, true);
            Assert.Empty(cluster.Collections);
        }

        [Fact]
        public void LocalCollectionDeletionLeavesServerCollection()
        {
            var cluster = TestUtil.Cluster;

            var config = TestUtil.CollectionConfig;

            cluster.TryAddCollection(config, out var collection);

            cluster.TryDeleteCollection(collection.CollectionName);

            Assert.Empty(cluster.Collections);

            cluster = TestUtil.BuildCluster(cluster.ScopeId);

            Assert.NotEmpty(cluster.Collections);
            cluster.TryDeleteCollection(collection.CollectionName, true);
        }

        [Fact]
        public void CanDeleteCollectionOnServer()
        {
            var cluster = TestUtil.Cluster;

            var config = TestUtil.CollectionConfig;

            cluster.TryAddCollection(config, out var collection);

            cluster.TryDeleteCollection(collection.CollectionName, true);

            cluster = TestUtil.BuildCluster(cluster.ScopeId);

            Assert.Empty(cluster.Collections);
        }

        [Fact]
        public void ClusterCorrectlyBootstrapsCollection()
        {
            var cluster = TestUtil.Cluster;
            var scope = cluster.ScopeId;
            var config = TestUtil.CollectionConfig;

            cluster.TryAddCollection(config, out var collection);


            cluster = TestUtil.BuildCluster(scope);
            cluster.TryDeleteCollection(collection.CollectionName, true);

            Assert.NotEmpty(cluster.Collections);
            Assert.NotEmpty(cluster.Collections);
        }

        [Fact]
        public void CanCommitSingleDocument()
        {
            var cluster = TestUtil.Cluster;
            var collectionConfig = TestUtil.CollectionConfig;
            cluster.TryAddCollection(collectionConfig, out var collection);

            var doc = new SampleResult 
            {
                OrderId = 123,
                CollectionId = collection.CollectionName,
                CustomerFirstName = "Matt",
                CustomerLastName = "Stovall",
                Products = new[]
                {
                    new SampleProduct
                    {
                        Id = "3",
                        ProductName = "Sample",
                        Quantity = 1
                    }
                }
            };

            var result = cluster.TryCommit(doc);

            var q = new SimpleQuery<SampleResult>(new SimpleQueryCriteria<SampleResult>("matt", new[] { collection.CollectionName }));
            var res = cluster.Query(q);

            cluster.TryDeleteCollection(collection.CollectionName, true);
            Assert.True(result);
            Assert.NotEmpty(res.Documents);
        }

        [Fact]
        public void CanCommitDocumentBatch()
        {
            var cluster = TestUtil.Cluster;
            var collectionConfig = TestUtil.CollectionConfig;
            cluster.TryAddCollection(collectionConfig, out var collection);

            var docs = new []
            {
                new SampleResult
                {
                    OrderId = 123,
                    CollectionId = collection.CollectionName,
                    CustomerFirstName = "Matt",
                    CustomerLastName = "Stovall",
                    Products = new[]
                    {
                        new SampleProduct
                        {
                            Id = "3",
                            ProductName = "Sample",
                            Quantity = 1
                        }
                    }
                },
                new SampleResult
                {
                    OrderId = 124,
                    CollectionId = collection.CollectionName,
                    CustomerFirstName = "Matt",
                    CustomerLastName = "Stovall",
                    Products = new[]
                    {
                        new SampleProduct
                        {
                            Id = "3",
                            ProductName = "Sample",
                            Quantity = 1
                        }
                    }
                },
                    new SampleResult
                {
                    OrderId = 125,
                    CollectionId = collection.CollectionName,
                    CustomerFirstName = "Matt",
                    CustomerLastName = "Stovall",
                    Products = new[]
                    {
                        new SampleProduct
                        {
                            Id = "3",
                            ProductName = "Sample",
                            Quantity = 1
                        }
                    }
                }
            };

            var result = cluster.TryCommit(docs, out _);

            Assert.True(result);

            var q = new SimpleQuery<SampleResult>(new SimpleQueryCriteria<SampleResult>("matt", new[] { collection.CollectionName }));
            var res = cluster.Query(q);

            cluster.TryDeleteCollection(collection.CollectionName, true);
            Assert.NotEmpty(res.Documents);
            Assert.Equal(3, res.Documents.Count());
        }

        [Fact]
        public void CanDeleteDocuments()
        {
            var cluster = TestUtil.Cluster;
            var collectionConfig = TestUtil.CollectionConfig;
            cluster.TryAddCollection(collectionConfig, out var collection);

            var doc = new SampleResult
            {
                OrderId = 123,
                CollectionId = collection.CollectionName,
                CustomerFirstName = "Matt",
                CustomerLastName = "Stovall",
                Products = new[]
                {
                    new SampleProduct
                    {
                        Id = "3",
                        ProductName = "Sample",
                        Quantity = 1
                    }
                }
            };

            var result = cluster.TryCommit(doc);

            Assert.True(result);

            var q = new SimpleQuery<SampleResult>(new SimpleQueryCriteria<SampleResult>("matt", new[] { collection.CollectionName }));
            var res = cluster.Query(q);

            Assert.NotEmpty(res.Documents);

            var deleteRes = cluster.TryDelete<SampleResult>(collection.CollectionName, out var deleteErrors, doc.Id);

            
            res = cluster.Query(q);

            cluster.TryDeleteCollection(collection.CollectionName, true);
            Assert.Empty(deleteErrors);
            Assert.True(deleteRes);
            Assert.Empty(res.Documents);
        }


    }
}
