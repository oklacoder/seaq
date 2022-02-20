using Bogus;
using Nest;
using seaq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace SEAQ.Tests
{
    public class TestModule
    {
        protected const string Url_8x = "https://localhost:9200/";
        protected const string Url_7x = "http://localhost:9200/";
        protected const string Url_6x = "http://localhost:9200/";
        protected const string Username = "elastic";
        protected const string Password = "elastic";
        protected string Url => Url_8x;

        protected ConnectionSettings _connection => new ConnectionSettings(
                new Elasticsearch.Net.SingleNodeConnectionPool(
                    new Uri(Url)),
                (a, b) => new DefaultSeaqElasticsearchSerializer(TryGetSearchType))
            .ServerCertificateValidationCallback((a, b, c, d) => true)
            .EnableApiVersioningHeader()
            .BasicAuthentication(Username, Password);
        protected ElasticClient _client => new ElasticClient(_connection);
        protected const string SampleIndex = "kibana_sample_data_ecommerce";
        private readonly ITestOutputHelper testOutput;

        protected static string[] SampleIndices => new[] { SampleIndex };
        protected static Dictionary<string, Type> _searchableTypes;

        public TestModule(
            ITestOutputHelper testOutput)
        {
            this.testOutput = testOutput;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TestOutput(testOutput)
                .CreateLogger();
            _searchableTypes = FieldNameUtilities.GetAllSearchableTypes().ToDictionary(t => t.FullName, t => t);
            _searchableTypes.Add("order", typeof(SampleResult));
        }

        private Type TryGetSearchType(
            string typeFullName)
        {
            if (_searchableTypes.TryGetValue(typeFullName, out var type))
            {
                return type;
            }
            else
            {
                return typeof(BaseDocument);
            }
        }

        protected string Scope => Guid.NewGuid().ToString("N");

        protected ClusterArgs GetArgs(string method, bool allowAutomaticIndexCreation = true)
        {
            var scope = $"{Scope}_{method}".ToLower();
            return new ClusterArgs(scope, Url, Username, Password, true, null, allowAutomaticIndexCreation);
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
}
