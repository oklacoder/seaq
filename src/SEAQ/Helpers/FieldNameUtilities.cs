using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace seaq
{
    public static class FieldNameUtilities
    {
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
                    .Where(p => typeof(IDocument).IsAssignableFrom(p))
                    .Select(p => p.Assembly);

            var types =
                implements
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(IDocument).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Distinct()
                    .ToList();

            return types;
        }
        private static string GetElasticPropertyName(
            Type type,
            string propertyName,
            string suffix)
        {
            var property = GetPropertyForType(type, propertyName);

            var chunks = propertyName.Split('.');
            var fieldName = "";
            foreach (var chunk in chunks)
            {
                fieldName = String.Join(".", fieldName, chunk);
            }

            if ((property.PropertyType == typeof(string) || property.PropertyType.GetGenericArguments()?.FirstOrDefault() == typeof(string)) && !String.IsNullOrWhiteSpace(suffix))
            {
                fieldName = $"{fieldName}.{suffix}";
            }

            return fieldName.Trim('.');
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
    }
}
