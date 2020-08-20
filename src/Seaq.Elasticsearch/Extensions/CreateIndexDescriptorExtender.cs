using System;
using System.Collections.Generic;
using Nest;
using Seaq.Elasticsearch.Clusters;
using Seaq.Elasticsearch.Stores;

namespace Seaq.Elasticsearch.Extensions
{
    internal static class CreateIndexDescriptorExtender
    {
        public static CreateIndexDescriptor Extend(
            CreateStoreSettings settings,
            CreateIndexDescriptor descriptor)
        {
            return
                descriptor
                    .Map(x => GetTypeMappingDescriptor(x, settings.Type)
                    .Meta(m => m.Add(WellKnownKeys.IndexSettings.StoreSchema, 
                        new StoreSchema(settings))))
                    .Settings(p => p
                        .GetAnalysisDescriptor()
                        .GetShardSettingsDescriptor(settings));
        }

        private static IndexSettingsDescriptor GetShardSettingsDescriptor(
            this IndexSettingsDescriptor indexSettingsDescriptor,
            CreateStoreSettings settings)
        {
            return
                indexSettingsDescriptor
                    .NumberOfShards(settings.PrimaryShards)
                    .NumberOfReplicas(settings.ReplicaShards);
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
                    WellKnownKeys.Normalizers.Lowercase,
                    customNormalizer => customNormalizer.Filters(WellKnownKeys.Normalizers.Lowercase));
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