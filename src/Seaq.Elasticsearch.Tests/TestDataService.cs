using Bogus;
using System;
using System.Collections.Generic;
using System.Text;
using Flurl;
using Flurl.Http;
using Seaq.Elasticsearch.Clusters;

namespace Seaq.Elasticsearch.Tests
{
    public class TestDataService
    {
        public static Faker<Person> GetFaker(string storeIdName)
        {
            return new Faker<Person>()
                .RuleFor(u => u.Gender, (f, u) => f.PickRandom<Bogus.DataSets.Name.Gender>())
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName(u.Gender))
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName(u.Gender))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.Birthday, (f, u) => u.Birthday = f.Person.DateOfBirth)
                .RuleFor(u => u.StoreId, (f, u) => u.StoreId = storeIdName);
        }

        public static Person GetSingleFake(string storeIdName)
        {
            var personFaker = GetFaker(storeIdName);

            return personFaker.Generate();
        }

        public static Person[] GetFakes(string storeIdName, int count = 10)
        {
            var returnValue = new List<Person>();

            var personFaker = GetFaker(storeIdName);

            for (var i = 0; i < count; i++)
            {
                returnValue.Add(personFaker.Generate());
            }

            return returnValue.ToArray();
        }

        public static bool PingElastic(string url)
        {
            try
            {
                return url.HeadAsync().Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static ClusterSettings GetClusterSettings(
            string scopeId, 
            bool? forceRefreshOnCommit = null, 
            bool? eagerlyPersistStoreMeta = null)
        {
            //var httpUrl = "http://temp1:9200";
            //var httpUrl = "http://elastic.okstovall.com";
            //var httpUrl = "http://hmrasp001:9200";
            //var httpUrl = "http://hmelas002:9200";
            var httpUrl = "http://localhost:9200";
            //var httpsUrl = "https://localhost:9200";
            var httpsUrl = "https://elastic.okstovall.com";

            string urlToUse;

            urlToUse = PingElastic(httpUrl) ?
                httpUrl : httpsUrl;

            return new ClusterSettings(
                urlToUse,
                "elastic",
                "elastic",
                scopeId,
                forceRefreshOnCommit,
                eagerlyPersistStoreMeta);
        }
    }
}
