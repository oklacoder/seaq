using seaq;
using System.Linq;
using Xunit;

namespace SEAQ.Tests
{
    public class SimpleQueryTests :
        TestModule
    {
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
            Assert.Equal(25, results.Documents.Count());
        }
        [Fact]
        public void SimpleQueryCriteriaWorksCorrectly_Take_Untyped()
        {
            const int _take = 25;
            var criteria = new SimpleQueryCriteria(typeof(SampleResult).FullName, "", new[] { SampleIndex }, 0, _take);
            var query = new SimpleQuery(criteria);

            var results = query.Execute(_client);
            Assert.Equal(25, results.Documents.Count());
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

            var r = results.Documents.ElementAt(1) as SampleResult;
            var r2 = results2.Documents.ElementAt(0) as SampleResult;

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

            var r = results.Documents.ElementAt(1) as SampleResult;
            var r2 = results2.Documents.ElementAt(0) as SampleResult;

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

            var actualFirst = results.Documents.FirstOrDefault() as SampleResult;
            var intendedFirst = results.Documents.OrderBy(x => (x as SampleResult)?.OrderId).FirstOrDefault() as SampleResult;

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

            var actualFirst = results.Documents.FirstOrDefault() as SampleResult;
            var intendedFirst = results.Documents.OrderBy(x => (x as SampleResult)?.OrderId).FirstOrDefault() as SampleResult;

            Assert.Equal(actualFirst.OrderId, intendedFirst.OrderId);
        }
    }
}
