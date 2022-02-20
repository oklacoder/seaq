using System;

namespace seaq
{
    public static partial class DescriptorExtensions
    {
        public static CreateIndexDescriptor Extend(
            this CreateIndexDescriptor descriptor,
            IndexConfig config,
            Type type)
        {
            return CreateIndexDescriptorExtender.Extend(config, type, descriptor);
        }
    }
}
