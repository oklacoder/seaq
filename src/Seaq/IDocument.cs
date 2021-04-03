using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seaq
{
    public interface IDocument
    {
        string Id { get; }
        string CollectionId { get; }
        string Type { get; }
    }
}
