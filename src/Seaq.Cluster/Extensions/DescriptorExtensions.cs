using System;
using Nest;

namespace Seaq.Clusters{
    public static partial class DescriptorExtensions
    {
        public static CreateIndexDescriptor Extend(
            this CreateIndexDescriptor descriptor,
            CollectionConfig config,
            Type type)
        {
            return CreateIndexDescriptorExtender.Extend(config, type, descriptor);
        }
    }

}
