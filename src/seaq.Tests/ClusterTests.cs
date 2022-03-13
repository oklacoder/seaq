using seaq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SEAQ.Tests
{

    public class ClusterTests :
        TestModule
    {
        public ClusterTests(
            ITestOutputHelper testOutput) :
            base(testOutput)
        {

        }

        [Fact]
        public async void CanPingCluster()
        {
            const string method = "CanPingCluster";
            //GetArgs(method)
            var cluster = Cluster.Create(GetArgs(method));
            var clusterAsync = await Cluster.CreateAsync(GetArgs(method));

            var sync_sync = cluster.CanPing();
            var sync_async = await cluster.CanPingAsync();
            var async_sync = cluster.CanPing();
            var async_async = await cluster.CanPingAsync();

            DecomissionCluster(cluster);
            DecomissionCluster(clusterAsync);

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

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.True(exists);
            Assert.True(existsByType);
            Assert.True(hasAlias.IsValid);
            Assert.NotEmpty(hasAlias.Records);
        }
        [Fact]
        public async void CanCreateIndex_WithIndexAsType()
        {
            const string method = "CanCreateIndex";
            const string test_alias = "test_alias";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc1).FullName;
            var t0 = typeof(TestDoc).FullName;

            var c0 = new IndexConfig(t0, t0);
            _ = await cluster.CreateIndexAsync(c0);

            var config = new IndexConfig(type, type, indexAsType: t0);
            var resp = await cluster.CreateIndexAsync(config);

            Assert.NotNull(resp);
            Assert.NotNull(resp.IndexAsType);

            var exists = cluster.Indices.Any(x => x.Name == config.Name);
            var existsByType = cluster.IndicesByType[type]?.Any();

            var indexAs_cluster = cluster.Indices.FirstOrDefault(x => x.Name.Equals(resp.Name)).IndexAsType;
            var indexAs_resp = resp.IndexAsType;

            DecomissionCluster(cluster);

            Assert.True(exists);
            Assert.True(existsByType);
            Assert.Equal(t0, indexAs_cluster);
            Assert.Equal(t0, indexAs_resp);
        }
        [Fact]
        public async void CreateClusterIncludesIndexAsIndices()
        {
            const string method = "CreateClusterIncludesIndexAsIndices";

            var clusterArgs = GetArgs(method);
            var cluster = await Cluster.CreateAsync(clusterArgs);

            var type = typeof(TestDoc1).FullName;
            var t0 = typeof(TestDoc).FullName;

            var c0 = new IndexConfig(t0, t0);
            _ = await cluster.CreateIndexAsync(c0);

            var config = new IndexConfig(type, type, indexAsType: t0);
            var resp = await cluster.CreateIndexAsync(config);

            Assert.NotNull(resp);

            var cluster2 = await Cluster.CreateAsync(clusterArgs);

            var c1Count = cluster.Indices.Count();
            var c2Count = cluster2.Indices.Count();

            DecomissionCluster(cluster);
            DecomissionCluster(cluster2);

            Assert.Equal(c1Count, c2Count);
        }
        [Fact]
        public async void CanCreateIndex_WithIndexAsType_FailsWhenMappingTypeDoesntExist()
        {
            const string method = "CanCreateIndex";
            const string test_alias = "test_alias";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc1).FullName;
            var t0 = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type, new[] { test_alias, test_alias }, indexAsType: t0);
            var resp = await cluster.CreateIndexAsync(config);

            DecomissionCluster(cluster);

            Assert.Null(resp);
        }
        [Fact]
        public async void CanCreateIndex_WithIndexAsType_FailsWhenDocTypeDoesntImplementMappingType()
        {
            const string method = "CanCreateIndex";
            const string test_alias = "test_alias";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc1).FullName;
            var t0 = typeof(TestDoc2).FullName;

            var config0 = new IndexConfig(t0, t0, new[] { test_alias, test_alias });
            var resp0 = await cluster.CreateIndexAsync(config0);
            var config = new IndexConfig(type, type, new[] { test_alias, test_alias }, indexAsType: t0);
            var resp = await cluster.CreateIndexAsync(config);

            DecomissionCluster(cluster);

            Assert.Null(resp);
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

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.True(exists);
            Assert.True(existsByType);
            Assert.True(delResp);
            Assert.False(stillExists);
            Assert.False(stillExistsByType);
        }
        [Fact]
        public async void DeleteIndex_DeletesDependantIndices()
        {
            const string method = "DeleteIndex_DeletesDependantIndices";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type0 = typeof(TestDoc).FullName;
            var type = typeof(TestDoc1).FullName;

            var config0 = new IndexConfig(type0, type0);
            var resp0 = await cluster.CreateIndexAsync(config0);
            var config = new IndexConfig(type, type, indexAsType: type0);
            var resp = await cluster.CreateIndexAsync(config);

            var exists = cluster.Indices.Any(x => x.Name == config.Name);
            var existsByType = cluster.IndicesByType[type]?.Any();

            //delete it here
            var delResp = await cluster.DeleteIndexAsync(config0.Name);

            var stillExists = cluster.Indices.Any(x => x.Name == config.Name);
            var stillExistsByType = cluster.IndicesByType[type]?.Any();

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.True(exists);
            Assert.True(existsByType);
            Assert.True(delResp);
            Assert.False(stillExists);
            Assert.False(stillExistsByType);
        }
        [Fact]
        public async void DeleteIndex_DeletesIndexAsTypeIndex()
        {
            const string method = "DeleteIndex_DeletesIndexAsTypeIndex";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type0 = typeof(TestDoc).FullName;
            var type = typeof(TestDoc1).FullName;

            var config0 = new IndexConfig(type0, type0);
            var resp0 = await cluster.CreateIndexAsync(config0);
            var config = new IndexConfig(type, type, indexAsType: type0);
            var resp = await cluster.CreateIndexAsync(config);

            var exists = cluster.Indices.Any(x => x.Name == config.Name);
            var existsByType = cluster.IndicesByType[type]?.Any();

            //delete it here
            var delResp = await cluster.DeleteIndexAsync(config.Name);

            var stillExists = cluster.Indices.Any(x => x.Name == config.Name);
            var stillExistsByType = cluster.IndicesByType[type]?.Any();

            DecomissionCluster(cluster);

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

            var doc = GetFakeDocs<TestDoc>(1).FirstOrDefault();

            var resp = await cluster.CommitAsync(doc);

            DecomissionCluster(cluster);

            Assert.True(resp);
        }
        [Fact]
        public async void CanIndexOneDocumentAsOtherType()
        {
            const string method = "CanIndexOneDocument";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc1).FullName;
            var t0 = typeof(TestDoc).FullName;

            var c0 = new IndexConfig(t0, t0);
            var idx0 = await cluster.CreateIndexAsync(c0);
            Assert.NotNull(idx0);
            var config = new IndexConfig(type, type, indexAsType: typeof(TestDoc).FullName);
            var idx = await cluster.CreateIndexAsync(config);
            Assert.NotNull(idx);

            var doc = GetFakeDocs<TestDoc1>(1).FirstOrDefault();

            var resp = await cluster.CommitAsync(doc);

            var exists = _client.Search<TestDoc1>(x => x.Index(idx0.Name).Query(q => q.MatchAll()));

            DecomissionCluster(cluster);

            Assert.True(resp);
            Assert.NotNull(exists);
            Assert.NotEmpty(exists.Documents);
        }
        [Fact]
        public async void CanIndexOneDocument_Untyped()
        {
            const string method = "CanIndexOneDocument_Untyped";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var doc = GetFakeDocs<TestDoc>(1).FirstOrDefault();

            var resp = await cluster.CommitAsync(doc as object);

            DecomissionCluster(cluster);

            Assert.True(resp);
        }
        [Fact]
        public async void CanIndexOneDocument_Untyped_FailsWhenBadType()
        {
            const string method = "CanIndexOneDocument_Untyped_FailsWhenBadType";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var doc = new { value1 = "test", value2 = 2 };

            var resp = await cluster.CommitAsync(doc as object);

            DecomissionCluster(cluster);

            Assert.False(resp);
        }
        [Fact]
        public async void CanIndexOneDocument_AutomaticIndexCreation()
        {
            const string method = "CanIndexOneDocument";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);

            var doc = GetFakeDocs<TestDoc>(1).FirstOrDefault();

            var resp = await cluster.CommitAsync(doc);

            DecomissionCluster(cluster);

            Assert.True(resp);
        }
        [Fact]
        public async void CanIndexOneDocument_FailsWhenIndexNotExists()
        {
            const string method = "CanIndexOneDocument";
            var cluster = await Cluster.CreateAsync(GetArgs(method, false));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);

            var doc = GetFakeDocs<TestDoc>(1).FirstOrDefault();

            var resp = await cluster.CommitAsync(doc);

            DecomissionCluster(cluster);

            Assert.False(resp);
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

            var docs = GetFakeDocs<TestDoc>(100);

            var resp = await cluster.CommitAsync(docs);

            DecomissionCluster(cluster);

            Assert.True(resp);
        }
        [Fact]
        public async void CanIndex100Documents_AsOtherType()
        {
            const string method = "CanIndex100Documents";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;
            var type1 = typeof(TestDoc1).FullName;

            var config0 = new IndexConfig(type, type);
            var idx0 = await cluster.CreateIndexAsync(config0);
            var config1 = new IndexConfig(type1, type1, indexAsType: type);
            var idx1 = await cluster.CreateIndexAsync(config1);

            var docs = GetFakeDocs<TestDoc1>(100);

            var resp = await cluster.CommitAsync(docs);
            var exists = _client.Search<TestDoc1>(x => x.Index(idx0.Name).Query(q => q.MatchAll()));

            DecomissionCluster(cluster);

            Assert.True(resp);
            Assert.NotNull(exists);
            Assert.NotEmpty(exists.Documents);
        }
        [Fact]
        public async void CanIndex100Documents_Untyped_AsOtherType()
        {
            const string method = "CanIndex100Documents";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;
            var type1 = typeof(TestDoc1).FullName;

            var config0 = new IndexConfig(type, type);
            var idx0 = await cluster.CreateIndexAsync(config0);
            var config1 = new IndexConfig(type1, type1, indexAsType: type);
            var idx1 = await cluster.CreateIndexAsync(config1);

            var docs = GetFakeDocs<TestDoc1>(100);

            var resp = await cluster.CommitAsync(docs.Select(x => x as object));
            var exists = _client.Search<TestDoc1>(x => x.Index(idx0.Name).Query(q => q.MatchAll()));

            DecomissionCluster(cluster);

            Assert.True(resp);
            Assert.NotNull(exists);
            Assert.NotEmpty(exists.Documents);
        }
        [Fact]
        public async void CanIndex100Documents_Untyped()
        {
            const string method = "CanIndex100Documents_Untyped";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc>(100);

            var resp = await cluster.CommitAsync(docs.Select(x => x as object));

            DecomissionCluster(cluster);

            Assert.True(resp);
        }

        [Fact]
        public async void CanIndex100Documents_Untyped_FailsWhenBadType()
        {
            const string method = "CanIndex100Documents_UntypedFailsWhenBadType";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = new[] { new { value1 = "test", value2 = 2 } };

            var resp = await cluster.CommitAsync(docs.Select(x => x as object));

            DecomissionCluster(cluster);

            Assert.False(resp);
        }
        [Fact(Skip = "performance")]
        public async void CanIndex10000Documents()
        {
            const string method = "CanIndex10000Documents";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc>(10000);

            var resp = await cluster.CommitAsync(docs);

            DecomissionCluster(cluster);

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

            DecomissionCluster(cluster);

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

            var docs = GetFakeDocs<TestDoc>(1000);

            foreach (var d in docs)
            {
                d.IndexName = config.Name;
            }

            await cluster.CommitAsync(docs);

            var resp = await cluster.CopyIndexAsync(config.Name, name2, true);

            var exists = cluster.Indices.Any(x => x.Name == name2);
            var existsByType = cluster.IndicesByType[type]?.Any(x => x.Name.Equals(name2));

            DecomissionCluster(cluster);

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

            DecomissionCluster(cluster);

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

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.Equal(testLabel, actual);
        }
        [Fact]
        public async void CanUpdateIndexField()
        {
            const string method = "CanUpdateIndexField";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            var fName = nameof(TestDoc.StringValue);
            var f = createResp.Fields.FirstOrDefault(x => x.Name.Equals(fName, StringComparison.OrdinalIgnoreCase));

            f.Label = method;

            var resp = await cluster.UpdateIndexFieldAsync(createResp.Name, f);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(idx);
            Assert.Equal(resp.Fields.FirstOrDefault(x => x.Name.Equals(fName, StringComparison.OrdinalIgnoreCase))?.Label, method);
            Assert.Equal(idx.Fields.FirstOrDefault(x => x.Name.Equals(fName, StringComparison.OrdinalIgnoreCase))?.Label, method);
        }

        //deprecate index
        [Fact]
        public async void CanDeprecateIndex()
        {
            const string method = "CanDeprecateIndex";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            const string depMsg = "Test Deprecation Message";
            var resp = await cluster.DeprecateIndexAsync(createResp.Name, depMsg);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.Equal(resp.IsDeprecated, idx.IsDeprecated);
            Assert.Equal(resp.DeprecationMessage, idx.DeprecationMessage);
            Assert.True(idx.IsDeprecated);
            Assert.Equal(idx.DeprecationMessage, depMsg);
        }
        [Fact]
        public async void CanUnDeprecateIndex()
        {
            const string method = "CanUnDeprecateIndex";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            const string depMsg = "Test Deprecation Message";
            await cluster.DeprecateIndexAsync(createResp.Name, depMsg);
            var resp = await cluster.UnDeprecateIndexAsync(createResp.Name);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.False(idx.IsDeprecated);
            Assert.Null(idx.DeprecationMessage);
            Assert.False(resp.IsDeprecated);
            Assert.Null(resp.DeprecationMessage);
        }
        //force refresh on commit
        [Fact]
        public async void CanForceRefreshOnDocumentCommit()
        {
            const string method = "CanForceRefreshOnDocumentCommit";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            var resp = await cluster.ForceRefreshOnDocumentCommitAsync(createResp.Name);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.True(idx.ForceRefreshOnDocumentCommit);
            Assert.True(resp.ForceRefreshOnDocumentCommit);
        }

        [Fact]
        public async void DoNotForceRefreshOnDocumentCommit()
        {
            const string method = "DoNotForceRefreshOnDocumentCommit";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            var resp = await cluster.DoNotForceRefreshOnDocumentCommitAsync(createResp.Name);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.False(idx.ForceRefreshOnDocumentCommit);
            Assert.False(resp.ForceRefreshOnDocumentCommit);
        }
        //hide index
        [Fact]
        public async void CanHideIndex()
        {
            const string method = "CanHideIndex";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            var resp = await cluster.HideIndexAsync(createResp.Name);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.True(idx.IsHidden);
            Assert.True(resp.IsHidden);
        }
        [Fact]
        public async void CanUnHideIndex()
        {
            const string method = "CanUnHideIndex";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            await cluster.HideIndexAsync(createResp.Name);
            var resp = await cluster.UnHideIndexAsync(createResp.Name);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.False(idx.IsHidden);
            Assert.False(resp.IsHidden);
        }
        //toggle inclusion in global search results
        [Fact]
        public async void CanIncludeInGlobalSearch()
        {
            const string method = "CanIncludeInGlobalSearch";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            var resp = await cluster.IncludeIndexInGlobalSearchAsync(createResp.Name);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.True(idx.ReturnInGlobalSearch);
            Assert.True(resp.ReturnInGlobalSearch);
        }
        [Fact]
        public async void CanExcludeFromGlobalSearch()
        {
            const string method = "CanExcludeFromGlobalSearch";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            await cluster.HideIndexAsync(createResp.Name);
            var resp = await cluster.ExcludeIndexFromGlobalSearchAsync(createResp.Name);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.False(idx.ReturnInGlobalSearch);
            Assert.False(resp.ReturnInGlobalSearch);
        }
        [Fact]
        public async void CanSetObjectLabel()
        {
            const string method = "CanSetObjectLabel";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            await cluster.HideIndexAsync(createResp.Name);
            var resp = await cluster.SetIndexObjectLabelAsync(createResp.Name, method);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.Equal(resp.ObjectLabel, idx.ObjectLabel);
            Assert.Equal(idx.ObjectLabel, method);
        }
        [Fact]
        public async void CanSetObjectLabelPlural()
        {
            const string method = "CanSetObjectLabelPlural";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            await cluster.HideIndexAsync(createResp.Name);
            var resp = await cluster.SetIndexObjectLabelPluralAsync(createResp.Name, method);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.Equal(resp.ObjectLabelPlural, idx.ObjectLabelPlural);
            Assert.Equal(idx.ObjectLabelPlural, method);
        }
        [Fact]
        public async void CanSetIndexPrimaryField()
        {
            const string method = "CanSetIndexPrimaryField";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            await cluster.HideIndexAsync(createResp.Name);
            var resp = await cluster.SetIndexPrimaryFieldAsync(createResp.Name, method);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.Equal(resp.PrimaryField, idx.PrimaryField);
            Assert.Equal(idx.PrimaryField, method);
        }
        [Fact]
        public async void CanSetIndexPrimaryFieldLabel()
        {
            const string method = "CanSetIndexPrimaryFieldLabel";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            await cluster.HideIndexAsync(createResp.Name);
            var resp = await cluster.SetIndexPrimaryFieldLabelAsync(createResp.Name, method);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.Equal(resp.PrimaryFieldLabel, idx.PrimaryFieldLabel);
            Assert.Equal(idx.PrimaryFieldLabel, method);
        }
        [Fact]
        public async void CanSetIndexSecondaryField()
        {
            const string method = "CanSetIndexSecondaryField";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            await cluster.HideIndexAsync(createResp.Name);
            var resp = await cluster.SetIndexSecondaryFieldAsync(createResp.Name, method);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.Equal(resp.SecondaryField, idx.SecondaryField);
            Assert.Equal(idx.SecondaryField, method);
        }
        [Fact]
        public async void CanSetIndexSecondaryFieldLabel()
        {
            const string method = "CanSetIndexSecondaryFieldLabel";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            await cluster.HideIndexAsync(createResp.Name);
            var resp = await cluster.SetIndexSecondaryFieldLabelAsync(createResp.Name, method);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.Equal(resp.SecondaryFieldLabel, idx.SecondaryFieldLabel);
            Assert.Equal(idx.SecondaryFieldLabel, method);
        }
        [Fact]
        public async void CanSetIndexMeta()
        {
            const string method = "CanSetIndexMeta";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            await cluster.HideIndexAsync(createResp.Name);
            var meta = new Dictionary<string, object>();
            meta.Add("1", method);
            meta.Add("2", DateTime.Now);
            meta.Add("3", 123);
            var resp = await cluster.SetIndexMetaAsync(createResp.Name, meta);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(idx);
            Assert.NotNull(idx.Meta);
            Assert.NotNull(resp.Meta);
            foreach (var k in meta.Keys)
            {
                Assert.True(idx.Meta.ContainsKey(k));
                Assert.True(resp.Meta.ContainsKey(k));
                Assert.Equal(idx.Meta[k].ToString(), meta[k].ToString());
                Assert.Equal(resp.Meta[k].ToString(), meta[k].ToString());
            }
        }
        [Fact]
        public async void CanDeleteIndexMeta()
        {
            const string method = "CanDeleteIndexMeta";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            await cluster.HideIndexAsync(createResp.Name);
            var meta = new Dictionary<string, object>();
            meta.Add("1", method);
            meta.Add("2", DateTime.Now);
            meta.Add("3", 123);
            var resp = await cluster.SetIndexMetaAsync(createResp.Name, meta);

            var resp2 = await cluster.DeleteIndexMetaAsync(createResp.Name);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp);
            Assert.NotNull(idx);
            Assert.NotNull(idx.Meta);
            Assert.NotNull(resp.Meta);
            Assert.NotNull(resp2.Meta);
            Assert.Empty(resp2.Meta);
        }
        [Fact]
        public async void CanReplaceIndexMeta()
        {
            const string method = "CanReplaceIndexMeta";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            await cluster.HideIndexAsync(createResp.Name);
            var meta = new Dictionary<string, object>();
            meta.Add("1", method);
            meta.Add("2", DateTime.Now);
            meta.Add("3", 123);
            var resp = await cluster.SetIndexMetaAsync(createResp.Name, meta);
            var meta2 = new Dictionary<string, object>();
            meta2.Add("4", method);
            meta2.Add("5", DateTime.Now);
            meta2.Add("6", 123);
            var resp2 = await cluster.ReplaceIndexMetaAsync(createResp.Name, meta2);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp2);
            Assert.NotNull(idx);
            Assert.NotNull(idx.Meta);
            Assert.NotNull(resp2.Meta);
            foreach (var k in meta2.Keys)
            {
                Assert.True(idx.Meta.ContainsKey(k));
                Assert.True(resp2.Meta.ContainsKey(k));
                Assert.Equal(idx.Meta[k].ToString(), meta2[k].ToString());
                Assert.Equal(resp2.Meta[k].ToString(), meta2[k].ToString());
            }
        }
        [Fact]
        public async void CanAppendIndexMeta()
        {
            const string method = "CanAppendIndexMeta";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            var createResp = await cluster.CreateIndexAsync(config);

            await cluster.HideIndexAsync(createResp.Name);
            var meta = new Dictionary<string, object>();
            meta.Add("1", method);
            meta.Add("2", DateTime.Now);
            meta.Add("3", 123);
            var resp = await cluster.SetIndexMetaAsync(createResp.Name, meta);
            var meta2 = new Dictionary<string, object>();
            meta2.Add("4", method);
            meta2.Add("5", DateTime.Now);
            meta2.Add("6", 123);
            var resp2 = await cluster.AppendIndexMetaAsync(createResp.Name, meta2);

            var combined = new Dictionary<string, object>();
            foreach (var m in meta)
                combined.Add(m.Key, m.Value);
            foreach (var m in meta2)
                combined.Add(m.Key, m.Value);

            var idx = await cluster.GetIndexDefinitionAsync(createResp.Name);// cluster.Indices.FirstOrDefault(x => x.Name.Equals(createResp.Name));

            DecomissionCluster(cluster);

            Assert.NotNull(resp2);
            Assert.NotNull(idx);
            Assert.NotNull(idx.Meta);
            Assert.NotNull(resp2.Meta);
            foreach (var k in combined.Keys)
            {
                Assert.True(idx.Meta.ContainsKey(k));
                Assert.True(resp2.Meta.ContainsKey(k));
                Assert.Equal(idx.Meta[k].ToString(), combined[k].ToString());
                Assert.Equal(resp2.Meta[k].ToString(), combined[k].ToString());
            }
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

            var docs = GetFakeDocs<TestDoc>(100).ToList();

            await cluster.CommitAsync(docs);

            var toDelete = docs.ElementAt(5);

            var resp = await cluster.DeleteAsync(toDelete);

            DecomissionCluster(cluster);

            Assert.True(resp);
        }
        [Fact]
        public async void CanDelete1Document_IndexAsType()
        {
            const string method = "CanDelete1Document";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type0 = typeof(TestDoc).FullName;
            var type = typeof(TestDoc1).FullName;

            var config0 = new IndexConfig(type0, type0);
            var r = await cluster.CreateIndexAsync(config0);
            var config = new IndexConfig(type, type, indexAsType: type0);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc1>(100).ToList();

            await cluster.CommitAsync(docs);

            var toDelete = docs.ElementAt(5);

            var resp = await cluster.DeleteAsync(toDelete);

            var stillExists = _client.Search<TestDoc1>(x => x.Index(r.Name).Query(q => q.Ids(i => i.Values(toDelete.Id))));

            DecomissionCluster(cluster);

            Assert.True(resp);
            Assert.Empty(stillExists.Documents);
        }
        [Fact]
        public async void CanDelete1Document_Untyped_IndexAsType()
        {
            const string method = "CanDelete1Document";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type0 = typeof(TestDoc).FullName;
            var type = typeof(TestDoc1).FullName;

            var config0 = new IndexConfig(type0, type0);
            var r = await cluster.CreateIndexAsync(config0);
            var config = new IndexConfig(type, type, indexAsType: type0);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc1>(100).ToList();

            await cluster.CommitAsync(docs);
            var toDelete = docs.ElementAt(5);
            var obj = toDelete as object;

            var resp = await cluster.DeleteAsync(obj);

            var stillExists = _client.Search<TestDoc1>(x => x.Index(r.Name).Query(q => q.Ids(i => i.Values(toDelete.Id))));

            DecomissionCluster(cluster);

            Assert.True(resp);
            Assert.Empty(stillExists.Documents);
        }
        //delete single doc
        [Fact]
        public async void CanDelete1Document_Untyped()
        {
            const string method = "CanDelete1Document_Untyped";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc>(100).ToList();

            await cluster.CommitAsync(docs);

            var toDelete = docs.ElementAt(5) as object;

            var resp = await cluster.DeleteAsync(toDelete);

            DecomissionCluster(cluster);

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

            var docs = GetFakeDocs<TestDoc>(100).ToList();

            await cluster.CommitAsync(docs);

            var toDelete = docs.Take(5);

            var resp = await cluster.DeleteAsync(toDelete);

            DecomissionCluster(cluster);

            Assert.True(resp);
        }
        [Fact]
        public async void CanDelete5Documents_IndexAsType()
        {
            const string method = "CanDelete5Documents";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type0 = typeof(TestDoc).FullName;
            var type = typeof(TestDoc1).FullName;

            var config0 = new IndexConfig(type0, type0);
            var r = await cluster.CreateIndexAsync(config0);
            var config = new IndexConfig(type, type, indexAsType: type0);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc1>(100).ToList();

            await cluster.CommitAsync(docs);

            var toDelete = docs.Take(5);

            var resp = await cluster.DeleteAsync(toDelete);

            var stillExists = _client.Search<TestDoc1>(x => x.Index(r.Name).Query(q => q.Ids(i => i.Values(toDelete.Select(p => p.Id)))));

            DecomissionCluster(cluster);

            Assert.True(resp);
            Assert.Empty(stillExists.Documents);
        }
        [Fact]
        public async void CanDelete5Documents_Untyped_IndexAsType()
        {
            const string method = "CanDelete5Documents";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type0 = typeof(TestDoc).FullName;
            var type = typeof(TestDoc1).FullName;

            var config0 = new IndexConfig(type0, type0);
            var r = await cluster.CreateIndexAsync(config0);
            var config = new IndexConfig(type, type, indexAsType: type0);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc1>(100).ToList();

            await cluster.CommitAsync(docs);

            var toDelete = docs.Take(5);

            var resp = await cluster.DeleteAsync(toDelete.Select(x => x as object));

            var stillExists = _client.Search<TestDoc1>(x => x.Index(r.Name).Query(q => q.Ids(i => i.Values(toDelete.Select(p => p.Id)))));

            DecomissionCluster(cluster);

            Assert.True(resp);
            Assert.Empty(stillExists.Documents);
        }
        [Fact]
        public async void CanDelete5Documents_Untyped()
        {
            const string method = "CanDelete5Documents_Untyped";
            var cluster = await Cluster.CreateAsync(GetArgs(method));

            var type = typeof(TestDoc).FullName;

            var config = new IndexConfig(type, type);
            await cluster.CreateIndexAsync(config);

            var docs = GetFakeDocs<TestDoc>(100).ToList();

            await cluster.CommitAsync(docs);

            var toDelete = docs.Take(5).Select(x => x as object);

            var resp = await cluster.DeleteAsync(toDelete);

            DecomissionCluster(cluster);

            Assert.True(resp);
        }

    }
}
