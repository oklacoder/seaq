using Seaq.Elasticsearch.Clusters;
using Seaq.Elasticsearch.Queries;
using Seaq.Elasticsearch.Queries.Comparators;
using Seaq.Elasticsearch.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Seaq.Elasticsearch.Tests
{
    public class ComparatorTests
    {

        private ClusterSettings settings { get; }


        public ComparatorTests()
        {
            settings = TestDataService.GetClusterSettings(this.GetType().FullName.ToLowerInvariant());
        }

        #region text

        [Fact]
        public void Can_Filter_Query_On_Text_Equals()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.LastName), tester.LastName, Comparator.Equal);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);
            
            var resultsCorrect = results.Results.All(p => (p as Person).LastName == tester.LastName);

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.True(resultsCorrect);
        }

        [Fact]
        public void Can_Filter_Query_On_Text_Not_Equals()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.LastName), tester.LastName, Comparator.NotEqual);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var resultsCorrect = results.Results.All(p => (p as Person).LastName != tester.LastName);

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.True(resultsCorrect);
        }



        #endregion text


        #region numbers
        [Fact]
        public void Can_Filter_Query_On_Number_Equal()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.Age), tester.Age.ToString(), Comparator.Equal);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var resultsContainTester = results.Results.Any(p => p.DocumentId == tester.DocumentId);

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.True(resultsContainTester);
        }

        [Fact]
        public void Can_Filter_Query_On_Number_Greater_Than()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.Age), tester.Age.ToString(), Comparator.GreaterThan);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var expectedList = documents.Where(p => p.Age > tester.Age).ToArray();
            var actualList = results.Results.ToArray();

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.Equal(expectedList.Length, results.Paging.TotalItems);
            Assert.True(actualList.All(p => expectedList.Select(x => x.DocumentId).Contains(p.DocumentId)));
        }

        [Fact]
        public void Can_Filter_Query_On_Number_Greater_Than_Or_Equal()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.Age), tester.Age.ToString(), Comparator.GreaterThanEqual);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var expectedList = documents.Where(p => p.Age >= tester.Age).ToArray();
            var actualList = results.Results.ToArray();

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.Equal(expectedList.Length, results.Paging.TotalItems);
            Assert.True(actualList.All(p => expectedList.Select(x => x.DocumentId).Contains(p.DocumentId)));
        }

        [Fact]
        public void Can_Filter_Query_On_Number_Less_Than()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.Age), tester.Age.ToString(), Comparator.LessThan);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var expectedList = documents.Where(p => p.Age < tester.Age).ToArray();
            var actualList = results.Results.ToArray();

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.Equal(expectedList.Length, results.Paging.TotalItems);
            Assert.True(actualList.All(p => expectedList.Select(x => x.DocumentId).Contains(p.DocumentId)));
        }

        [Fact]
        public void Can_Filter_Query_On_Number_Less_Than_Or_Equal()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.Age), tester.Age.ToString(), Comparator.LessThanEqual);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var expectedList = documents.Where(p => p.Age <= tester.Age).ToArray();
            var actualList = results.Results.ToArray();

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.Equal(expectedList.Length, results.Paging.TotalItems);
            Assert.True(actualList.All(p => expectedList.Select(x => x.DocumentId).Contains(p.DocumentId)));
        }

        [Fact]
        public void Can_Filter_Query_On_Number_Between()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.Age), $"5{WellKnownKeys.Queries.BetweenDelimeter}50", Comparator.Between);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var expectedList = documents.Where(p => p.Age >= 5 && p.Age <= 50).ToArray();
            var actualList = results.Results.ToArray();

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.Equal(expectedList.Length, results.Paging.TotalItems);
            Assert.True(actualList.All(p => expectedList.Select(x => x.DocumentId).Contains(p.DocumentId)));
        }

        #endregion numbers

        #region dates
        [Fact]
        public void Can_Filter_Query_On_Date_Equal()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.Birthday), tester.Birthday.ToString("o"), Comparator.Equal);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var resultsContainTester = results.Results.Any(p => p.DocumentId == tester.DocumentId);

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.True(resultsContainTester);
        }

        [Fact]
        public void Can_Filter_Query_On_Date_Greater_Than()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.Birthday), tester.Birthday.ToString("o"), Comparator.GreaterThan);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var expectedList = documents.Where(p => p.Birthday > tester.Birthday).ToArray();
            var actualList = results.Results.ToArray();

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.Equal(expectedList.Length, results.Paging.TotalItems);
            Assert.True(actualList.All(p => expectedList.Select(x => x.DocumentId).Contains(p.DocumentId)));
        }

        [Fact]
        public void Can_Filter_Query_On_Date_Greater_Than_Or_Equal()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.Birthday), tester.Birthday.ToString("o"), Comparator.GreaterThanEqual);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var expectedList = documents.Where(p => p.Birthday >= tester.Birthday).ToArray();
            var actualList = results.Results.ToArray();

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.Equal(expectedList.Length, results.Paging.TotalItems);
            Assert.True(actualList.All(p => expectedList.Select(x => x.DocumentId).Contains(p.DocumentId)));
        }

        [Fact]
        public void Can_Filter_Query_On_Date_Less_Than()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.Birthday), tester.Birthday.ToString("o"), Comparator.LessThan);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var expectedList = documents.Where(p => p.Birthday < tester.Birthday).ToArray();
            var actualList = results.Results.ToArray();

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.Equal(expectedList.Length, results.Paging.TotalItems);
            Assert.True(actualList.All(p => expectedList.Select(x => x.DocumentId).Contains(p.DocumentId)));
        }

        [Fact]
        public void Can_Filter_Query_On_Date_Less_Than_Or_Equal()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.Birthday), tester.Birthday.ToString("o"), Comparator.LessThanEqual);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var expectedList = documents.Where(p => p.Birthday <= tester.Birthday).ToArray();
            var actualList = results.Results.ToArray();

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.Equal(expectedList.Length, results.Paging.TotalItems);
            Assert.True(actualList.All(p => expectedList.Select(x => x.DocumentId).Contains(p.DocumentId)));
        }

        [Fact]
        public void Can_Filter_Query_On_Date_Between()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.Birthday), $"{tester.Birthday}{WellKnownKeys.Queries.BetweenDelimeter}{DateTime.Now}", Comparator.Between);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var expectedList = documents.Where(p => p.Birthday >= tester.Birthday && p.Birthday <= DateTime.Now).ToArray();
            var actualList = results.Results.ToArray();

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.Equal(expectedList.Length, results.Paging.TotalItems);
            Assert.True(actualList.All(p => expectedList.Select(x => x.DocumentId).Contains(p.DocumentId)));
        }

        #endregion dates


        private Cluster CreateCluster()
        {
            return new Cluster(settings);
        }

        private void Decommission(Cluster cluster)
        {
            var stores = cluster.GetScopedStoreList();

            foreach (var store in stores)
            {
                cluster.DeleteStore(store);
            }
        }

        private (Cluster, Store, Person[]) SpinUp(
            int? countOverride = null)
        {
            var cluster = new Cluster(settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person).FullName);

            var store = cluster.CreateStore(storeSettings);

            var documents = TestDataService.GetFakes(store.StoreId.Name, countOverride ?? 100);

            var commitResult = cluster.Commit(documents);

            return (cluster, store, documents);
        }
    }
}
