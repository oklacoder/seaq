namespace seaq
{
    public class DefaultReturnField :
        IReturnField
    {
        public string FieldName { get; set; }

        public DefaultReturnField(
            string fieldName)
        {
            FieldName = fieldName;
        }
    }
}
