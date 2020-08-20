using System.Linq;
using System;

namespace Seaq.Elasticsearch.Stores
{
    public class StoreId
    {
        const string _delimiter = "_";
        public StoreId(
            string scopeId,
            string moniker)
        {
            ScopeId = scopeId;
            Moniker = moniker;
        }

        public StoreId(
          string storeIdName)
        {
            var parts = storeIdName.Split(_delimiter);
            if (parts.Length > 2){
              throw new ArgumentException($"Provided input is invalid - can only contain, at most, one instance of '{_delimiter}'");
            }
            else if (parts.Length == 2){
              ScopeId = parts.FirstOrDefault();
              Moniker = parts.LastOrDefault();
            }
            else if (parts.Length == 1){
              Moniker = parts.FirstOrDefault();
            }
            else{
              throw new ArgumentException($"Provided input '{storeIdName}' is not valid.");
            }
        }

        public string ScopeId { get; }
        public string Moniker { get; }
        public string Name => FormatAsIndexId(ScopeId, Moniker);


        public static string FormatAsIndexId(
            string scopeId,
            string moniker)
        {
            if (string.IsNullOrWhiteSpace(moniker))
            {
                throw new ArgumentNullException($"Argument {nameof(moniker)} is required.");
            }
            if (scopeId.Contains(_delimiter) || moniker.Contains(_delimiter))
            {
                throw new ArgumentException($"Provided value for {nameof(scopeId)} or {nameof(moniker)} contains the reserved character {_delimiter}.");
            }
            if (string.IsNullOrWhiteSpace(scopeId))
            {
                return moniker.ToLowerInvariant();
            }
            return $"{scopeId}{_delimiter}{moniker}".ToLowerInvariant();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StoreId)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ScopeId, Moniker);
        }

        public override string ToString()
        {
            return Name;
        }

        protected bool Equals(StoreId other)
        {
            return
                ScopeId == other.ScopeId &&
                Moniker == other.Moniker;
        }
    }
}
