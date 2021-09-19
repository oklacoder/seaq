using seaq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SEAQ.Tests
{
    public class SampleResult :
        IDocument
    {
        [DataMember(Name = "id")]
        public string Id => OrderId.ToString();
        [DataMember(Name = "collection_id")]
        public string IndexName { get; set; } = "kibana_sample_data_ecommerce";



        public IEnumerable<string> Category { get; set; }
        public string Currency { get; set; }
        [DataMember(Name = "customer_first_name")]
        public string CustomerFirstName { get; set; }
        [DataMember(Name = "customer_full_name")]
        public string CustomerFullName { get; set; }
        [DataMember(Name = "customer_gender")]
        public string CustomerGender { get; set; }
        [DataMember(Name = "customer_id")]
        public int? CustomerId { get; set; }
        [DataMember(Name = "customer_last_name")]
        public string CustomerLastName { get; set; }
        [DataMember(Name = "customer_phone")]
        public string CustomerPhone { get; set; }
        [DataMember(Name = "day_of_week")]
        public string DayOfWeek { get; set; }
        [DataMember(Name = "day_of_week_i")]
        public int? DayOfWeekI { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Manufacturer { get; set; }
        [DataMember(Name = "order_date")]
        public DateTime OrderDate { get; set; }
        [DataMember(Name = "order_id")]
        public int? OrderId { get; set; }
        public IEnumerable<SampleProduct> Products { get; set; }
        public IEnumerable<string> Sku { get; set; }
        [DataMember(Name = "taxful_total_price")]
        public double? TaxfulTotalPrice { get; set; }
        [DataMember(Name = "taxless_total_price")]
        public double? TaxlessTotalPrice { get; set; }
        [DataMember(Name = "total_quantity")]
        public int? TotalQuantity { get; set; }
        [DataMember(Name = "total_unique_products")]
        public int? TotalUniqueProducts { get; set; }
        public string Type => GetType().FullName;
        public string User { get; set; }
    }
}
