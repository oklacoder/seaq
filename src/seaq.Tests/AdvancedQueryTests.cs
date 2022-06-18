using seaq;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace seaq.Tests
{
    public class AdvancedQueryTests :
        TestModule
    {
        public AdvancedQueryTests(
            ITestOutputHelper testOutput) : 
            base(testOutput)
        {

        }

        [Fact]
        public void CanExecute()
        {

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Results);
        }
        [Fact]
        public void CanExecute_Untyped()
        {

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Results);
        }
        [Fact]
        public async void CanExecuteAsync()
        {
            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = await query.ExecuteAsync(_client);
            Assert.True(results != null);
        }
        [Fact]
        public async void CanExecuteAsync_Untyped()
        {
            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices);
            var query = new AdvancedQuery(
                criteria);

            var results = await query.ExecuteAsync(_client);
            Assert.True(results != null);
        }
        [Fact]
        public void CriteriaWorksCorrectly_Take()
        {
            const int _take = 25;

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                take: _take);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);
            Assert.Equal(25, results.Results.Count());
        }
        [Fact]
        public void CriteriaWorksCorrectly_Take_Untyped()
        {
            const int _take = 25;

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                take: _take);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);
            Assert.Equal(25, results.Results.Count());
        }
        [Fact]
        public void CriteriaWorksCorrectly_Skip()
        {
            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                skip: 0);
            var query = new AdvancedQuery<SampleResult>(
                criteria);
            var criteria2 = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                skip: 1);
            var query2 = new AdvancedQuery<SampleResult>(
                criteria2);

            var results = query.Execute(_client);
            var results2 = query2.Execute(_client);

            var r = results.Results.ElementAt(1)?.Document;
            var r2 = results2.Results.ElementAt(0)?.Document;

            Assert.Equal(r.OrderId, r2.OrderId);

        }
        [Fact]
        public void CriteriaWorksCorrectly_Skip_Untyped()
        {
            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                skip: 0);
            var query = new AdvancedQuery(
                criteria);
            var criteria2 = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                skip: 1);
            var query2 = new AdvancedQuery(
                criteria2);

            var results = query.Execute(_client);
            var results2 = query2.Execute(_client);

            var r = results.Results.Select(x => x?.Document as SampleResult).ElementAt(1);
            var r2 = results2.Results.Select(x => x?.Document as SampleResult).ElementAt(0);

            Assert.Equal(r.OrderId, r2.OrderId);

        }
        [Fact]
        public void CriteriaWorksCorrectly_Sort()
        {
            var sort = new[] { new DefaultSortField("order_id", 0, true) };

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                sort);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var actualFirst = results.Results.FirstOrDefault()?.Document as SampleResult;
            var intendedFirst = results.Results.OrderBy(x => (x?.Document as SampleResult)?.OrderId).FirstOrDefault()?.Document as SampleResult;

            Assert.Equal(actualFirst.OrderId, intendedFirst.OrderId);
        }
        [Fact]
        public void CriteriaWorksCorrectly_Sort_Untyped()
        {
            var sort = new[] { new DefaultSortField("order_id", 0, true) };

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                sort);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var actualFirst = results.Results.FirstOrDefault()?.Document as SampleResult;
            var intendedFirst = results.Results.OrderBy(x => (x?.Document as SampleResult)?.OrderId).FirstOrDefault()?.Document as SampleResult;

            Assert.Equal(actualFirst.OrderId, intendedFirst.OrderId);
        }
        [Fact]
        public void ReturnsAllFieldsByDefault()
        {
            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoAllTheseFieldsHaveValues(
                results.Results.FirstOrDefault()?.Document,
                GetObjectFields(results.Results.FirstOrDefault()?.Document).ToArray());

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsAllFieldsByDefault_Untyped()
        {
            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoAllTheseFieldsHaveValues(
                results.Results.FirstOrDefault()?.Document,
                GetObjectFields(results.Results.FirstOrDefault()?.Document).ToArray());

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsOnlySpecifiedFields()
        {
            const string fieldToUse = "customer_full_name";//nameof(SampleResult.CustomerFullName);// 

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                returnFields: new[] { new DefaultReturnField(fieldToUse) }
                );
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoOnlyTheseFieldsHaveValues(
                results.Results.FirstOrDefault()?.Document,
                nameof(SampleResult.CustomerFullName));

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsOnlySpecifiedFields_Untyped()
        {
            const string fieldToUse = "customer_full_name";//nameof(SampleResult.CustomerFullName);// 

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                returnFields: new[] { new DefaultReturnField(fieldToUse) });
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoOnlyTheseFieldsHaveValues(
                results.Results.FirstOrDefault()?.Document,
                nameof(SampleResult.CustomerFullName));

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsNoFieldsWhenEmptySpecified()
        {
            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoAllTheseFieldsHaveValues(
                results.Results.FirstOrDefault()?.Document,
                GetObjectFields(results.Results.FirstOrDefault()?.Document).ToArray());

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsNoFieldsWhenEmptySpecified_Untyped()
        {
            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoAllTheseFieldsHaveValues(
                results.Results.FirstOrDefault()?.Document,
                GetObjectFields(results.Results.FirstOrDefault()?.Document).ToArray());

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsAllRecordsWhenNoFilterSpecified()
        {
            const int expectedTotal = 4675;

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                null,
                null,
                null);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            Assert.Equal(expectedTotal, results.Total);
        }
        [Fact]
        public void ReturnsAllRecordsWhenNoFilterSpecified_Untyped()
        {
            const int expectedTotal = 4675;

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                null,
                null,
                null);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            Assert.Equal(expectedTotal, results.Total);
        }
        [Fact]
        public void ReturnsOnlyMatchingRecordsWhenFilterSpecified_AnyWord()
        {
            const string valToMatch = "Basic";
            var filter = new DefaultFilterField(DefaultComparator.AnyWord, valToMatch, "products.product_name");

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                null,
                null,
                new[] { filter });
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var all = results.Results.All(
                x => x.Document.Products.Any(
                    z => z.ProductName.Contains(valToMatch, StringComparison.OrdinalIgnoreCase)));
            Assert.NotEmpty(results.Results);
            Assert.True(all);
        }
        [Fact]
        public void ReturnsOnlyMatchingRecordsWhenFilterSpecified_AnyWord_Untyped()
        {
            const string valToMatch = "Basic";
            var filter = new DefaultFilterField(DefaultComparator.AnyWord, valToMatch, "products.product_name");

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                null,
                null,
                new[] { filter });
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var all = results.Results.Select(x => x?.Document as SampleResult).All(
                x => x.Products.Any(
                    z => z.ProductName.Contains(valToMatch, StringComparison.OrdinalIgnoreCase)));
            Assert.NotEmpty(results.Results);
            Assert.True(all);
        }
        [Fact]
        public void SubstitutesEqualComparatorWhenNoneSpecified()
        {
            const string valToMatch = "Eddie";
            var filter = new DefaultFilterField(null, valToMatch, "customer_first_name");

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                null,
                null,
                new[] { filter });
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var all = results.Results.All(x => x?.Document?.CustomerFirstName == valToMatch);

            Assert.NotEmpty(results.Results);
            Assert.True(all);
        }
        [Fact]
        public void SubstitutesEqualComparatorWhenNoneSpecified_Untyped()
        {
            const string valToMatch = "Eddie";
            var filter = new DefaultFilterField(null, valToMatch, "customer_first_name");

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                null,
                null,
                new[] { filter });
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var all = results.Results.Select(x => x?.Document as SampleResult).All(x => x?.CustomerFirstName == valToMatch);

            Assert.NotEmpty(results.Results);
            Assert.True(all);
        }
        [Fact]
        public void ReturnsOnlyMatchingRecordsWhenFilterSpecified_Equal()
        {
            const string valToMatch = "Eddie";
            var filter = new DefaultFilterField(DefaultComparator.Equal, valToMatch, "customer_first_name");

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                null,
                null,
                new[] { filter });
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var all = results.Results.All(x => x?.Document?.CustomerFirstName == valToMatch);

            Assert.NotEmpty(results.Results);
            Assert.True(all);
        }
        [Fact]
        public void ReturnsOnlyMatchingRecordsWhenFilterSpecified_Equal_Untyped()
        {
            const string valToMatch = "Eddie";
            var filter = new DefaultFilterField(DefaultComparator.Equal, valToMatch, "customer_first_name");

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                null,
                null,
                new[] { filter });
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var all = results.Results.Select(x => x?.Document as SampleResult).All(x => x?.CustomerFirstName == valToMatch);

            Assert.NotEmpty(results.Results);
            Assert.True(all);
        }
        [Fact]
        public void ReturnsBucketsForSpecifiedFields_ReservedName()
        {
            var b = new[] { new DefaultBucketField("type") };

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                bucketFields: b);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Results);
            Assert.NotEmpty(results.Buckets);
        }
        [Fact]
        public void ReturnsBucketsForSpecifiedFields()
        {
            var b = new[] { new DefaultBucketField("manufacturer.keyword") };

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                bucketFields: b);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Results);
            Assert.NotEmpty(results.Buckets);
        }
        [Fact]
        public void ReturnsBucketsForSpecifiedFields_Untyped()
        {
            var b = new[] { new DefaultBucketField("manufacturer.keyword") };

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                bucketFields: b);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Results);
            Assert.NotEmpty(results.Buckets);
        }
        [Fact]
        public void ReturnsBucketsForSpecifiedFieldsButNoDocumentsWhenZeroPageSize()
        {
            var b = new[] { new DefaultBucketField("manufacturer.keyword") };

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                bucketFields: b,
                take: 0);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.Empty(results.Results);
            Assert.NotEmpty(results.Buckets);
        }
        [Fact]
        public void ReturnsBucketsForSpecifiedFieldsButNoDocumentsWhenZeroPageSize_Untyped()
        {
            var b = new[] { new DefaultBucketField("manufacturer.keyword") };

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                bucketFields: b,
                take: 0);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.Empty(results.Results);
            Assert.NotEmpty(results.Buckets);
        }
        [Fact]
        public void ReturnsNoBucketsWhenNoFieldsSpecified()
        {
            var criteria = new AdvancedQueryCriteria<SampleResult>(
                null,
                SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Results);
            Assert.Empty(results.Buckets);
        }
        [Fact]
        public void ReturnsNoBucketsWhenNoFieldsSpecified_Untyped()
        {
            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Results);
            Assert.Empty(results.Buckets);
        }

        //works correctly
        ///can query based on "IndexAsType" setting

        [Fact]
        public async void CanQueryIndexWithIndexAsTypeSetting()
        {
            //TODO
            const string method = "CanQueryIndexWithIndexAsTypeSetting";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var idxArgs0 = new IndexConfig(
                typeof(TestDoc).FullName,
                typeof(TestDoc).FullName);
            var idx0 = await cluster.CreateIndexAsync(idxArgs0);
            var idxArgs = new IndexConfig(
                typeof(TestDoc1).FullName,
                typeof(TestDoc1).FullName,
                indexAsType: typeof(TestDoc).FullName);
            var idx = await cluster.CreateIndexAsync(idxArgs);

            var docs = GetFakeDocs<TestDoc1>();
            cluster.Commit(docs);

            var criteria = new AdvancedQueryCriteria<TestDoc1>();
            var query = new AdvancedQuery<TestDoc1>(criteria);

            var resp = cluster.Query(query);

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(resp.Results);
            Assert.NotEmpty(resp.Results);
            var t = typeof(TestDoc1);
            resp.Results.ToList().ForEach(x => Assert.IsType(t, x.Document));
        }
        [Fact]
        public async void CanQueryIndexWithIndexAsTypeSetting_Untyped()
        {
            //TODO
            const string method = "CanQueryIndexWithIndexAsTypeSetting";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var idxArgs0 = new IndexConfig(
                typeof(TestDoc).FullName,
                typeof(TestDoc).FullName);
            var idx0 = await cluster.CreateIndexAsync(idxArgs0);
            var idxArgs = new IndexConfig(
                typeof(TestDoc1).FullName,
                typeof(TestDoc1).FullName,
                indexAsType: typeof(TestDoc).FullName);
            var idx = await cluster.CreateIndexAsync(idxArgs);

            var docs = GetFakeDocs<TestDoc1>();
            cluster.Commit(docs);

            var criteria = new AdvancedQueryCriteria(
                null, typeof(TestDoc1).FullName);
            var query = new AdvancedQuery(criteria);

            var resp = cluster.Query<ISeaqQueryResults>(query);

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(resp.Results);
            Assert.NotEmpty(resp.Results);
            var t = typeof(TestDoc1);
            resp.Results.ToList().ForEach(x => Assert.IsType(t, x.Document));
        }

        ///includes message for deprecated
        [Fact]
        public async void IncludesMessagesWhenTargetingDeprecatedIndices()
        {
            const string method = "IncludesMessagesWhenTargetingDeprecatedIndices";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var idxArgs = new IndexConfig(
                typeof(SampleResult).FullName,
                typeof(SampleResult).FullName);
            var idx = await cluster.CreateIndexAsync(idxArgs);

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices);
            var query = new AdvancedQuery(
                criteria);

            var docs = query.Execute(_client)?.Results.Select(x => x.Document as SampleResult);
            foreach(var doc in docs)
            {
                doc.IndexName = idx.Name;
            }

            await cluster.CommitAsync<SampleResult>(docs);

            const string depMsg = "Test Deprecation";
            cluster.DeprecateIndex(idx.Name, depMsg);

            var criteria2 = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName);
            var query2 = new AdvancedQuery(
                criteria2);
            var results = await cluster.QueryAsync<AdvancedQueryResults>(query2);

            DecomissionCluster(cluster);

            Assert.NotNull(results);
            Assert.NotEmpty(results.Messages);
            Assert.Collection(results.Messages,x => x.Contains(depMsg));
        }
        ///excludes hidden
        [Fact]
        public async void ExcludesHiddenIndices()
        {
            const string method = "ExcludesHiddenIndices";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var idxArgs = new IndexConfig(
                typeof(SampleResult).FullName,
                typeof(SampleResult).FullName);
            var idx = await cluster.CreateIndexAsync(idxArgs);
            var idxArgs2 = new IndexConfig(
                typeof(SampleResult).FullName + "2",
                typeof(SampleResult).FullName);
            var idx2 = await cluster.CreateIndexAsync(idxArgs2);

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices);
            var query = new AdvancedQuery(
                criteria);

            var docs = query.Execute(_client)?.Results.Select(x => x.Document as SampleResult);
            foreach (var doc in docs)
            {
                doc.IndexName = idx.Name;
            }
            await cluster.CommitAsync<SampleResult>(docs);
            foreach (var doc in docs)
            {
                doc.IndexName = idx2.Name;
            }
            await cluster.CommitAsync<SampleResult>(docs);

            await cluster.HideIndexAsync(idx.Name);

            var criteria2 = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                take: 100);
            var query2 = new AdvancedQuery(
                criteria2);
            var results = await cluster.QueryAsync<AdvancedQueryResults>(query2);

            DecomissionCluster(cluster);

            Assert.NotNull(results);
            Assert.All(results.Results, x => x.Index.Equals(idx2.Name));
        }
        ///properly handles global search include/exclude
        [Fact]
        public async void ReturnsInGlobalSearch()
        {
            const string method = "IncludesMessagesWhenTargetingDeprecatedIndices";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var idxArgs = new IndexConfig(
                typeof(SampleResult).FullName,
                typeof(SampleResult).FullName);
            var idx = await cluster.CreateIndexAsync(idxArgs);
            var idxArgs2 = new IndexConfig(
                typeof(SampleResult).FullName + "2",
                typeof(SampleResult).FullName);
            var idx2 = await cluster.CreateIndexAsync(idxArgs2);

            var criteria = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices);
            var query = new AdvancedQuery(
                criteria);

            var docs = query.Execute(_client)?.Results.Select(x => x.Document as SampleResult);
            foreach (var doc in docs)
            {
                doc.IndexName = idx.Name;
            }
            await cluster.CommitAsync<SampleResult>(docs);
            foreach (var doc in docs)
            {
                doc.IndexName = idx2.Name;
            }
            await cluster.CommitAsync<SampleResult>(docs);

            await cluster.ExcludeIndexFromGlobalSearchAsync(idx.Name);

            var criteria2 = new AdvancedQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                take: 100);
            var query2 = new AdvancedQuery(
                criteria2);
            var results = await cluster.QueryAsync<AdvancedQueryResults>(query2);

            DecomissionCluster(cluster);

            Assert.NotNull(results);
            Assert.All(results.Results, x => x.Index.Equals(idx2.Name));
        }
    }
}
