using Nest;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Seaq.Elasticsearch.Clusters
{
    public class AdditionalTextFieldPropertiesVisitor :
         NoopPropertyVisitor
    {
        public override void Visit(
            ITextProperty type,
            PropertyInfo propertyInfo,
            ElasticsearchPropertyAttributeBase attribute)
        {
            var sort =
                new KeywordPropertyDescriptor<string>()
                    .Name(WellKnownKeys.Fields.LowerField)
                    .Normalizer(WellKnownKeys.Normalizers.Lowercase);

            type.Fields.Add(WellKnownKeys.Fields.LowerField, sort);

            base.Visit(type, propertyInfo, attribute);
        }
    }
}
