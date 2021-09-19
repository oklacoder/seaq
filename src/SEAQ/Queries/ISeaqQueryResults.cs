using System.Collections.Generic;

namespace seaq
{
    public interface ISeaqQueryResults<T>
        where T : class, IDocument
    {
        public IEnumerable<T> Documents { get; }
        long Took { get; }
        long Total { get; }
    }
}
