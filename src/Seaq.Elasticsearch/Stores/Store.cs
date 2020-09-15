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
            TypeFullName = settings.TypeFullName;
        }
        public Store(
            Store store,
            StoreSchema storeSchema)
        {
            StoreId = store.StoreId;
            TypeFullName = store.TypeFullName;
            StoreSchema = storeSchema;
        }
        public Store(
          StoreId storeId,
          StoreSchema storeSchema,
          string typeFullName)
        {
          StoreId = storeId;
          StoreSchema = storeSchema;
          TypeFullName = typeFullName;
        }

        public StoreId StoreId { get; }

        public StoreSchema StoreSchema { get; }

        public string TypeFullName { get; }

    }
}
