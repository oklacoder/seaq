using Nest;
using System.Reflection;

namespace seaq
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
                    .Name(Constants.Fields.SortField)
                    .Normalizer(Constants.Normalizers.Lowercase);

            type.Fields.Add(Constants.Fields.SortField, sort);
            type.Analyzer = Constants.Tokenizers.Letter;

            base.Visit(type, propertyInfo, attribute);
        }
    }
}
