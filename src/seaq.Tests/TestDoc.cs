using seaq;
using System;
using System.Collections.Generic;

namespace seaq.Tests
{
    public class TestDoc :
        BaseDocument
    {
        public override string Id => DocId.ToString();

        public override string IndexName => GetType().FullName;

        public override string Type => GetType().FullName;

        public Guid DocId { get; set; }

        public string StringValue { get; set; }
        public int? IntValue { get; set; }
        public double? DoubleValue { get; set; }
        public decimal? DecimalValue { get; set; }
        public DateTime? DateValue { get; set; }
        public TestChild ObjectProperty { get; set; }
        public IEnumerable<TestChild> CollectionProperty { get; set; }

        public TestDoc()
        {

        }
    }

    public class TestDoc1 :
        TestDoc
    {
        public override string IndexAsType => typeof(BaseDocument).FullName;
    }
    public class TestDoc2 :
        TestDoc
    {
        public override string IndexAsType => typeof(BaseDocument).FullName;

    }
}
