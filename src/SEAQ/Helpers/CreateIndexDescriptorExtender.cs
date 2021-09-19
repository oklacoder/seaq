using Nest;
using System;

namespace seaq
{
    internal static class CreateIndexDescriptorExtender
    {
        public static CreateIndexDescriptor Extend(
            IndexConfig config,
            Type type,
            CreateIndexDescriptor descriptor)
        {
            return
                descriptor
                    .Aliases(x => x.Alias(config.Type))
                    .Map(x => GetTypeMappingDescriptor(x, type)
                    .Meta(m => m.Add(Constants.Indices.Meta.SchemaKey,
                        new Index(config))))
                    .Settings(p => p
                        .GetAnalysisDescriptor()
                        .GetShardSettingsDescriptor(config));
        }

        private static IndexSettingsDescriptor GetShardSettingsDescriptor(
            this IndexSettingsDescriptor indexSettingsDescriptor,
            IndexConfig config)
        {
            return
                indexSettingsDescriptor
                    .NumberOfShards(config.PrimaryShards)
                    .NumberOfReplicas(config.ReplicaShards);
        }

        private static IndexSettingsDescriptor GetAnalysisDescriptor(
            this IndexSettingsDescriptor indexSettingsDescriptor)
        {
            return
                indexSettingsDescriptor.Analysis(
                    analysis => analysis.Normalizers(GetLowerCaseNormalizer));
        }

        private static NormalizersDescriptor GetLowerCaseNormalizer(
            NormalizersDescriptor normalizersDescriptor)
        {
            return
                normalizersDescriptor.Custom(
                    Constants.Normalizers.Lowercase,
                    customNormalizer => customNormalizer.Filters(Constants.Normalizers.Lowercase));
        }

        private static TypeMappingDescriptor<object> GetTypeMappingDescriptor(
            TypeMappingDescriptor<object> typeMappingDescriptor,
            Type type)
        {
            return
                typeMappingDescriptor.AutoMap(
                    type,
                    new AdditionalTextFieldPropertiesVisitor());
        }
    }
}
