using System;
using System.Runtime.Serialization;

namespace SEAQ.Tests
{
    public class SampleProduct
    {
        [DataMember(Name = "base_price")]
        public double? BasePrice { get; set; }
        [DataMember(Name = "discount_percentage")]
        public double? DiscountPercentage { get; set; }
        public int? Quantity { get; set; }
        public string Manufacturer { get; set; }
        [DataMember(Name = "tax_amount")]
        public double? TaxAmount { get; set; }
        [DataMember(Name = "product_id")]
        public int? ProductId { get; set; }
        public string Category { get; set; }
        public string Sku { get; set; }
        [DataMember(Name = "taxless_price")]
        public double? TaxlessPrice { get; set; }
        [DataMember(Name = "unit_discount_amount")]
        public double? UnitDiscountAmount { get; set; }
        [DataMember(Name = "min_price")]
        public double? MinPrice { get; set; }
        [DataMember(Name = "_id")]
        public string Id { get; set; }
        [DataMember(Name = "discount_amount")]
        public double? DiscountAmount { get; set; }
        [DataMember(Name = "created_on")]
        public DateTime CreatedOn { get; set; }
        [DataMember(Name = "product_name")]
        public string ProductName { get; set; }
        public double? Price { get; set; }
        [DataMember(Name = "taxful_price")]
        public double? TaxfulPrice { get; set; }
        [DataMember(Name = "base_unit_price")]
        public double? BaseUnitPrice { get; set; }
    }
}
