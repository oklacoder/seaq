using seaq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SEAQ.Tests
{
    public class SampleResult :
        BaseDocument
    {
        [DataMember(Name = "id")]
        [JsonPropertyName("id")]
        public override string Id => OrderId.ToString();
        //[DataMember(Name = "_index")]
        //[JsonPropertyName("_index")]
        public override string IndexName { get; set; } = "kibana_sample_data_ecommerce";

        public IEnumerable<string> Category { get; set; }
        public string Currency { get; set; }
        [DataMember(Name = "customer_first_name")]
        [JsonPropertyName("customer_first_name")]
        public string CustomerFirstName { get; set; }
        [DataMember(Name = "customer_full_name")]
        [JsonPropertyName("customer_full_name")]
        public string CustomerFullName { get; set; }
        [DataMember(Name = "customer_gender")]
        [JsonPropertyName("customer_gender")]
        public string CustomerGender { get; set; }
        [DataMember(Name = "customer_id")]
        [JsonPropertyName("customer_id")]
        public int? CustomerId { get; set; }
        [DataMember(Name = "customer_last_name")]
        [JsonPropertyName("customer_last_name")]
        public string CustomerLastName { get; set; }
        [DataMember(Name = "customer_phone")]
        [JsonPropertyName("customer_phone")]
        public string CustomerPhone { get; set; }
        [DataMember(Name = "day_of_week")]
        [JsonPropertyName("day_of_week")]
        public string DayOfWeek { get; set; }
        [DataMember(Name = "day_of_week_i")]
        [JsonPropertyName("day_of_week_i")]
        public int? DayOfWeekI { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Manufacturer { get; set; }
        [DataMember(Name = "order_date")]
        [JsonPropertyName("order_date")]
        public DateTime OrderDate { get; set; }
        [DataMember(Name = "order_id")]
        [JsonPropertyName("order_id")]
        public int? OrderId { get; set; }
        public IEnumerable<SampleProduct> Products { get; set; }
        public IEnumerable<string> Sku { get; set; }
        [DataMember(Name = "taxful_total_price")]
        [JsonPropertyName("taxful_total_price")]
        public double? TaxfulTotalPrice { get; set; }
        [DataMember(Name = "taxless_total_price")]
        [JsonPropertyName("taxless_total_price")]
        public double? TaxlessTotalPrice { get; set; }
        [DataMember(Name = "total_quantity")]
        [JsonPropertyName("total_quantity")]
        public int? TotalQuantity { get; set; }
        [DataMember(Name = "total_unique_products")]
        [JsonPropertyName("total_unique_products")]
        public int? TotalUniqueProducts { get; set; }
        [DataMember(Name = "type")]
        [JsonPropertyName("type")]
        public override string Type => GetType().FullName;
        public string User { get; set; }
    }
}
