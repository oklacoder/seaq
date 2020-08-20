using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Seaq.Elasticsearch.Tests
{
    [DataContract]
    public class Person :
        IDocument
    {
        public Person() { }
        public Person(
            string firstName,
            string lastName,
            string phoneNumber,
            string email,
            string storeId,
            DateTime birthday)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Email = email;
            StoreId = storeId;
            Birthday = birthday;
        }

        [DataMember(Name = nameof(DocumentId))]
        public string DocumentId => Email;

        [DataMember(Name = nameof(StoreId))]
        public string StoreId { get; set; }

        [DataMember(Name = nameof(Age))]
        public int Age => 
            DateTime.Now.DayOfYear <= Birthday.DayOfYear ? 
            DateTime.Now.Year - Birthday.Year - 1 : 
            DateTime.Now.Year - Birthday.Year;

        [DataMember(Name = nameof(Birthday))]
        public DateTime Birthday { get; set; }

        [DataMember(Name = nameof(Type))]
        public string Type => this.GetType().FullName;

        [DataMember(Name = nameof(PrimaryDisplay))]
        public string PrimaryDisplay => $"{FirstName} {LastName}";

        [DataMember(Name = nameof(SecondaryDisplay))]
        public string SecondaryDisplay => Email;

        [DataMember(Name = nameof(Suggestions))]
        public string[] Suggestions => new string[] { FirstName, LastName, Email }.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();

        [DataMember(Name = nameof(FirstName))]
        public string FirstName { get; set; }

        [DataMember(Name = nameof(LastName))]
        public string LastName { get; set; }

        [DataMember(Name = nameof(PhoneNumber))]
        public string PhoneNumber { get; set; }

        [DataMember(Name = nameof(Email))]
        public string Email { get; set; }

        [DataMember(Name = nameof(Gender))]
        public Bogus.DataSets.Name.Gender Gender { get; set; }
    }
}
