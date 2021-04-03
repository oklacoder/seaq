using System.Text;
using System.Threading.Tasks;

namespace Seaq
{
    public interface IFilterField
    {
        public string FieldName { get; }
        public string Value { get; }
        public IComparator Comparator { get; }
    }

}
