using System;
using Xunit;
using Seaq.Elasticsearch.Clusters;
using Seaq.Elasticsearch.Stores;
using System.Linq;
using Seaq.Elasticsearch.Queries;
using Seaq.Elasticsearch.Documents;

namespace Kitchenbuddy.Dialect.Model
{
    public class Ingredient :
        IDocument
    {
        public Ingredient()
        {
            Tags = new Tag[] { };
        }

        public string Name { get; set; }
        public Tag[] Tags { get; set; }


        public string Id { get; set; }

        public string DocumentId => Id;

        public string ScopeId { get; set; }

        public string StoreId =>
            new Seaq.Elasticsearch.Stores.StoreId(
                ScopeId,
                GetType().FullName).Name;

        public string Type => GetType().FullName;

        public string PrimaryDisplay => Name;

        public string SecondaryDisplay => null;

        public string[] Suggestions =>
            new string[]
            {
                Name
            }
            .Concat(Tags.Select(x => x.Value))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        public void EnsureScopeId(
            string scopeId)
        {
            if (string.IsNullOrWhiteSpace(this.ScopeId))
            {
                this.ScopeId = scopeId;
            }
        }

    }
    public class Tag
    {
        public string Value { get; set; }

        public Tag(
            string value)
        {
            Value = value;
        }
    }
}

namespace Seaq.Elasticsearch.Tests
{
    public class ClusterTests
    {
        private readonly string ScopeId;
        private ClusterSettings settings { get; }


        public ClusterTests()
        {
            ScopeId = this.GetType().FullName.ToLowerInvariant();
            settings = TestDataService.GetClusterSettings(ScopeId);
        }


        private Cluster CreateCluster()
        {
            return new Cluster(settings);
        }

        [Fact]
        public void Cheater()
        {
            const string appScope = "kb-local-dev";
            var ingredient = new Kitchenbuddy.Dialect.Model.Ingredient();
            var settings = new ClusterSettings("http://localhost:9200", "", "", appScope);
            var cluster = new Cluster(settings);
            var storeSettings = new CreateStoreSettings(
                typeof(Kitchenbuddy.Dialect.Model.Ingredient).FullName, 
                "kb-local-dev", 
                typeof(Kitchenbuddy.Dialect.Model.Ingredient));
            var store = cluster.CreateStore(storeSettings);

            ingredient.Id = Guid.NewGuid().ToString("N");
            ingredient.Name = "Test Ingredient";
            ingredient.Tags = new Kitchenbuddy.Dialect.Model.Tag[] 
            { 
                new Kitchenbuddy.Dialect.Model.Tag("Tag1"),
                new Kitchenbuddy.Dialect.Model.Tag("Tag2"),
                new Kitchenbuddy.Dialect.Model.Tag("Tag3"),
                new Kitchenbuddy.Dialect.Model.Tag("Tag4")
            };
            ingredient.EnsureScopeId(appScope);
            cluster.Commit(new[] { ingredient });


            var criteria = new FieldValuesQueryCriteria(
                new[] { "Kitchenbuddy.Dialect.Model.Ingredient" },
                $"{nameof(Kitchenbuddy.Dialect.Model.Ingredient.Tags)}.{nameof(Kitchenbuddy.Dialect.Model.Tag.Value)}",
                "tag1");
            var query = new FieldValuesQuery(criteria);
            var result = cluster.Query(query);

            Assert.NotNull(cluster);
        }

        
        [Fact]
        public void Can_Create_Cluster()
        {
            var cluster = CreateCluster();

            Assert.NotNull(cluster);
        }

        [Fact]
        public void Can_Ping_Cluster()
        {
            var cluster = CreateCluster();

            Assert.True(cluster.CanPing());
        }

        [Fact]
        public void Can_Get_Store_List()
        {
            var cluster = CreateCluster();

            var stores = cluster.GetScopedStoreList();

            Assert.NotNull(stores);
        }

        [Fact]
        public void Can_Create_Store()
        {
            var cluster = new Cluster(settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person));

            var store = cluster.CreateStore(storeSettings);

            cluster.DeleteStore(store.StoreId.Name);

            Assert.NotNull(store);
        }

        [Fact]
        public void Create_Store_Without_Eager_Persist_Should_Not_Update_Schema_On_Create()
        {
            var _settings = TestDataService.GetClusterSettings(ScopeId, true, false);
            var cluster = new Cluster(_settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person), 1, 2, false);

            var store = cluster.CreateStore(storeSettings);

            cluster.DeleteStore(store.StoreId.Name);

