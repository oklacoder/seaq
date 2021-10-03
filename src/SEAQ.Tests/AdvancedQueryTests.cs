using seaq;
using System;
using System.Linq;
using Xunit;

namespace SEAQ.Tests
{
    public class AdvancedQueryTests :
        TestModule
    {

        [Fact]
        public void CanExecute()
        {

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Documents);
        }
        [Fact]
        public void CanExecute_Untyped()
        {

            var criteria = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Documents);
        }
        [Fact]
        public async void CanExecuteAsync()
        {
            var criteria = new AdvancedQueryCriteria<SampleResult>(
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
                SampleIndices,
                take: _take);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);
            Assert.Equal(25, results.Documents.Count());
        }
        [Fact]
        public void CriteriaWorksCorrectly_Take_Untyped()
        {
            const int _take = 25;

            var criteria = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices,
                take: _take);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);
            Assert.Equal(25, results.Documents.Count());
        }
        [Fact]
        public void CriteriaWorksCorrectly_Skip()
        {
            var criteria = new AdvancedQueryCriteria<SampleResult>(
                SampleIndices,
                skip: 0);
            var query = new AdvancedQuery<SampleResult>(
                criteria);
            var criteria2 = new AdvancedQueryCriteria<SampleResult>(
                SampleIndices,
                skip: 1);
            var query2 = new AdvancedQuery<SampleResult>(
                criteria2);

            var results = query.Execute(_client);
            var results2 = query2.Execute(_client);

            var r = results.Documents.ElementAt(1) as SampleResult;
            var r2 = results2.Documents.ElementAt(0) as SampleResult;

            Assert.Equal(r.OrderId, r2.OrderId);

        }
        [Fact]
        public void CriteriaWorksCorrectly_Skip_Untyped()
        {
            var criteria = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices,
                skip: 0);
            var query = new AdvancedQuery(
                criteria);
            var criteria2 = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices,
                skip: 1);
            var query2 = new AdvancedQuery(
                criteria2);

            var results = query.Execute(_client);
            var results2 = query2.Execute(_client);

            var r = results.Documents.Select(x => x as SampleResult).ElementAt(1);
            var r2 = results2.Documents.Select(x => x as SampleResult).ElementAt(0);

            Assert.Equal(r.OrderId, r2.OrderId);

        }
        [Fact]
        public void CriteriaWorksCorrectly_Sort()
        {
            var sort = new[] { new DefaultSortField("order_id", 0, true) };

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                SampleIndices,
                sort);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var actualFirst = results.Documents.FirstOrDefault() as SampleResult;
            var intendedFirst = results.Documents.OrderBy(x => (x as SampleResult)?.OrderId).FirstOrDefault() as SampleResult;

            Assert.Equal(actualFirst.OrderId, intendedFirst.OrderId);
        }
        [Fact]
        public void CriteriaWorksCorrectly_Sort_Untyped()
        {
            var sort = new[] { new DefaultSortField("order_id", 0, true) };

            var criteria = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices,
                sort);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var actualFirst = results.Documents.FirstOrDefault() as SampleResult;
            var intendedFirst = results.Documents.OrderBy(x => (x as SampleResult)?.OrderId).FirstOrDefault() as SampleResult;

            Assert.Equal(actualFirst.OrderId, intendedFirst.OrderId);
        }
        [Fact]
        public void ReturnsAllFieldsByDefault()
        {
            var criteria = new AdvancedQueryCriteria<SampleResult>(
                SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoAllTheseFieldsHaveValues(
                results.Documents.FirstOrDefault(),
                GetObjectFields(results.Documents.FirstOrDefault()).ToArray());

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsAllFieldsByDefault_Untyped()
        {
            var criteria = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoAllTheseFieldsHaveValues(
                results.Documents.FirstOrDefault(),
                GetObjectFields(results.Documents.FirstOrDefault()).ToArray());

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsOnlySpecifiedFields()
        {
            const string fieldToUse = "customer_full_name";//nameof(SampleResult.CustomerFullName);// 

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                SampleIndices,
                returnFields: new[] { new DefaultReturnField(fieldToUse) }
                );
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoOnlyTheseFieldsHaveValues(
                results.Documents.FirstOrDefault(),
                nameof(SampleResult.CustomerFullName));

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsOnlySpecifiedFields_Untyped()
        {
            const string fieldToUse = "customer_full_name";//nameof(SampleResult.CustomerFullName);// 

            var criteria = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices,
                returnFields: new[] { new DefaultReturnField(fieldToUse) });
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoOnlyTheseFieldsHaveValues(
                results.Documents.FirstOrDefault(),
                nameof(SampleResult.CustomerFullName));

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsNoFieldsWhenEmptySpecified()
        {
            var criteria = new AdvancedQueryCriteria<SampleResult>(
                SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoAllTheseFieldsHaveValues(
                results.Documents.FirstOrDefault(),
                GetObjectFields(results.Documents.FirstOrDefault()).ToArray());

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsNoFieldsWhenEmptySpecified_Untyped()
        {
            var criteria = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var hasValues = DoAllTheseFieldsHaveValues(
                results.Documents.FirstOrDefault(),
                GetObjectFields(results.Documents.FirstOrDefault()).ToArray());

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsAllRecordsWhenNoFilterSpecified()
        {
            const int expectedTotal = 4675;

            var criteria = new AdvancedQueryCriteria<SampleResult>(
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
                SampleIndices,
                null,
                null,
                new[] { filter });
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var all = results.Documents.All(
                x => x.Products.Any(
                    z => z.ProductName.Contains(valToMatch, StringComparison.OrdinalIgnoreCase)));
            Assert.NotEmpty(results.Documents);
            Assert.True(all);
        }
        [Fact]
        public void ReturnsOnlyMatchingRecordsWhenFilterSpecified_AnyWord_Untyped()
        {
            const string valToMatch = "Basic";
            var filter = new DefaultFilterField(DefaultComparator.AnyWord, valToMatch, "products.product_name");

            var criteria = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices,
                null,
                null,
                new[] { filter });
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var all = results.Documents.Select(x => x as SampleResult).All(
                x => x.Products.Any(
                    z => z.ProductName.Contains(valToMatch, StringComparison.OrdinalIgnoreCase)));
            Assert.NotEmpty(results.Documents);
            Assert.True(all);
        }
        [Fact]
        public void ReturnsOnlyMatchingRecordsWhenFilterSpecified_Equal()
        {
            const string valToMatch = "Eddie";
            var filter = new DefaultFilterField(DefaultComparator.Equal, valToMatch, "customer_first_name");

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                SampleIndices,
                null,
                null,
                new[] { filter });
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);

            var all = results.Documents.All(x => x.CustomerFirstName == valToMatch);

            Assert.NotEmpty(results.Documents);
            Assert.True(all);
        }
        [Fact]
        public void ReturnsOnlyMatchingRecordsWhenFilterSpecified_Equal_Untyped()
        {
            const string valToMatch = "Eddie";
            var filter = new DefaultFilterField(DefaultComparator.Equal, valToMatch, "customer_first_name");

            var criteria = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices,
                null,
                null,
                new[] { filter });
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);

            var all = results.Documents.Select(x => x as SampleResult).All(x => x.CustomerFirstName == valToMatch);

            Assert.NotEmpty(results.Documents);
            Assert.True(all);
        }
        [Fact]
        public void ReturnsBucketsForSpecifiedFields()
        {
            var b = new[] { new DefaultBucketField("manufacturer.keyword") };

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                SampleIndices,
                bucketFields: b);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Documents);
            Assert.NotEmpty((results as AdvancedQueryResults<SampleResult>)?.Buckets);
        }
        [Fact]
        public void ReturnsBucketsForSpecifiedFields_Untyped()
        {
            var b = new[] { new DefaultBucketField("manufacturer.keyword") };

            var criteria = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices,
                bucketFields: b);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Documents);
            Assert.NotEmpty((results as AdvancedQueryResults<BaseDocument>)?.Buckets);
        }
        [Fact]
        public void ReturnsBucketsForSpecifiedFieldsButNoDocumentsWhenZeroPageSize()
        {
            var b = new[] { new DefaultBucketField("manufacturer.keyword") };

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                SampleIndices,
                bucketFields: b,
                take: 0);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.Empty(results.Documents);
            Assert.NotEmpty((results as AdvancedQueryResults<SampleResult>)?.Buckets);
        }
        [Fact]
        public void ReturnsBucketsForSpecifiedFieldsButNoDocumentsWhenZeroPageSize_Untyped()
        {
            var b = new[] { new DefaultBucketField("manufacturer.keyword") };

            var criteria = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices,
                bucketFields: b,
                take: 0);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.Empty(results.Documents);
            Assert.NotEmpty((results as AdvancedQueryResults<BaseDocument>)?.Buckets);
        }
        [Fact]
        public void ReturnsNoBucketsWhenNoFieldsSpecified()
        {
            var criteria = new AdvancedQueryCriteria<SampleResult>(
                SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Documents);
            Assert.Empty((results as AdvancedQueryResults<SampleResult>)?.Buckets);
        }
        [Fact]
        public void ReturnsNoBucketsWhenNoFieldsSpecified_Untyped()
        {
            var criteria = new AdvancedQueryCriteria(
                typeof(SampleResult).FullName,
                SampleIndices);
            var query = new AdvancedQuery(
                criteria);

            var results = query.Execute(_client);
            Assert.True(results != null);
            Assert.NotEmpty(results.Documents);
            Assert.Empty((results as AdvancedQueryResults<BaseDocument>)?.Buckets);
        }
    }
}
