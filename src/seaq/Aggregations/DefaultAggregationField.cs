using System;

namespace seaq
{
    public class DefaultAggregationField :
        IAggregationField
    {
        public string FieldName { get; set; }

        public DefaultAggregationField()
        {

        }
        public DefaultAggregationField(
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException($"Parameter {nameof(fieldName)} is required.");
            FieldName = fieldName;
        }
    }
}
