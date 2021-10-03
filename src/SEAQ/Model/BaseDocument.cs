namespace seaq
{
    public class BaseDocument :
        IDocument
    {
        public virtual string Id { get; set; }
               
        public virtual string IndexName { get; set; }
               
        public virtual string Type { get; set; }

        public BaseDocument()
        {

        }
    }
}
