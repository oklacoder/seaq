using seaq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SEAQ.Tests
{

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
            const string test_alias = "test_alias";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type, new[] { test_alias, test_alias });
            var resp = await cluster.CreateIndexAsync(config);

            var exists = cluster.Indices.Any(x => x.Name == config.Name);
            var existsByType = cluster.IndicesByType[type]?.Any();

            var hasAlias = _client.Cat.Aliases(x => x.Name(test_alias));

            var delResp = await cluster.DeleteIndexAsync(config.Name);

            Assert.NotNull(resp);
            Assert.True(exists);
            Assert.True(existsByType);
            Assert.True(hasAlias.IsValid);
            Assert.NotEmpty(hasAlias.Records);
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
        [Fact(Skip = "performance")]
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
            Assert.Equal(resp.DocumentType, config.DocumentType);
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
            Assert.Equal(resp.DocumentType, config.DocumentType);
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
            var existsByType = cluster.IndicesByType[resp.DocumentType]?.Any();

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

            const string testLabel = "TestLabel";
            var fields = createResp.Fields.ToList();
            var f = fields.FirstOrDefault();
            var idx = fields.ToList().FindIndex(x => x.Name.Equals(f.Name));
            fields[idx].Label = testLabel;

            createResp.Fields = fields;            

            var resp = await cluster.UpdateIndexDefinitionAsync(createResp);

            var actual = resp.Fields.FirstOrDefault(x => x.Name.Equals(f.Name))?.Label;

            await cluster.DeleteIndexAsync(config.Name);

            Assert.NotNull(resp);
            Assert.Equal(testLabel, actual);
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
}
