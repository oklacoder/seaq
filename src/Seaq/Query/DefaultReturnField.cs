using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seaq
{
    public class DefaultReturnField :
        IReturnField
    {
        public string FieldName { get; }

        public DefaultReturnField(
            string fieldName)
        {
            FieldName = fieldName;
        }
    }
}
