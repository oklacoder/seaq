using System.Collections.Generic;

namespace seaq
{
    public interface ISeaqQueryResults
    {
        public IEnumerable<BaseDocument> Documents { get; }
        long Took { get; }
        long Total { get; }
    }
    public interface ISeaqQueryResults<T>
        where T : BaseDocument
    {
        public IEnumerable<T> Documents { get; }
        long Took { get; }
        long Total { get; }
    }
}
