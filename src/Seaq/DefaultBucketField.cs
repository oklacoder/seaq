using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seaq
{
    public class DefaultBucketField :
        IBucketField
    {
        public string FieldName { get; }

        public DefaultBucketField(
            string fieldName)
        {
            FieldName = fieldName;
        }
    }
}
