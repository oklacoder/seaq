using seaq;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SEAQ.Tests
{
    public class SimpleQueryTests :
        TestModule
    {
        public SimpleQueryTests(
            ITestOutputHelper testOutput) :
            base(testOutput)
        {

        }

        [Fact]
        public void CanExecuteSimpleQuery()
        {
            var criteria = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex });
            var query = new SimpleQuery<SampleResult>(criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
        }
        [Fact]
        public void CanExecuteSimpleQuery_Untyped()
        {
            var criteria = new SimpleQueryCriteria(
                typeof(SampleResult).FullName,
                "", 
                new[] { SampleIndex });
            var query = new SimpleQuery(criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
        }

        [Fact]
        public void CanExecuteSimpleQueryAsync()
        {
            var criteria = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex });
            var query = new SimpleQuery<SampleResult>(criteria);

            var results = query.ExecuteAsync(_client).Result;
            Assert.True(results != null);
        }
        [Fact]
        public void CanExecuteSimpleQueryAsync_Untyped()
        {
            var criteria = new SimpleQueryCriteria(
                typeof(SampleResult).FullName, "", new[] { SampleIndex });
            var query = new SimpleQuery(criteria);

            var results = query.ExecuteAsync(_client).Result;
            Assert.True(results != null);
        }

        [Fact]
        public void SimpleQueryCriteriaWorksCorrectly_Take()
        {
            const int _take = 25;
            var criteria = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex }, 0, _take);
            var query = new SimpleQuery<SampleResult>(criteria);

            var results = query.Execute(_client);
            Assert.Equal(25, results.Results.Count());
        }
        [Fact]
        public void SimpleQueryCriteriaWorksCorrectly_Take_Untyped()
        {
            const int _take = 25;
            var criteria = new SimpleQueryCriteria(typeof(SampleResult).FullName, "", new[] { SampleIndex }, 0, _take);
            var query = new SimpleQuery(criteria);

            var results = query.Execute(_client);
            Assert.Equal(25, results.Results.Count());
        }
        [Fact]
        public void SimpleQueryCriteriaWorksCorrectly_Skip()
        {
            const int _take = 25;

            var criteria = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex }, 0, _take);
            var criteria2 = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex }, 1, _take);
            var query = new SimpleQuery<SampleResult>(criteria);
            var query2 = new SimpleQuery<SampleResult>(criteria2);


            var results = query.Execute(_client);
            var results2 = query2.Execute(_client);

            var r = results.Results.ElementAt(1)?.Document as SampleResult;
            var r2 = results2.Results.ElementAt(0)?.Document as SampleResult;

            Assert.Equal(r.OrderId, r2.OrderId);

        }
        [Fact]
        public void SimpleQueryCriteriaWorksCorrectly_Skip_Untyped()
        {
            const int _take = 25;

            var criteria = new SimpleQueryCriteria(typeof(SampleResult).FullName, "", new[] { SampleIndex }, 0, _take);
            var criteria2 = new SimpleQueryCriteria(typeof(SampleResult).FullName, "", new[] { SampleIndex }, 1, _take);
            var query = new SimpleQuery(criteria);
            var query2 = new SimpleQuery(criteria2);


            var results = query.Execute(_client);
            var results2 = query2.Execute(_client);

            var r = results.Results.ElementAt(1)?.Document as SampleResult;
            var r2 = results2.Results.ElementAt(0)?.Document as SampleResult;

            Assert.Equal(r.OrderId, r2.OrderId);

        }

        [Fact]
        public void SimpleQueryCriteriaWorksCorrectly_Sort()
        {
            const int _take = 25;

            var sort = new[] { new DefaultSortField("order_id", 0, true) };

            var criteria = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex }, 0, _take, sort);
            var query = new SimpleQuery<SampleResult>(criteria);

            var results = query.Execute(_client);

            var actualFirst = results.Results.FirstOrDefault()?.Document as SampleResult;
            var intendedFirst = results.Results.OrderBy(x => (x?.Document as SampleResult)?.OrderId).FirstOrDefault()?.Document as SampleResult;

            Assert.Equal(actualFirst.OrderId, intendedFirst.OrderId);
        }
        [Fact]
        public void SimpleQueryCriteriaWorksCorrectly_Sort_Untyped()
        {
            const int _take = 25;

            var sort = new[] { new DefaultSortField("order_id", 0, true) };

            var criteria = new SimpleQueryCriteria(typeof(SampleResult).FullName, "", new[] { SampleIndex }, 0, _take, sort);
            var query = new SimpleQuery(criteria);

            var results = query.Execute(_client);

            var actualFirst = results.Results.FirstOrDefault()?.Document as SampleResult;
            var intendedFirst = results.Results.OrderBy(x => (x?.Document as SampleResult)?.OrderId).FirstOrDefault()?.Document as SampleResult;

            Assert.Equal(actualFirst.OrderId, intendedFirst.OrderId);
        }
        [Fact]
        public void ReturnsOnlySpecifiedFields()
        {
            const string fieldToUse = "customer_full_name";//nameof(SampleResult.CustomerFullName);// 

            var criteria = new SimpleQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                returnFields: new[] { new DefaultReturnField(fieldToUse) }
                );
            var query = new SimpleQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoOnlyTheseFieldsHaveValues(
                results.Results.FirstOrDefault()?.Document,
                nameof(SampleResult.CustomerFullName));

            Assert.True(hasValues);
        }
        [Fact]
        public async void ReturnsOnlySpecifiedFields_Untyped()
        {
            const string method = "ReturnsOnlySpecifiedFields_Untyped";
            const string fieldToUse = "customer_full_name";//nameof(SampleResult.CustomerFullName);// 

            var criteria = new SimpleQueryCriteria(
                typeof(SampleResult).FullName,
                null,
                SampleIndices,
                returnFields: new[] { new DefaultReturnField(fieldToUse) });
            var query = new SimpleQuery(
                criteria);

            var r1 = query.Execute(_client);
            //var cluster = await Cluster.CreateAsync(GetArgs(method));
            //var r2 = cluster.Query<ISeaqQueryResults>(query);

            var hasValues1 = DoOnlyTheseFieldsHaveValues(
                r1.Results.FirstOrDefault()?.Document,
                nameof(SampleResult.CustomerFullName));
            //var hasValues2 = DoOnlyTheseFieldsHaveValues(
            //    r2.Results.FirstOrDefault()?.Document,
            //    nameof(SampleResult.CustomerFullName));

            Assert.True(hasValues1);
            //Assert.True(hasValues2);
        }
        //works correctly
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

            var criteria = new SimpleQueryCriteria(
                typeof(SampleResult).FullName,
                null,
                SampleIndices);
            var query = new SimpleQuery(
                criteria);

            var docs = query.Execute(_client)?.Results.Select(x => x.Document as SampleResult);
            foreach (var doc in docs)
            {
                doc.IndexName = idx.Name;
            }

            await cluster.CommitAsync<SampleResult>(docs);

            const string depMsg = "Test Deprecation";
            cluster.DeprecateIndex(idx.Name, depMsg);

            var criteria2 = new SimpleQueryCriteria(
                typeof(SampleResult).FullName, null);
            var query2 = new SimpleQuery(
                criteria2);
            var results = await cluster.QueryAsync<SimpleQueryResults>(query2);


            foreach(var i in cluster.Indices)
            {
                await cluster.DeleteIndexAsync(i.Name);
            }

            Assert.NotNull(results);
            Assert.NotEmpty(results.Messages);
            Assert.Collection(results.Messages, x => x.Contains(depMsg));
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

            var criteria = new SimpleQueryCriteria(
                typeof(SampleResult).FullName,
                null,
                SampleIndices);
            var query = new SimpleQuery(
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

            var criteria2 = new SimpleQueryCriteria(
                typeof(SampleResult).FullName,
                null,
                take: 100);
            var query2 = new SimpleQuery(
                criteria2);
            var results = await cluster.QueryAsync<SimpleQueryResults>(query2);


            foreach (var i in cluster.Indices)
            {
                await cluster.DeleteIndexAsync(i.Name);
            }

            Assert.NotNull(results);
            Assert.All(results.Results, x => x.Index.Equals(idx2.Name));
        }
        [Fact]
        public async void ExcludesHiddenIndicesWithGlobalSearchEnabled()
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

            var criteria = new SimpleQueryCriteria(
                typeof(SampleResult).FullName,
                null,
                SampleIndices);
            var query = new SimpleQuery(
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
            await cluster.IncludeIndexInGlobalSearchAsync(idx.Name);

            var criteria2 = new SimpleQueryCriteria(
                typeof(SampleResult).FullName,
                null,
                take: 100);
            var query2 = new SimpleQuery(
                criteria2);
            var results = await cluster.QueryAsync<SimpleQueryResults>(query2);


            foreach (var i in cluster.Indices)
            {
                await cluster.DeleteIndexAsync(i.Name);
            }

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

            var criteria = new SimpleQueryCriteria(
                typeof(SampleResult).FullName,
                null,
                SampleIndices);
            var query = new SimpleQuery(
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

            var criteria2 = new SimpleQueryCriteria(
                typeof(SampleResult).FullName,
                null,
                take: 100);
            var query2 = new SimpleQuery(
                criteria2);
            var results = await cluster.QueryAsync<SimpleQueryResults>(query2);


            foreach (var i in cluster.Indices)
            {
                await cluster.DeleteIndexAsync(i.Name);
            }

            Assert.NotNull(results);
            Assert.All(results.Results, x => x.Index.Equals(idx2.Name));
        }
    }
}
