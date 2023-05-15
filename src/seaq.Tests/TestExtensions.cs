using System;
using System.Reflection;

namespace seaq.Tests
{
    public static class TestExtensions
    {
        internal static bool IsPropertyEmpty(this PropertyInfo p, string val)
        {
            var res = true;
            if (val == null)
                return true;

            var t = p.PropertyType;

            if (t == typeof(DateTime))
            {
                res = DateTime.Parse(val) == DateTime.MinValue;
            }
            else if (t == typeof(DateTime?))
            {
                res = DateTime.Parse(val) == DateTime.MinValue;
            }
            else
            {
                res = val == null;
            }

            return res;
        }

    }
}
