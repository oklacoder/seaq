using Seaq.Elasticsearch.Clusters;
using Seaq.Elasticsearch.Documents;
using Seaq.Elasticsearch.Queries;
using Seaq.Elasticsearch.Queries.Comparators;
using Seaq.Elasticsearch.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;



namespace Seaq.Elasticsearch.Tests
{
    public class QueryTests
    {
        private ClusterSettings settings { get; }
        private readonly ITestOutputHelper output;

        public QueryTests(ITestOutputHelper output)
        {
            settings = TestDataService.GetClusterSettings(this.GetType().FullName.ToLowerInvariant());
            this.output = output;
        }


        [Fact]
        public void Can_Filter_Query()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.LastName), tester.LastName, Comparator.Equal);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var resultsContainTester = results.Results.Any(p => p.DocumentId == tester.DocumentId);

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.True(resultsContainTester);
        }

        [Fact] 
        public void Can_Field_Values_Query()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var criteria = new FieldValuesQueryCriteria(new[] { store.StoreId.Name }, "lastName", tester.LastName);
            var query = new FieldValuesQuery(criteria);

            var result = cluster.Query(query) as FieldValuesQueryResult;

            var buckets = result.Buckets;

            var expectedCount = documents.Where(x => x.LastName == tester.LastName).Count();
            var actualCount = buckets.FirstOrDefault(x => x.Key.Contains(nameof(Person.LastName)))?.Values?.FirstOrDefault(x => x?.Key.Equals(tester.LastName, StringComparison.OrdinalIgnoreCase) == true)?.Count;

            Decommission(cluster);
            Assert.True(buckets.Count == 1);
            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public void Can_Field_Values_Query_With_Null_QueryText()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var criteria = new FieldValuesQueryCriteria(new[] { store.StoreId.Name }, "lastName", null);
            var query = new FieldValuesQuery(criteria);

            var result = cluster.Query(query) as FieldValuesQueryResult;

            var buckets = result.Buckets;

            Decommission(cluster);
            Assert.NotEmpty(buckets);
        }
        [Fact]
        public void Can_Field_Values_Query_For_Aggregate_Fields()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var criteria = new FieldValuesQueryCriteria(new[] { store.StoreId.Name }, $"lastName.{WellKnownKeys.Fields.KeywordField}", null);
            var query = new FieldValuesQuery(criteria);

            var result = cluster.Query(query) as FieldValuesQueryResult;

            var buckets = result.Buckets;

            Decommission(cluster);
            Assert.NotEmpty(buckets);
        }
        
        [Fact]
        public void Filter_Query_Only_Returns_Suggestion_Bucket_For_Specified_Filter_Fields()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];
            
            var filter = new QueryFilter(nameof(tester.LastName), tester.LastName, Comparator.Equal);

            var filters = new QueryFilter[] { filter };

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, filters, restrictBucketsToInputFilters: true);

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var buckets = (results as FilteredQueryResult)?.Buckets;

            var bucketCount = buckets.Count;
            var filterCount = filters.Length;

            
            Decommission(cluster);

            Assert.Equal(filterCount, bucketCount);
        }
        
        [Fact]
        public void Sorted_Filter_Query_Returns_Correctly()
        {
            var (cluster, store, documents) = SpinUp(50);

            var tester = documents[5];

            var fieldNameUtilities = new DefaultFieldNameUtilities();

            var filter = new QueryFilter($"{fieldNameUtilities.GetElasticSortPropertyName(tester.GetType(), nameof(tester.LastName))}", tester.LastName, Comparator.Equal, new Sort(true, true));

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var results = cluster.Query(query);

            var resultsContainTester = results.Results.Any(p => p.DocumentId == tester.DocumentId);

            var expectedTop = documents.Where(p => p.LastName.ToLowerInvariant() == tester.LastName.ToLowerInvariant()).OrderBy(p => p.LastName).FirstOrDefault();
            var actualTop = results.Results.FirstOrDefault();

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.Equal(expectedTop.DocumentId, actualTop.DocumentId);
        }

        [Fact]
        public void Can_Simple_Query()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var criteria = new SimpleQueryCriteria(new[] { store.StoreId.Name }, tester.LastName);

            var query = new SimpleQuery(criteria);

            var results = cluster.Query(query);

            var resultsContainTester = results.Results.Any(p => p.DocumentId == tester.DocumentId);

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.True(resultsContainTester);
        }

        [Fact]
        public void Can_Suggestion_Query()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var criteria = new SuggestionQueryCriteria(new[] { store.StoreId.Name }, tester.LastName);

            var query = new SuggestionQuery(criteria);

            var results = cluster.Query(query);

            var resultsContainTester = results.Results.Any(p => p.DocumentId == tester.DocumentId);

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.True(resultsContainTester);
        }

        [Fact]
        public void Can_GetById_Query()
        {
            var (cluster, store, documents) = SpinUp();

            var tester = documents[5];

            var criteria = new GetByIdsQueryCriteria(store.StoreId.Name, tester.DocumentId);

            var query = new GetByIdsQuery(criteria);

            var results = cluster.Query(query);

            var resultsContainTester = results.Results.Any(p => p.DocumentId == tester.DocumentId);

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.True(resultsContainTester);
        }

        [Fact(Skip = "Performance test only.")]
        //[Fact]
        public void Performance_Can_Filter_Query()
        {
            var (cluster, store, documents) = SpinUp(1000000);

            var tester = documents[5];

            var filter = new QueryFilter(nameof(tester.LastName), tester.LastName, Comparator.Equal);

            var criteria = new FilteredQueryCriteria(new[] { store.StoreId.Name }, new QueryFilter[] { filter });

            var query = new FilteredQuery(criteria);

            var s = new Stopwatch();
            s.Start();

            var results = cluster.Query(query);

            s.Stop();


            var resultsContainTester = results.Results.All(p => (p as Person)?.LastName.ToLowerInvariant() == tester.LastName.ToLowerInvariant());

            Decommission(cluster);
            output.WriteLine(s.ElapsedMilliseconds.ToString());

            Assert.NotEmpty(results.Results);
            Assert.True(resultsContainTester);

            Assert.True(s.ElapsedMilliseconds < 3000);
        }

        [Fact(Skip = "Performance test only.")]
        //[Fact]
        public void Performance_Can_Simple_Query()
        {
            var (cluster, store, documents) = SpinUp(100000);

            var tester = documents[5];

            var criteria = new SimpleQueryCriteria(new[] { store.StoreId.Name }, tester.LastName);

            var query = new SimpleQuery(criteria);

            var s = new Stopwatch();
            s.Start();

            var results = cluster.Query(query);

            s.Stop();

            var resultsContainTester = results.Results.Any(p => p.DocumentId == tester.DocumentId);

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.True(resultsContainTester);
            Assert.True(s.ElapsedMilliseconds < 500);
        }

        [Fact(Skip = "Performance test only.")]
        //[Fact]
        public void Performance_Can_Suggestion_Query()
        {
            var (cluster, store, documents) = SpinUp(100000);

            var tester = documents[5];

            var criteria = new SuggestionQueryCriteria(new[] { store.StoreId.Name }, tester.LastName);

            var query = new SuggestionQuery(criteria);

            var s = new Stopwatch();
            s.Start();

            var results = cluster.Query(query);

            s.Stop();

            var resultsContainTester = results.Results.Any(p => p.DocumentId == tester.DocumentId);

            Decommission(cluster);

            Assert.NotEmpty(results.Results);
            Assert.True(resultsContainTester);
        }

        [Fact(Skip = "Performance test only.")]
        //[Fact]
        public void Performance_Can_Handle_Simultaneous_Queries()
        {
            var (cluster, store, documents) = SpinUp(100000);

            var s = new Stopwatch();

            s.Start();

            System.Threading.Tasks.Parallel.For(0, 100, i =>
            {
                var tester = documents[i];

                var criteria = new SuggestionQueryCriteria(new[] { store.StoreId.Name }, tester.LastName);

                var query = new SuggestionQuery(criteria);

                var results = cluster.Query(query);
            });

            s.Stop();

            Decommission(cluster);

            Assert.True(s.ElapsedMilliseconds < 30000);
            output.WriteLine(s.ElapsedMilliseconds.ToString());
        }

        [Fact(Skip = "Performance test only.")]
        //[Fact]
        public void Performance_Can_Performantly_Bulk_Index_Large_Quantities()
        {
            var runs = new int[]{ 1000,2000,10000,15000,25000,60000 };

            foreach(var r in runs)
            {
                var count = 500000;
                var pageSize = r;
                var s = new Stopwatch();
                s.Start();
                var (cluster, store, documents) = SpinUp(count, pageSize);

                s.Stop();

                var recordsPerMS = count / (s.ElapsedMilliseconds);

                Decommission(cluster);

                output.WriteLine($"Page Size: {pageSize}");
                output.WriteLine($"{count} records");
                output.WriteLine($"{s.ElapsedMilliseconds} total ms");
                output.WriteLine($"{recordsPerMS} records per ms");
                output.WriteLine($"End run.");
            }


            Assert.True(true);
        }

        [Fact(Skip = "Performance test only.")]
        //[Fact]
        public void Performance_Can_Acceptably_Write_Single_Records()
        {

            const int repeats = 5;

            for (var i = 0; i <= repeats; i++)
            {
                var cluster = new Cluster(settings);

                var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person).FullName);

                var store = cluster.CreateStore(storeSettings);

                var results = new List<long>();
                var documents = TestDataService.GetFakes(store.StoreId.Name, 1000);

                foreach (var doc in documents)
                {
                    var s = new Stopwatch();
                    s.Start();
                    cluster.Commit(new[] { doc });
                    s.Stop();
                    results.Add(s.ElapsedMilliseconds);
                }

                var avg = results.Average();
                var max = results.Max();
                var min = results.Min();
                var variance = max - min;

                output.WriteLine($"Results (ms/record)");
                output.WriteLine($"Avg: {avg}");
                output.WriteLine($"Max: {max}");
                output.WriteLine($"Min: {min}");
                output.WriteLine($"Var: {variance}");

                Decommission(cluster);
            }


            Assert.True(true);
        }

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
            int? countOverride = null,
            int? pageSizeOverride = null)
        {
            var count = countOverride ?? 10000;
            var pageSize = pageSizeOverride ?? 1000;

            var cluster = new Cluster(settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person).FullName);

            var store = cluster.CreateStore(storeSettings);

            var pages = (int)Math.Ceiling((double)count / pageSize);

            var returnDocuments = new List<Person>();
            //Parallel.For(0, pages - 1, (i) =>
            //  {
            //      var quantity = Math.Min(count - (i * pageSize), pageSize);
            //      var documents = TestDataService.GetFakes(store.StoreId.Name, quantity);

            //      if (i == 0)
            //          returnDocuments.AddRange(documents);

            //      var commitResult = cluster.Commit(documents);
            //  });
            for (var i = 0; i < pages; i++)
            {
                var quantity = Math.Min(count - (i * pageSize), pageSize);
                var documents = TestDataService.GetFakes(store.StoreId.Name, quantity);

                if (returnDocuments.Count < 10000)
                    returnDocuments.AddRange(documents);

                var commitResult = cluster.Commit(documents);
            }

            if (count > pageSize)
            {
                Console.WriteLine($"Quantity of requested fake documents ({count}) is more than the maximum page size of {pageSize}. " +
                    $"All documents have been created on the server, but only one page has been returned as a result of this operation.");
            }

            return (cluster, store, returnDocuments.ToArray());
        }
    }
}