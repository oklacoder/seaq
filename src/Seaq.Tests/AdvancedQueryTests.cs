using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Seaq.Tests
{
    public class AdvancedQueryTests
    {

        [Fact]
        public void CanExecute()
        {
            var c = TestUtil.Client;

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(c);
            Assert.True(results != null);
            Assert.NotEmpty(results.Documents);
        }
        [Fact]
        public async void CanExecuteAsync()
        {
            var c = TestUtil.Client;

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = await query.ExecuteAsync(c);
            Assert.True(results != null);
        }
        [Fact]
        public void CriteriaWorksCorrectly_Take()
        {
            var c = TestUtil.Client;

            const int _take = 25;

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices,
                take: _take);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(c);
            Assert.Equal(25, results.Documents.Count());
        }
        [Fact]
        public void CriteriaWorksCorrectly_Skip()
        {
            var c = TestUtil.Client;

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices,
                skip: 0);
            var query = new AdvancedQuery<SampleResult>(
                criteria);
            var criteria2 = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices,
                skip: 1);
            var query2 = new AdvancedQuery<SampleResult>(
                criteria2);

            var results = query.Execute(c);
            var results2 = query2.Execute(c);

            var r = results.Documents.ElementAt(1) as SampleResult;
            var r2 = results2.Documents.ElementAt(0) as SampleResult;

            Assert.Equal(r.OrderId, r2.OrderId);

        }
        [Fact]
        public void CriteriaWorksCorrectly_Sort()
        {
            var c = TestUtil.Client;

            var sort = new[] { new DefaultSortField("order_id", true, 0) };

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices,
                sort);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(c);

            var actualFirst = results.Documents.FirstOrDefault() as SampleResult;
            var intendedFirst = results.Documents.OrderBy(x => (x as SampleResult)?.OrderId).FirstOrDefault() as SampleResult;

            Assert.Equal(actualFirst.OrderId, intendedFirst.OrderId);
        }
        [Fact]
        public void ReturnsAllFieldsByDefault()
        {
            var c = TestUtil.Client;

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(c);

            var hasValues = TestUtil.DoAllTheseFieldsHaveValues(
                results.Documents.FirstOrDefault(),
                TestUtil.GetObjectFields(results.Documents.FirstOrDefault()).ToArray());

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsOnlySpecifiedFields()
        {
            var c = TestUtil.Client;

            const string fieldToUse = "customer_full_name";//nameof(SampleResult.CustomerFullName);// 

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices,
                returnFields: new[] { new DefaultReturnField(fieldToUse) });
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(c);

            var hasValues = TestUtil.DoOnlyTheseFieldsHaveValues(
                results.Documents.FirstOrDefault(),
                nameof(SampleResult.CustomerFullName));

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsNoFieldsWhenEmptySpecified()
        {
            var c = TestUtil.Client;

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(c);

            var hasValues = TestUtil.DoAllTheseFieldsHaveValues(
                results.Documents.FirstOrDefault(),
                TestUtil.GetObjectFields(results.Documents.FirstOrDefault()).ToArray());

            Assert.True(hasValues);
        }
        [Fact]
        public void ReturnsAllRecordsWhenNoFilterSpecified()
        {
            var c = TestUtil.Client;

            const int expectedTotal = 4675;

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices,
                null,
                null,
                null);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(c);

            Assert.Equal(expectedTotal, results.Total);
        }
        [Fact]
        public void ReturnsOnlyMatchingRecordsWhenFilterSpecified_AnyWord()
        {
            var c = TestUtil.Client;

            const string valToMatch = "Basic";
            var filter = new DefaultFilterField(DefaultComparator.AnyWord, valToMatch, "products.product_name");

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices,
                null,
                null,
                new[] { filter });
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(c);

            var all = results.Documents.All(
                x => x.Products.Any(
                    z => z.ProductName.Contains(valToMatch, StringComparison.OrdinalIgnoreCase)));
            Assert.NotEmpty(results.Documents);
            Assert.True(all);
        }
        [Fact]
        public void ReturnsOnlyMatchingRecordsWhenFilterSpecified_Equal()
        {
            var c = TestUtil.Client;

            const string valToMatch = "Eddie";
            var filter = new DefaultFilterField(DefaultComparator.Equal, valToMatch, "customer_first_name");

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices,
                null,
                null,
                new[] { filter });
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(c);

            var all = results.Documents.All(x => x.CustomerFirstName == valToMatch);
            
            Assert.NotEmpty(results.Documents);
            Assert.True(all);            
        }
        [Fact]
        public void ReturnsBucketsForSpecifiedFields()
        {
            var c = TestUtil.Client;

            var b = new[] { new DefaultBucketField("manufacturer.keyword") };

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices,
                bucketFields: b);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(c);
            Assert.True(results != null);
            Assert.NotEmpty(results.Documents);
            Assert.NotEmpty((results as AdvancedQueryResults<SampleResult>)?.Buckets);
        }
        [Fact]
        public void ReturnsBucketsForSpecifiedFieldsButNoDocumentsWhenZeroPageSize()
        {
            var c = TestUtil.Client;

            var b = new[] { new DefaultBucketField("manufacturer.keyword") };

            
            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices,
                bucketFields: b,
                take: 0);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(c);
            Assert.True(results != null);
            Assert.Empty(results.Documents);
            Assert.NotEmpty((results as AdvancedQueryResults<SampleResult>)?.Buckets);
        }


        [Fact]
        public void ReturnsNoBucketsWhenNoFieldsSpecified()
        {
            var c = TestUtil.Client;

            
            var criteria = new AdvancedQueryCriteria<SampleResult>(
                TestUtil.SampleIndices);
            var query = new AdvancedQuery<SampleResult>(
                criteria);

            var results = query.Execute(c);
            Assert.True(results != null);
            Assert.NotEmpty(results.Documents);
            Assert.Empty((results as AdvancedQueryResults<SampleResult>)?.Buckets);
        }
    }
}
