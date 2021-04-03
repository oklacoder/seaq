using System;
using Nest;

namespace Seaq.Clusters{
    internal static class CreateIndexDescriptorExtender
    {
        public static CreateIndexDescriptor Extend(
            ICollectionConfig config,
            Type type,
            CreateIndexDescriptor descriptor)
        {
            return
                descriptor
                    .Map(x => GetTypeMappingDescriptor(x, type)
                    .Meta(m => m.Add(Constants.CollectionSettings.SchemaKey,
                        new Collection(config))))
                    .Settings(p => p
                        .GetAnalysisDescriptor()
                        .GetShardSettingsDescriptor(config));
        }

        private static IndexSettingsDescriptor GetShardSettingsDescriptor(
            this IndexSettingsDescriptor indexSettingsDescriptor,
            ICollectionConfig config)
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