            Assert.NotNull(store);
            Assert.Null(store.StoreSchema.Fields);
        }

        [Fact]
        public void Create_Store_With_Eager_Persist_Should_Update_Schema_On_Create()
        {
            var _settings = TestDataService.GetClusterSettings(ScopeId, true, false);
            var cluster = new Cluster(_settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person));

            var store = cluster.CreateStore(storeSettings);

            cluster.DeleteStore(store.StoreId.Name);

            Assert.NotNull(store);
            Assert.NotNull(store.StoreSchema.Fields);
        }

        [Fact]
        public void SaveStoreSchema_Should_Correctly_Update_Store_Schema()
        {
            var _settings = TestDataService.GetClusterSettings(ScopeId, true, false);
            var cluster = new Cluster(_settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person));

            var store = cluster.CreateStore(storeSettings);

            var schema = store.StoreSchema;
            var fields = schema.Fields;
            
            const string newValue = "New Label Value";

            fields[0] = new StoreField(fields[0].Name, fields[0].Type, fields[0].Fields, newValue);

            var newSchema = new StoreSchema(schema.StoreId, schema.Type, fields);

            cluster.SaveStoreSchema(store.StoreId.Name, newSchema);

            var compareSchema = cluster.GetStoreSchema(store.StoreId.Name);

            var updateTook = compareSchema.Fields.FirstOrDefault()?.Label == newValue;

            cluster.DeleteStore(store.StoreId.Name);

            Assert.NotNull(store);
            Assert.True(updateTook);
        }

        [Fact]
        public void Can_Get_Store_Schema()
        {
            var _settings = TestDataService.GetClusterSettings(ScopeId, true, false);
            var cluster = new Cluster(_settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person));

            var store = cluster.CreateStore(storeSettings);

            var schema = cluster.GetStoreSchema(store.StoreId.Name);

            cluster.DeleteStore(store.StoreId.Name);

            Assert.NotNull(store);
            Assert.NotNull(schema);
            Assert.NotEmpty(schema.Fields);
        }

        [Fact]
        public void Can_Delete_Store()
        {
            var cluster = new Cluster(settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person));

            var store = cluster.CreateStore(storeSettings);

            var deleteResult = cluster.DeleteStore(store.StoreId.Name);

            Assert.True(deleteResult);
        }

        [Fact]
        public void Can_Commit_Single_Document()
        {
            var cluster = new Cluster(settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person));

            var store = cluster.CreateStore(storeSettings);

            var document = TestDataService.GetSingleFake(store.StoreId.Name);

            var commitResult = cluster.Commit(new[] { document });

            cluster.DeleteStore(store.StoreId.Name);

            Assert.Empty(commitResult);
        }

        [Fact]
        public void Can_Commit_Batched_Documents()
        {
            var cluster = new Cluster(settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person));

            var store = cluster.CreateStore(storeSettings);

            var documents = TestDataService.GetFakes(store.StoreId.Name, 100);

            var commitResult = cluster.Commit(documents);

            cluster.DeleteStore(store.StoreId.Name);

            Assert.Empty(commitResult);
        }

        [Fact]
        public void Can_Delete_Single_Document() 
        {
            var cluster = new Cluster(settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person));

            var store = cluster.CreateStore(storeSettings);

            var documents = TestDataService.GetFakes(store.StoreId.Name);
            
            var commitResult = cluster.Commit(documents);

            var deleteResult = cluster.DeleteDocuments(new[] { documents.FirstOrDefault() });

            cluster.DeleteStore(store.StoreId.Name);

            Assert.Empty(deleteResult);
        }

        [Fact]
        public void Can_Delete_Batched_Documents()
        {
            var cluster = new Cluster(settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person));

            var store = cluster.CreateStore(storeSettings);

            var documents = TestDataService.GetFakes(store.StoreId.Name, 100);

            var commitResult = cluster.Commit(documents);

            var deleteResult = cluster.DeleteDocuments(documents.Take(10).ToArray());

            cluster.DeleteStore(store.StoreId.Name);

            Assert.Empty(deleteResult);
        }

        [Fact]
        public void Can_Populate_Existing_Indices()
        {
          var cluster = new Cluster(settings);

          var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person));

          var store = cluster.CreateStore(storeSettings);

          var cluster2 = new Cluster(settings);

          cluster.DeleteStore(store.StoreId.Name);

          Assert.NotNull(cluster2);
          Assert.NotEmpty(cluster2.Stores);
        }

        [Fact]
        public void Can_Query_Populated_Indices()
        {
            var cluster = new Cluster(settings);

            var storeSettings = new CreateStoreSettings(Guid.NewGuid().ToString("N"), settings.ScopeId, typeof(Person));

            var store = cluster.CreateStore(storeSettings);

            var documents = TestDataService.GetFakes(store.StoreId.Name, 100);

            var commitResult = cluster.Commit(documents);

            var tester = documents[5];

            var cluster2 = new Cluster(settings);

            var criteria = new SimpleQueryCriteria(new[] { store.StoreId.Name }, tester.LastName);

            var query = new SimpleQuery(criteria);

            var results = cluster2.Query(query);

            cluster.DeleteStore(store.StoreId.Name);

            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);
        }
    }
}
