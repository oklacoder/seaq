using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    internal static class CreateIndexDescriptorExtender
    {
        public static CreateIndexDescriptor Extend(
            IndexConfig config,
            Type type,
            CreateIndexDescriptor descriptor,
            string clusterScope)
        {
            return
                descriptor
                    .IncludeTypeName(false)
                    .Aliases(x => 
                    {
                        Dictionary<string, object> cache = new Dictionary<string, object>();
                        //cache.Add(
                        //    config.DocumentType.FormatIndexName(clusterScope),
                        //    null);

                        if (config?.Aliases is not null)
                        foreach(var a in config.Aliases)
                        {
                            var comp = IndexNameUtilities.FormatIndexName(a, clusterScope);
                            if (!cache.ContainsKey(comp)) cache.Add(comp, null);                                                            
                        }

                        cache.Keys.ToList().ForEach(a =>
                        {
                            if (a.Equals(config.Name) is not true)
                                x.Alias(a);
                        });

                        return x;
                    })
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
                    analysis => analysis
                        .Normalizers(GetLowerCaseNormalizer)
                        .Analyzers(GetDefaultTextAnalyzer));
        }

        private static NormalizersDescriptor GetLowerCaseNormalizer(
            NormalizersDescriptor normalizersDescriptor)
        {
            return
                normalizersDescriptor.Custom(
                    Constants.Normalizers.Lowercase,
                    customNormalizer => customNormalizer.Filters(Constants.Normalizers.Lowercase));
        }

        private static AnalyzersDescriptor GetDefaultTextAnalyzer(
            AnalyzersDescriptor analyzersDescriptor)
        {
            return analyzersDescriptor
                .Custom(
                    Constants.Tokenizers.Classic,
                    c => c.Filters("classic", "lowercase", "uppercase", "stop", "word_delimiter_graph").Tokenizer(Constants.Tokenizers.Classic));
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
