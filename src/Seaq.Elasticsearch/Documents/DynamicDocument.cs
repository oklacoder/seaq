using System.Collections.Generic;
using System.Dynamic;

namespace Seaq.Elasticsearch.Documents
{
    public class DynamicDocument : 
        DynamicObject, 
        IDocument
    {
        public DynamicDocument()
        {

        }
        public DynamicDocument(
            string documentId,
            string storeId,
            string type,
            string primaryDisplay,
            string secondaryDisplay,
            string[] suggestions)
        {
            DocumentId = documentId;
            StoreId = storeId;
            Type = type;
            PrimaryDisplay = primaryDisplay;
            SecondaryDisplay = secondaryDisplay;
            Suggestions = suggestions;
        }

        public string DocumentId { get; }

        public string StoreId { get; }

        public string Type { get; }

        public string PrimaryDisplay { get; }

        public string SecondaryDisplay { get; }

        public string[] Suggestions { get; }

        

        // The inner dictionary.
        Dictionary<string, object> dictionary
            = new Dictionary<string, object>();

        // This property returns the number of elements
        // in the inner dictionary.
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return dictionary.Keys;
        }

        // If you try to get a value of a property
        // not defined in the class, this method is called.
        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            //string name = binder.Name.ToLower();
            string name = binder.Name;

            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            return dictionary.TryGetValue(name, out result);
        }

        // If you try to set a value of a property that is
        // not defined in the class, this method is called.
        public override bool TrySetMember(
            SetMemberBinder binder, object value)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            //dictionary[binder.Name.ToLower()] = value;
            dictionary[binder.Name] = value;

            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }
    }
}
