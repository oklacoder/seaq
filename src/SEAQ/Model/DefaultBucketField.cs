namespace seaq
{
    public class DefaultBucketField :
        IBucketField
    {
        public string FieldName { get; set; }

        public DefaultBucketField(
            string fieldName)
        {
            FieldName = fieldName;
        }
    }
}
