using System;

namespace Seaq.Elasticsearch.Stores
{

    public class Store
    {
        public Store(
            CreateStoreSettings settings)
        {

            StoreId = new StoreId(
                settings.ScopeId, 
                settings.Moniker);
            Type = settings.Type;
        }
        public Store(
            Store store,
            StoreSchema storeSchema)
        {
            StoreId = store.StoreId;
            Type = store.Type;
            StoreSchema = storeSchema;
        }
        public Store(
          StoreId storeId,
          StoreSchema storeSchema,
          Type type)
        {
          StoreId = storeId;
          StoreSchema = storeSchema;
          Type = type;
        }

        public StoreId StoreId { get; }

        public StoreSchema StoreSchema { get; }

        public Type Type { get; }

    }
}
