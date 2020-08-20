using Nest;
using Seaq.Elasticsearch.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seaq.Elasticsearch.Extensions
{
    public static class StoreExtensions
    {
        public static StoreField ToStoreField(this IProperty property, string parentFieldName = null)
        {
            var fieldName = string.Join(".", parentFieldName, property.Name.Name).Trim('.');

            return property switch
            {
                ObjectProperty objectProperty => GetStoreField(objectProperty, fieldName),
                TextProperty textProperty => GetStoreField(textProperty, fieldName),
                BooleanProperty boolProperty => GetStoreField(boolProperty, fieldName),
                NumberProperty numberProperty => GetStoreField(numberProperty, fieldName),
                DateProperty dateProperty => GetStoreField(dateProperty, fieldName),
                _ => GetStoreField(property, fieldName)
            };
        }

        private static StoreField GetStoreField(ObjectProperty property, string fieldName)
        {
            return new StoreField
            (
                fieldName,
                (property as IProperty).Type,
                property?.Properties == null ? null : property.Properties.Select(x => ToStoreField(x.Value, fieldName))
            );
        }
        private static StoreField GetStoreField(TextProperty property, string fieldName)
        {
            return new StoreField
            (
                fieldName,
                (property as IProperty).Type,
                property?.Fields == null ? null : property.Fields.Select(x => ToStoreField(x.Value, fieldName))
            );
        }
        private static StoreField GetStoreField(BooleanProperty property, string fieldName)
        {
            return new StoreField
            (
                fieldName,
                (property as IProperty).Type,
                property?.Fields == null ? null : property.Fields.Select(x => ToStoreField(x.Value, fieldName))
            );
        }
        private static StoreField GetStoreField(NumberProperty property, string fieldName)
        {
            return new StoreField
            (
                fieldName,
                (property as IProperty).Type,
                property?.Fields == null ? null : property.Fields.Select(x => ToStoreField(x.Value, fieldName))
            );
        }
        private static StoreField GetStoreField(DateProperty property, string fieldName)
        {
            return new StoreField
            (
                fieldName,
                (property as IProperty).Type,
                property?.Fields == null ? null : property.Fields.Select(x => ToStoreField(x.Value, fieldName))
            );
        }
        private static StoreField GetStoreField(IProperty property, string fieldName)
        {
            return new StoreField
            (
                fieldName,
                (property as IProperty).Type,
                null
            );
        }
    }
}
