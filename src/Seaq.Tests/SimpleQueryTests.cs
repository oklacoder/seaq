using Newtonsoft.Json;
using Seaq;
using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace Seaq.Tests
{


    public class SimpleQueryTests
    {
        const string SampleIndex = "kibana_sample_data_ecommerce";
        [Fact]
        public void CanExecuteSimpleQuery()
        {
            var c = new Nest.ElasticClient(new Uri("http://localhost:9200"));

            var criteria = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex });
            var query = new SimpleQuery<SampleResult>(criteria);

            var results = query.Execute(c);
            Assert.True(results != null);
        }

        [Fact]
        public void CanExecuteSimpleQueryAsync()
        {
            var c = new Nest.ElasticClient(new Uri("http://localhost:9200"));

            var criteria = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex });
            var query = new SimpleQuery<SampleResult>(criteria);

            var results = query.ExecuteAsync(c).Result;
            Assert.True(results != null);
        }

        [Fact]
        public void SimpleQueryCriteriaWorksCorrectly_Take()
        {
            var c = new Nest.ElasticClient(new Uri("http://localhost:9200"));

            const int _take = 25;
            var criteria = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex }, 0, _take);
            var query = new SimpleQuery<SampleResult>(criteria);

            var results = query.Execute(c);
            Assert.Equal(25, results.Documents.Count());
        }
        [Fact]
        public void SimpleQueryCriteriaWorksCorrectly_Skip()
        {
            var c = new Nest.ElasticClient(new Uri("http://localhost:9200"));

            const int _take = 25;

            var criteria = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex }, 0, _take);
            var criteria2 = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex }, 1, _take);
            var query = new SimpleQuery<SampleResult>(criteria);
            var query2 = new SimpleQuery<SampleResult>(criteria2);


            var results = query.Execute(c);
            var results2 = query2.Execute(c);

            var r = results.Documents.ElementAt(1) as SampleResult;
            var r2 = results2.Documents.ElementAt(0) as SampleResult;

            Assert.Equal(r.OrderId, r2.OrderId);

        }

        [Fact]
        public void SimpleQueryCriteriaWorksCorrectly_Sort()
        {
            var c = new Nest.ElasticClient(new Uri("http://localhost:9200"));

            const int _take = 25;

            var sort = new[] { new DefaultSortField("order_id", true, 0) };

            var criteria = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex }, 0, _take, sort);
            var query = new SimpleQuery<SampleResult>(criteria);

            var results = query.Execute(c);

            var actualFirst = results.Documents.FirstOrDefault() as SampleResult;
            var intendedFirst = results.Documents.OrderBy(x => (x as SampleResult)?.OrderId).FirstOrDefault() as SampleResult;

            Assert.Equal(actualFirst.OrderId, intendedFirst.OrderId);
        }
    }
}
