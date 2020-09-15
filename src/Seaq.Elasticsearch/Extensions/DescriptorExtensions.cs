using System;
using Nest;
using Seaq.Elasticsearch.Stores;

namespace Seaq.Elasticsearch.Extensions
{
        public static partial class DescriptorExtensions
        {
            public static CreateIndexDescriptor Extend(
                this CreateIndexDescriptor descriptor,
                CreateStoreSettings settings,
                Type type)
            {
                return CreateIndexDescriptorExtender.Extend(settings, type, descriptor);
            }
        }
    
}
