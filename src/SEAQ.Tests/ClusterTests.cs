using Bogus;
using Nest;
using seaq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SEAQ.Tests
{
    public class TestModule
    {
        protected const string Url = "http://localhost:9200/";
        protected const string Username = "elastic";
        protected const string Password = "elastic";
        
        protected ConnectionSettings _connection => new ConnectionSettings(
                new Elasticsearch.Net.SingleNodeConnectionPool(
                    new Uri(Url)))
            .BasicAuthentication(Username, Password);
        protected ElasticClient _client => new ElasticClient(_connection);
        protected const string SampleIndex = "kibana_sample_data_ecommerce";
        protected static string[] SampleIndices => new[] { SampleIndex };

        protected string Scope => Guid.NewGuid().ToString("N");

        protected ClusterArgs GetArgs(string method)
        {
            var scope = $"{Scope}_{method}".ToLower();
            return new ClusterArgs(scope, Url, Username, Password, true);
        }

        public IEnumerable<TestDoc> GetFakeDocs(int count = 100)
        {
            var childFaker = new Faker<TestChild>()
                .RuleFor(x => x.ChildId, f => f.Random.Guid())
                .RuleFor(x => x.StringValue, f => f.Random.Utf16String())
                .RuleFor(x => x.IntValue, f => f.Random.Int())
                .RuleFor(x => x.DoubleValue, f => f.Random.Double())
                .RuleFor(x => x.DecimalValue, f => f.Random.Decimal())
                .RuleFor(x => x.DateValue, f => f.Date.Recent());
            var faker = new Faker<TestDoc>()
                .RuleFor(x => x.DocId, f => f.Random.Guid())
                .RuleFor(x => x.StringValue, f => f.Random.Utf16String())
                .RuleFor(x => x.IntValue, f => f.Random.Int())
                .RuleFor(x => x.DoubleValue, f => f.Random.Double())
                .RuleFor(x => x.DecimalValue, f => f.Random.Decimal())
                .RuleFor(x => x.DateValue, f => f.Date.Recent())
                .RuleFor(x => x.ObjectProperty, f => childFaker.Generate())
                .RuleFor(x => x.CollectionProperty, 
                    f => Enumerable.Range(0, new Random().Next(0, 10)).Select(x => childFaker.Generate()));

            return Enumerable.Range(0, count).Select(x => faker.Generate()).ToList();
        }

        protected static IEnumerable<string> GetObjectFields(object obj)
        {
            return obj.GetType().GetProperties().Where(x => !Constants.Fields.AlwaysReturnedFields.Contains(x.Name)).Select(x => x.Name);
        }
        protected static bool DoAllTheseFieldsHaveValues(object obj, params string[] fields)
        {
            var res = true;
            var props = obj.GetType().GetProperties().Where(x => !Constants.Fields.AlwaysReturnedFields.Contains(x.Name));

            foreach (var p in props.Where(x =>
                fields.Any(z =>
                    z.Equals(x.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var val = p.GetValue(obj)?.ToString();
                res = res && !p.IsPropertyEmpty(val);
            }

            return res;
        }
        protected static bool DoAllTheseFieldsHaveNoValues(object obj, params string[] fields)
        {
            var res = true;
            var props = obj.GetType().GetProperties().Where(x => !Constants.Fields.AlwaysReturnedFields.Contains(x.Name));

            foreach (var p in props.Where(x =>
                 fields.Any(z =>
                     z.Equals(x.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var val = p.GetValue(obj)?.ToString();
                res = res && p.IsPropertyEmpty(val);
            }

            return res;
        }
        protected static bool DoAllButTheseFieldsHaveValues(object obj, params string[] fields)
        {
            var res = true;
            var props = obj.GetType().GetProperties().Where(x => !Constants.Fields.AlwaysReturnedFields.Contains(x.Name));

            foreach (var p in props.Where(x =>
                 fields.Any(z =>
                     !z.Equals(x.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var val = p.GetValue(obj)?.ToString();
                res = res && p.IsPropertyEmpty(val);
            }
            foreach (var p in props.Where(x =>
                 fields.Any(z =>
                     z.Equals(x.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var val = p.GetValue(obj)?.ToString();
                res = res && p.IsPropertyEmpty(val);
            }

            return res;
        }
        protected static bool DoOnlyTheseFieldsHaveValues(object obj, params string[] fields)
        {
            var res = true;
            var props = obj.GetType().GetProperties().Where(x => !Constants.Fields.AlwaysReturnedFields.Contains(x.Name));

            foreach (var p in props.Where(x =>
                 fields.Any(z =>
                     !z.Equals(x.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var val = p.GetValue(obj)?.ToString();
                res = res && p.IsPropertyEmpty(val);
            }
            foreach (var p in props.Where(x =>
                 fields.Any(z =>
                     z.Equals(x.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var val = p.GetValue(obj)?.ToString();
                res = res && !p.IsPropertyEmpty(val);
            }

            return res;
        }

    }
    public static class TestExtensions
    {
        internal static bool IsPropertyEmpty(this PropertyInfo p, string val)
        {
            var res = true;
            if (val == null)
                return true;

            var t = p.PropertyType;

            if (t == typeof(DateTime))
            {
                res = DateTime.Parse(val) == DateTime.MinValue;
            }
            else if (t == typeof(DateTime?))
            {
                res = DateTime.Parse(val) == DateTime.MinValue;
            }
            else
            {
                res = val == null;
            }

            return res;
        }

    }


    public class TestDoc :
        IDocument
    {
        public string Id => DocId.ToString();

        public string IndexName { get; set; }

        public string Type => GetType().FullName;

        public Guid DocId { get; set; }

        public string StringValue { get; set; }
        public int? IntValue { get; set; }
        public double? DoubleValue { get; set; }
        public decimal? DecimalValue { get; set; }
        public DateTime? DateValue { get; set; }
        public TestChild ObjectProperty { get; set; }
        public IEnumerable<TestChild> CollectionProperty { get; set; }
    }

    public class TestChild
    {
        public Guid ChildId { get; set; }
        public string StringValue { get; set; }
        public int? IntValue { get; set; }
        public double? DoubleValue { get; set; }
        public decimal? DecimalValue { get; set; }
        public DateTime? DateValue { get; set; }
    }

    public class ClusterTests :
        TestModule
    {
        //TODO: unhappy paths

        [Fact]
        public async void CanPingCluster()
        {
            const string method = "CanPingCluster";
            //GetArgs(method)
            var cluster = Cluster.Create(GetArgs(method));
            var clusterAsync = Cluster.CreateAsync(GetArgs(method));

            var sync_sync = cluster.CanPing();
            var sync_async = await cluster.CanPingAsync();
            var async_sync = cluster.CanPing();
            var async_async = await cluster.CanPingAsync();

            Assert.True(async_sync);
            Assert.True(async_async);
            Assert.True(sync_sync);
            Assert.True(sync_async);
        }

        [Fact]
        public async void CanCreateIndex()
        {
            const string method = "CanCreateIndex";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var resp = await cluster.CreateIndexAsync(config);

            var exists = cluster.Indices.Any(x => x.Name == config.Name);
            var existsByType = cluster.IndicesByType[type]?.Any();

            var delResp = await cluster.DeleteIndexAsync(config.Name);

            Assert.NotNull(resp);
            Assert.True(exists);
            Assert.True(existsByType);
        }

        [Fact]
        public async void CanDeleteIndex()
        {
            const string method = "CanDeleteIndex";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var resp = await cluster.CreateIndexAsync(config);

            var exists = cluster.Indices.Any(x => x.Name == config.Name);
            var existsByType = cluster.IndicesByType[type]?.Any();

            //delete it here
            var delResp = await cluster.DeleteIndexAsync(config.Name);

            var stillExists = cluster.Indices.Any(x => x.Name == config.Name);
            var stillExistsByType = cluster.IndicesByType[type]?.Any();

            Assert.NotNull(resp);
            Assert.True(exists);
            Assert.True(existsByType);
            Assert.True(delResp);
            Assert.False(stillExists);
            Assert.False(stillExistsByType);
        }

        //can index 1
        [Fact]
        public async void CanIndexOneDocument()
        {
            const string method = "CanIndexOneDocument";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var doc = GetFakeDocs(1).FirstOrDefault();

            var resp = await cluster.CommitAsync(doc);

            await cluster.DeleteIndexAsync(config.Name);

            Assert.True(resp);
        }
        //can index multi
        [Fact]
        public async void CanIndex100Documents()
        {
            const string method = "CanIndex100Documents";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs(100);

            var resp = await cluster.CommitAsync(docs);

            await cluster.DeleteIndexAsync(config.Name);

            Assert.True(resp);
        }
        [Fact]
        public async void CanIndex10000Documents()
        {
            const string method = "CanIndex10000Documents";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs(10000);

            var resp = await cluster.CommitAsync(docs);

            await cluster.DeleteIndexAsync(config.Name);

            Assert.True(resp);
        }
        //can copy index just schema
        [Fact]
        public async void CanCopyIndexSchema()
        {
            const string method = "CanCopyIndexSchema";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            
            await cluster.CreateIndexAsync(config);
            var name2 = $"{config.Name}_2";
            var resp = await cluster.CopyIndexAsync(config.Name, name2, false);

            var exists = cluster.Indices.Any(x => x.Name == name2);
            var existsByType = cluster.IndicesByType[type]?.Any(x => x.Name.Equals(name2));

            await cluster.DeleteIndexAsync(config.Name);
            await cluster.DeleteIndexAsync(name2);

            Assert.NotNull(resp);
            Assert.Equal(resp.Name, name2);
            Assert.Equal(resp.Type, config.Type);
        }
        //can copy index with docs

        [Fact]
        public async void CanCopyIndexSchemaWithDocs()
        {
            const string method = "CanCopyIndexSchemaWithDocs";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);

            await cluster.CreateIndexAsync(config);
            var name2 = $"{config.Name}_2";

            var docs = GetFakeDocs(10000);

            foreach (var d in docs)
            {
                d.IndexName = config.Name;
            }

            await cluster.CommitAsync(docs);

            var resp = await cluster.CopyIndexAsync(config.Name, name2, true);

            var exists = cluster.Indices.Any(x => x.Name == name2);
            var existsByType = cluster.IndicesByType[type]?.Any(x => x.Name.Equals(name2));

            await cluster.DeleteIndexAsync(config.Name);
            await cluster.DeleteIndexAsync(name2);

            Assert.NotNull(resp);
            Assert.Equal(resp.Name, name2);
            Assert.Equal(resp.Type, config.Type);
        }
        [Fact]
        public async void CanGetIndexDefinition()
        {
            const string method = "CanGetIndexDefinition";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var idx = await cluster.CreateIndexAsync(config);

            var resp = await cluster.GetIndexDefinitionAsync(config.Name);

            var exists = cluster.Indices.Any(x => x.Name == resp.Name);
            var existsByType = cluster.IndicesByType[resp.Type]?.Any();

            var delResp = await cluster.DeleteIndexAsync(config.Name);

            Assert.NotNull(resp);
            Assert.True(exists);
            Assert.True(existsByType);
        }

        //update idx def
        [Fact]
        public async void CanUpdateIndexDefinition()
        {
            const string method = "CanUpdateIndexDefinition";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            var f = new seaq.Field("Test Update Field");
            var fields = createResp.Fields?.ToList() ?? new List<seaq.Field>();
            fields.Add(f);
            createResp.Fields = fields;

            var resp = await cluster.UpdateIndexDefinitionAsync(createResp);

            var newFieldExists = resp.Fields.Any(x => x.Name.Equals(f.Name, StringComparison.OrdinalIgnoreCase));

            await cluster.DeleteIndexAsync(config.Name);

            Assert.NotNull(resp);
            Assert.True(newFieldExists);
        }


        //delete single doc
        [Fact]
        public async void CanDelete1Document()
        {
            const string method = "CanDelete1Document";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs(100).ToList();

            await cluster.CommitAsync(docs);

            var toDelete = docs.ElementAt(5);

            var resp = await cluster.DeleteAsync(toDelete);

            await cluster.DeleteIndexAsync(config.Name);

            Assert.True(resp);
        }
        //delete mult docs
        [Fact]
        public async void CanDelete5Documents()
        {
            const string method = "CanDelete5Documents";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs(100).ToList();

            await cluster.CommitAsync(docs);

            var toDelete = docs.Take(5);

            var resp = await cluster.DeleteAsync(toDelete);

            await cluster.DeleteIndexAsync(config.Name);

            Assert.True(resp);
        }
    }

    public class SimpleQueryTests :
        TestModule
    {

        const string SampleIndex = "kibana_sample_data_ecommerce";
        [Fact]
        public void CanExecuteSimpleQuery()
        {
            var criteria = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex });
            var query = new SimpleQuery<SampleResult>(criteria);

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
        public void SimpleQueryCriteriaWorksCorrectly_Take()
        {
            const int _take = 25;
            var criteria = new SimpleQueryCriteria<SampleResult>("", new[] { SampleIndex }, 0, _take);
            var query = new SimpleQuery<SampleResult>(criteria);

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
    }


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
        public void ReturnsOnlySpecifiedFields()
        {
            const string fieldToUse = "customer_full_name";//nameof(SampleResult.CustomerFullName);// 

            var criteria = new AdvancedQueryCriteria<SampleResult>(
                SampleIndices,
                returnFields: new[] { new DefaultReturnField(fieldToUse) });
            var query = new AdvancedQuery<SampleResult>(
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
    }
}
