using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace seaq
{

    public static class FieldNameUtilities
    {
        public static bool CheckIsFieldAlwaysReturned(
            this string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return false;

            string[] toCheck;

            if (fieldName.IndexOf(
                Constants.Fields.SortField, 
                StringComparison.OrdinalIgnoreCase) > -1)
            {
                toCheck = Constants.Fields.AlwaysReturnedFields
                    .Select(x => $"{x}.{Constants.Fields.SortField}")
                    .ToArray();
            }
            else if (fieldName.IndexOf(
                Constants.Fields.KeywordField,
                StringComparison.OrdinalIgnoreCase) > -1)
            {
                toCheck = Constants.Fields.AlwaysReturnedFields
                    .Select(x => $"{x}.{Constants.Fields.KeywordField}")
                    .ToArray();
            }
            else
            {
                toCheck = Constants.Fields.AlwaysReturnedFields
                    .ToArray();
            }
            return toCheck.Any(x => 
                x.Equals(fieldName, StringComparison.OrdinalIgnoreCase)) is true;
        }

        public static string GetElasticPropertyName(
            Type type,
            string propertyName)
        {
            return GetElasticPropertyName(
                type,
                propertyName,
                Constants.Fields.KeywordField);
        }

        public static string GetElasticSortPropertyName(
            Type type,
            string propertyName)
        {
            return GetElasticPropertyName(
                type,
                propertyName,
                Constants.Fields.SortField);
        }
        public static string GetElasticAggregatablePropertyName(
            Type type,
            string propertyName)
        {
            return GetElasticPropertyName(
                type,
                propertyName,
                Constants.Fields.KeywordField);
        }

        public static string GetElasticPropertyNameWithoutSuffix(
            Type type,
            string propertyName)
        {
            return GetElasticPropertyName(type, propertyName, null);
        }

        public static string RemoveKnownPropertySuffixesFromPropertyName(
            string propertyName)
        {
            if (propertyName.EndsWith(Constants.Fields.KeywordField))
            {
                return propertyName.Substring(0, propertyName.Length - (Constants.Fields.KeywordField.Length + 1));
            }
            if (propertyName.EndsWith(Constants.Fields.SortField))
            {
                return propertyName.Substring(0, propertyName.Length - (Constants.Fields.SortField.Length + 1));
            }
            return propertyName;
        }

        public static Type GetSearchableType(
            string typeFullName)
        {
            var type =
                GetAllSearchableTypes()
                    .FirstOrDefault(x =>
                        x.FullName.Equals(typeFullName, StringComparison.OrdinalIgnoreCase));

            return type;
        }
        public static IEnumerable<Type> GetAllSearchableTypes()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var implements =
                allAssemblies
                    .SelectMany(p =>
                    {
                        try
                        {
                            return p.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(x => x != null);
                        }
                    })
                    .Where(p => typeof(BaseDocument).IsAssignableFrom(p))
                    .Select(p => p.Assembly);

            var types =
                implements
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(BaseDocument).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Distinct()
                    .ToList();

            return types;
        }
        public static IEnumerable<Type> GetAllAggregations()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var implements =
                allAssemblies
                    .SelectMany(p =>
                    {
                        try
                        {
                            return p.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(x => x != null);
                        }
                    })
                    .Where(p => typeof(IAggregation).IsAssignableFrom(p))
                    .Select(p => p.Assembly);

            var types =
                implements
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(IAggregation).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Distinct()
                    .ToList();

            return types;
        }
        
        public static string ToCamelCase(this string input)
        {

            if (string.IsNullOrEmpty(input) || !char.IsUpper(input[0]))
            {
                return input;
            }
            var chunks = input.Split('.');
            var resp = new StringBuilder();
            foreach(var c in chunks)
            {
                char[] chars = input.ToCharArray();
                FixCasing(chars);
                var s = new string(chars);
                resp.Append(s);
            }
            return resp.ToString();
        }

        private static string GetElasticPropertyName(
            Type type,
            string propertyName,
            string suffix)
        {
            var property = GetPropertyForType(type, propertyName);

            if (property is null)
                return propertyName;

            var chunks = propertyName.Split('.');
            var fieldName = "";
            foreach (var chunk in chunks)
            {
                fieldName = String.Join(".", fieldName, chunk.ToCamelCase());
            }

            if ((property.PropertyType == typeof(string) || property.PropertyType.GetGenericArguments()?.FirstOrDefault() == typeof(string)) && !String.IsNullOrWhiteSpace(suffix))
            {
                fieldName = $"{fieldName}.{suffix}";
            }

            fieldName = fieldName.Trim('.');

            return fieldName;
        }
        private static PropertyInfo GetPropertyForType(
            Type type,
            string propertyName)
        {
            if (type.BaseType == typeof(Array))
            {
                type = type.GetElementType();
            }

            var nameChunks = propertyName.Split('.').ToList();
            var propertyList = type.GetProperties();
            var interfacePropertyList = type.GetInterfaces().SelectMany(p => p.GetProperties());
            var props = propertyList.Concat(interfacePropertyList);

            var property = props.FirstOrDefault(p => p.Name.Equals(nameChunks.FirstOrDefault(), StringComparison.OrdinalIgnoreCase));
            nameChunks.RemoveAt(0);

            if (nameChunks.Count > 0)
                if (property.PropertyType.UnderlyingSystemType.GetProperties().Any())
                {
                    property = GetPropertyForType(property.PropertyType.UnderlyingSystemType, String.Join(".", nameChunks));
                }

            return property;
        }

        private static void FixCasing(Span<char> chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);

                // Stop when next char is already lowercase.
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    // If the next char is a space, lowercase current char before exiting.
                    if (chars[i + 1] == ' ')
                    {
                        chars[i] = char.ToLowerInvariant(chars[i]);
                    }

                    break;
                }

                chars[i] = char.ToLowerInvariant(chars[i]);
            }
        }
    }
}
