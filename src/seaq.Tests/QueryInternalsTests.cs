using seaq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace seaq.Tests
{
    public class QueryInternalsTests :
        TestModule
    {
        public QueryInternalsTests(
            ITestOutputHelper testOutput) :
            base(testOutput)
        {

        }


        //can query created things

        [Fact]
        public async void CanQuery()
        {
            const string method = "CanQuery";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc>(100);

            await cluster.CommitAsync(docs);

            var qargs = new SimpleQueryCriteria<TestDoc>(docs.First().StringValue);
            var q = new SimpleQuery<TestDoc>(qargs);

            var resp = await cluster.QueryAsync(q);

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(resp.Results);
            Assert.NotEmpty(resp.Results);
        }
        [Fact]
        public async void CanQuery_CaseInsensitive_Upper()
        {
            const string method = "CanQuery_CaseInsensitive";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc>(100);

            await cluster.CommitAsync(docs);

            var qargs = new SimpleQueryCriteria<TestDoc>(docs.First().StringValue.ToUpper());
            var q = new SimpleQuery<TestDoc>(qargs);

            var resp = await cluster.QueryAsync(q);

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(resp.Results);
            Assert.NotEmpty(resp.Results);
        }
        [Fact]
        public async void CanQuery_CaseInsensitive_Lower()
        {
            const string method = "CanQuery_CaseInsensitive";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc>(100);

            await cluster.CommitAsync(docs);

            var qargs = new SimpleQueryCriteria<TestDoc>(docs.First().StringValue.ToLower());
            var q = new SimpleQuery<TestDoc>(qargs);

            var resp = await cluster.QueryAsync(q);

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(resp.Results);
            Assert.NotEmpty(resp.Results);
        }
        [Fact]
        public async void CanQuery_CaseInsensitive_Spongebob()
        {
            const string method = "CanQuery_CaseInsensitive";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc>(100);

            await cluster.CommitAsync(docs);

            IEnumerable<string> sillyLetters = new List<string>();
            var val = docs.First().StringValue;

            sillyLetters = val.ToArray().Select((l, i) =>
            {
                return i % 2 == 0 ? l.ToString().ToUpper() : l.ToString().ToLower();
            });
            var queryVal = string.Join("", sillyLetters);

            var qargs = new SimpleQueryCriteria<TestDoc>(queryVal);
            var q = new SimpleQuery<TestDoc>(qargs);

            var resp = await cluster.QueryAsync(q);

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(resp.Results);
            Assert.NotEmpty(resp.Results);
        }
        //prefix ('smi' returns 'smith h' and 'smith 123 h' and 'smithson')
        //partial ('smith' returns 'smith h' and 'smith 123 h' and then 'smithson')
        //partial ('smith ' returns 'smith h' and 'smith 123 h' and not 'smithson')
        //parts ('smith 12' returns 'smith 123 h' only, not 'smith' or 'smithson'
        //separated parts ('smith h' returns 'smith h' and then 'smith 123 h' and _not_ 'smithson')
        [Fact]
        public async void CanQuery_Partial_Prefix()
        {
            const string method = "CanQuery_CaseInsensitive";
            const string queryVal = "smi";
            const int expectedQty = 3;
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc>(100).ToList();

            docs.ForEach(x => x.StringValue = Guid.Empty.ToString());
            docs.ElementAt(0).StringValue = "Smith M H";
            docs.ElementAt(1).StringValue = "Smith M 123 H";
            docs.ElementAt(2).StringValue = "Smithson";

            await cluster.CommitAsync(docs);

            IEnumerable<string> sillyLetters = new List<string>();


            var qargs = new SimpleQueryCriteria<TestDoc>(queryVal);
            var q = new SimpleQuery<TestDoc>(qargs);

            var resp = await cluster.QueryAsync(q);

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(resp.Results);
            Assert.NotEmpty(resp.Results);

            var doc1 = resp.Results.Any(x => x.Id.Equals(docs.ElementAt(0).Id));
            var doc2 = resp.Results.Any(x => x.Id.Equals(docs.ElementAt(1).Id));
            var doc3 = resp.Results.Any(x => x.Id.Equals(docs.ElementAt(2).Id));

            Assert.Equal(expectedQty, resp.Total);
            Assert.True(doc1);
            Assert.True(doc2);
            Assert.True(doc3);

        }
        [Fact]
        public async void CanQuery_Partial_NoTrailingSpace()
        {
            const string method = "CanQuery_CaseInsensitive";
            const string queryVal = "smith";
            const int expectedQty = 3;
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc>(100).ToList();

            docs.ForEach(x => x.StringValue = Guid.Empty.ToString());
            docs.ElementAt(0).StringValue = "Smith M H";
            docs.ElementAt(1).StringValue = "Smith M 123 H";
            docs.ElementAt(2).StringValue = "Smithson";

            await cluster.CommitAsync(docs);

            IEnumerable<string> sillyLetters = new List<string>();


            var qargs = new SimpleQueryCriteria<TestDoc>(queryVal);
            var q = new SimpleQuery<TestDoc>(qargs);

            var resp = await cluster.QueryAsync(q);

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(resp.Results);
            Assert.NotEmpty(resp.Results);

            var doc1 = resp.Results.Any(x => x.Id.Equals(docs.ElementAt(0).Id));
            var doc2 = resp.Results.Any(x => x.Id.Equals(docs.ElementAt(1).Id));
            var doc3 = resp.Results.Any(x => x.Id.Equals(docs.ElementAt(2).Id));

            Assert.Equal(expectedQty, resp.Total);
            Assert.True(doc1);
            Assert.True(doc2);
            Assert.True(doc3);

        }
        [Fact]
        public async void CanQuery_Partial_HasTrailingSpace()
        {
            const string method = "CanQuery_CaseInsensitive";
            const string queryVal = "smith ";
            const int expectedQty = 2;
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc>(100).ToList();

            docs.ForEach(x => x.StringValue = Guid.Empty.ToString());
            docs.ElementAt(0).StringValue = "Smith M H";
            docs.ElementAt(1).StringValue = "Smith M 123 H";
            docs.ElementAt(2).StringValue = "Smithson";

            await cluster.CommitAsync(docs);

            IEnumerable<string> sillyLetters = new List<string>();


            var qargs = new SimpleQueryCriteria<TestDoc>(queryVal);
            var q = new SimpleQuery<TestDoc>(qargs);

            var resp = await cluster.QueryAsync(q);

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(resp.Results);
            Assert.NotEmpty(resp.Results);

            var doc1 = resp.Results.Any(x => x.Id.Equals(docs.ElementAt(0).Id));
            var doc2 = resp.Results.Any(x => x.Id.Equals(docs.ElementAt(1).Id));
            var doc3 = resp.Results.Any(x => x.Id.Equals(docs.ElementAt(2).Id));

            Assert.Equal(expectedQty, resp.Total);
            Assert.True(doc1);
            Assert.True(doc2);
            Assert.False(doc3);
        }
        [Fact]
        public async void CanQuery_Partial_HasParts()
        {
            const string method = "CanQuery_CaseInsensitive";
            const string queryVal = "smith 123";
            const int expectedQty = 2;
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc>(100).ToList();

            docs.ForEach(x => x.StringValue = Guid.Empty.ToString());
            docs.ElementAt(0).StringValue = "Smith M H";
            docs.ElementAt(1).StringValue = "Smith M 123 H";
            docs.ElementAt(2).StringValue = "Smithson";

            await cluster.CommitAsync(docs);

            IEnumerable<string> sillyLetters = new List<string>();


            var qargs = new SimpleQueryCriteria<TestDoc>(queryVal);
            var q = new SimpleQuery<TestDoc>(qargs);

            var resp = await cluster.QueryAsync(q);

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(resp.Results);
            Assert.NotEmpty(resp.Results);

            var doc1 = resp.Results.Any(x => x.Id.Equals(docs.ElementAt(0).Id));
            var doc2 = resp.Results.Any(x => x.Id.Equals(docs.ElementAt(1).Id));
            var doc3 = resp.Results.Any(x => x.Id.Equals(docs.ElementAt(2).Id));

            Assert.Equal(expectedQty, resp.Total);
            Assert.True(doc1);
            Assert.True(doc2);
            Assert.False(doc3);

            Assert.Equal(resp.Results.ElementAt(0).Id, docs.ElementAt(1).Id);
            Assert.Equal(resp.Results.ElementAt(1).Id, docs.ElementAt(0).Id);
        }
    }
}
