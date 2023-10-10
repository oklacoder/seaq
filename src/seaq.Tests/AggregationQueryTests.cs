using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace seaq.Tests
{
    public class AggregationQueryTests :
        TestModule
    {
        public AggregationQueryTests(
            ITestOutputHelper testOutput) :
            base(testOutput)
        {

        }

        [Fact]
        public async void AggregationQuery_CanExecute()
        {
            const string name = "AggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.AverageAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
        }

        [Fact]
        public async void AggregationQuery_CanExecuteAsync()
        {
            const string name = "AggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.AverageAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = await cluster.QueryAsync(query) as AggregationQueryResults<SampleResult>;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
        }
        [Fact]
        public async void AggregationQuery_CanExecute_Untyped()
        {
            const string name = "AggregationQuery_CanExecute_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.AverageAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
        }
        [Fact]
        public async void AggregationQuery_CanExecuteAsync_Untyped()
        {
            const string name = "AggregationQuery_CanExecute_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.AverageAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery(criteria);
            var results = await cluster.QueryAsync(query) as AggregationQueryResults;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
        }
        [Fact]
        public async void AggregationField_ThrowsOnEmptyFieldName()
        {
            const string name = "AggregationField_FailsOnEmptyFieldName";
            Assert.Throws<ArgumentNullException>(() => new DefaultAggregationField(""));
        }
        [Fact]
        public async void AggregationRequest_ThrowsOnUnknownAggregationName()
        {
            const string name = "AggregationRequest_FailsOnEmptyFieldName";
            var cluster = Cluster.Create(GetArgs(name));
            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest("blurg", new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery<SampleResult>(criteria);

            Assert.Throws<KeyNotFoundException>(() => cluster.Query(query));

            DecomissionCluster(cluster);
        }
        [Fact]
        public async void AggregationRequest_ThrowsOnUnknownAggregationName_Untyped()
        {
            const string name = "AggregationRequest_ThrowsOnUnknownAggregationName_Untyped";
            var cluster = Cluster.Create(GetArgs(name));
            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest("blurg", new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery(criteria);

            Assert.Throws<KeyNotFoundException>(() => cluster.Query<ISeaqQueryResults>(query));

            DecomissionCluster(cluster);

        }
        [Fact]
        public async void AggregationRequest_ThrowsOnEmptyAggregationName()
        {
            const string name = "AggregationRequest_FailsOnEmptyFieldName";
            
            Assert.Throws<ArgumentNullException>(() => new DefaultAggregationRequest("", new DefaultAggregationField("taxful_total_price")));
        }
        [Fact]
        public async void AverageAggregationQuery_CanExecute()
        {
            const string name = "AverageAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.AverageAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as AverageAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Value);
            Assert.IsType<double>(r.Value);
        }
        [Fact]
        public async void AverageAggregationQuery_CanExecute_Untyped()
        {
            const string name = "AverageAggregationQuery_CanExecute_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.AverageAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            var r = results.AggregationResults.First() as AverageAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Value);
            Assert.IsType<double>(r.Value);
        }
        [Fact]
        public async void AverageAggregationQuery_FailsOnNonNumeric()
        {
            const string name = "StatsAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.AverageAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }
        [Fact]
        public async void AverageAggregationQuery_FailsOnNonNumeric_Untyped()
        {
            const string name = "AverageAggregationQuery_FailsOnNonNumeric_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(BaseDocument).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.AverageAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }

        [Fact]
        public async void DateHistogramAggregationQuery_CanExecute()
        {
            const string name = "DateHistogramAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                filterFields: new[] { new DefaultFilterField(DefaultComparator.GreaterThanOrEqual, "2022-03-01", "order_date") },
                aggregationRequests: new[] { new DateHistogramAggregationRequest(new DefaultAggregationField("order_date")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as DateHistogramAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Buckets);
            Assert.NotEmpty(r.Buckets);
        }
        [Fact]
        public async void DateHistogramAggregationQuery_CanExecute_Untyped()
        {
            const string name = "DateHistogramAggregationQuery_CanExecute_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                filterFields: new[] {new DefaultFilterField(DefaultComparator.GreaterThanOrEqual, "2022-03-01", "order_date")},
                aggregationRequests: new[] { new DateHistogramAggregationRequest(new DefaultAggregationField("order_date")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            var r = results.AggregationResults.First() as DateHistogramAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Buckets);
            Assert.NotEmpty(r.Buckets);
        }
        [Fact]
        public async void DateHistogramAggregationQuery_FailsOnNonDate()
        {
            const string name = "DateHistogramAggregationQuery_FailsOnNonDate";
            var cluster = Cluster.Create(GetArgs(name));

            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                filterFields: new[] { new DefaultFilterField(DefaultComparator.GreaterThanOrEqual, "2022-03-01", "order_date") },
                aggregationRequests: new[] { new DateHistogramAggregationRequest(new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }
        [Fact]
        public async void DateHistogramAggregationQuery_FailsOnNonDate_Untyped()
        {
            const string name = "DateHistogramAggregationQuery_FailsOnNonDate_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(BaseDocument).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.DateHistogramAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }
        [Fact]
        public async void DateHistogramAggregationQuery_ExtendedBoundsWork()
        {
            const string name = "DateHistogramAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            var extended_bounds_start = new DateTime(2020, 01, 01);
            var extended_bounds_end = new DateTime(DateTime.Now.Year, DateTime.Now.Day, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            var expected_buckets = Math.Abs(extended_bounds_end.Subtract(extended_bounds_start).Days)+1; //"+1" because Elastic treats it as an inclusive range

            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                filterFields: new[] { new DefaultFilterField(DefaultComparator.GreaterThanOrEqual, "2022-03-01", "order_date") },
                aggregationRequests: new[] { new DateHistogramAggregationRequest(
                    new DefaultAggregationField("order_date"),
                    interval: Constants.DateIntervals.Day,
                    minBucketSize: 0,
                    extendedBoundsMin: extended_bounds_start,
                    extendedBoundsMax: extended_bounds_end
                ) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as DateHistogramAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Buckets);
            Assert.NotEmpty(r.Buckets);
            Assert.Equal(expected_buckets, r.Buckets.Count());
            //1096
            //1369
        }
        
        [Fact]
        public async void HistogramAggregationQuery_CanExecute()
        {
            const string name = "HistogramAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                filterFields: new[] { new DefaultFilterField(DefaultComparator.GreaterThanOrEqual, "2022-03-01", "order_date") },
                aggregationRequests: new[] { new HistogramAggregationRequest(new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as HistogramAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Buckets);
            Assert.NotEmpty(r.Buckets);
        }
        [Fact]
        public async void HistogramAggregationQuery_CanExecute_Untyped()
        {
            const string name = "HistogramAggregationQuery_CanExecute_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                filterFields: new[] {new DefaultFilterField(DefaultComparator.GreaterThanOrEqual, "2022-03-01", "order_date")},
                aggregationRequests: new[] { new HistogramAggregationRequest(new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            var r = results.AggregationResults.First() as HistogramAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Buckets);
            Assert.NotEmpty(r.Buckets);
        }
        [Fact]
        public async void HistogramAggregationQuery_FailsOnNonNumber()
        {
            const string name = "HistogramAggregationQuery_FailsOnNonDate";
            var cluster = Cluster.Create(GetArgs(name));

            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                filterFields: new[] { new DefaultFilterField(DefaultComparator.GreaterThanOrEqual, "2022-03-01", "order_date") },
                aggregationRequests: new[] { new HistogramAggregationRequest(new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }
        [Fact]
        public async void HistogramAggregationQuery_FailsOnNonNumber_Untyped()
        {
            const string name = "HistogramAggregationQuery_FailsOnNonDate_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(BaseDocument).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.DateHistogramAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }


        [Fact]
        public async void MinAggregationQuery_CanExecute()
        {
            const string name = "MinAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.MinAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as MinAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Value);
            Assert.IsType<double>(r.Value);
        }
        [Fact]
        public async void MinAggregationQuery_CanExecute_Untyped()
        {
            const string name = "MinAggregationQuery_CanExecute_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.MinAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            var r = results.AggregationResults.First() as MinAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Value);
            Assert.IsType<double>(r.Value);
        }
        [Fact]
        public async void MinAggregationQuery_FailsOnNonNumeric()
        {
            const string name = "StatsAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.MinAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }
        [Fact]
        public async void MinAggregationQuery_FailsOnNonNumeric_Untyped()
        {
            const string name = "MinAggregationQuery_FailsOnNonNumeric_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(BaseDocument).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.MinAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }


        [Fact]
        public async void MaxAggregationQuery_CanExecute()
        {
            const string name = "MaxAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.MaxAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as MaxAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Value);
            Assert.IsType<double>(r.Value);
        }
        [Fact]
        public async void MaxAggregationQuery_CanExecute_Untyped()
        {
            const string name = "MaxAggregationQuery_CanExecute_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.MaxAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            var r = results.AggregationResults.First() as MaxAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Value);
            Assert.IsType<double>(r.Value);
        }
        [Fact]
        public async void MaxAggregationQuery_FailsOnNonNumeric()
        {
            const string name = "StatsAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.MaxAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }
        [Fact]
        public async void MaxAggregationQuery_FailsOnNonNumeric_Untyped()
        {
            const string name = "MaxAggregationQuery_FailsOnNonNumeric_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(BaseDocument).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.MaxAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }

        [Fact]
        public async void SumAggregationQuery_CanExecute()
        {
            const string name = "SumAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.SumAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as SumAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Value);
            Assert.IsType<double>(r.Value);
        }
        [Fact]
        public async void SumAggregationQuery_CanExecute_Untyped()
        {
            const string name = "SumAggregationQuery_CanExecute_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.SumAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            var r = results.AggregationResults.First() as SumAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotNull(r.Value);
            Assert.IsType<double>(r.Value);
        }
        [Fact]
        public async void SumAggregationQuery_FailsOnNonNumeric()
        {
            const string name = "StatsAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.SumAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }
        [Fact]
        public async void SumAggregationQuery_FailsOnNonNumeric_Untyped()
        {
            const string name = "SumAggregationQuery_FailsOnNonNumeric_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(BaseDocument).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.SumAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }


        [Fact]
        public async void PercentilesAggregationQuery_CanExecute()
        {
            const string name = "PercentilesAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.PercentilesAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as PercentilesAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotEmpty(r.Percentiles);
        }
        [Fact]
        public async void PercentilesAggregationQuery_CanExecute_Untyped()
        {
            const string name = "PercentilesAggregationQuery_CanExecute_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.PercentilesAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            var r = results.AggregationResults.First() as PercentilesAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotEmpty(r.Percentiles);
        }
        [Fact]
        public async void PercentilesAggregationQuery_FailsOnNonNumeric()
        {
            const string name = "StatsAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.PercentilesAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }
        [Fact]
        public async void PercentilesAggregationQuery_FailsOnNonNumeric_Untyped()
        {
            const string name = "PercentilesAggregationQuery_FailsOnNonNumeric_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(BaseDocument).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.PercentilesAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }


        [Fact]
        public async void StatsAggregationQuery_CanExecute()
        {
            const string name = "StatsAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.StatsAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as StatsAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);

            Assert.NotNull(r.Average);
            Assert.IsType<double>(r.Average);
            Assert.NotNull(r.Count);
            Assert.IsType<double>(r.Count);
            Assert.NotNull(r.Max);
            Assert.IsType<double>(r.Max);
            Assert.NotNull(r.Min);
            Assert.IsType<double>(r.Min);
            Assert.NotNull(r.Sum);
            Assert.IsType<double>(r.Sum);
        }
        [Fact]
        public async void StatsAggregationQuery_CanExecute_Untyped()
        {
            const string name = "StatsAggregationQuery_CanExecute_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.StatsAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            var r = results.AggregationResults.First() as StatsAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);

            Assert.NotNull(r.Average);
            Assert.IsType<double>(r.Average);
            Assert.NotNull(r.Count);
            Assert.IsType<double>(r.Count);
            Assert.NotNull(r.Max);
            Assert.IsType<double>(r.Max);
            Assert.NotNull(r.Min);
            Assert.IsType<double>(r.Min);
            Assert.NotNull(r.Sum);
            Assert.IsType<double>(r.Sum);
        }
        [Fact]
        public async void StatsAggregationQuery_FailsOnNonNumeric()
        {
            const string name = "StatsAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.StatsAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }
        [Fact]
        public async void StatsAggregationQuery_FailsOnNonNumeric_Untyped()
        {
            const string name = "StatsAggregationQuery_FailsOnNonNumeric_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(BaseDocument).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.StatsAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }
        [Fact]
        public async void ExtendedStatsAggregationQuery_CanExecute()
        {
            const string name = "ExtendedStatsAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.ExtendedStatsAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as ExtendedStatsAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);

            Assert.NotNull(r.Average);
            Assert.IsType<double>(r.Average);
            Assert.NotNull(r.Count);
            Assert.IsType<double>(r.Count);
            Assert.NotNull(r.Max);
            Assert.IsType<double>(r.Max);
            Assert.NotNull(r.Min);
            Assert.IsType<double>(r.Min);
            Assert.NotNull(r.Sum);
            Assert.IsType<double>(r.Sum);

            Assert.NotNull(r.StandardDeviation);
            Assert.IsType<double>(r.StandardDeviation);
            Assert.NotNull(r.StandardDeviationPopulation);
            Assert.IsType<double>(r.StandardDeviationPopulation);
            Assert.NotNull(r.StandardDeviationSampling);
            Assert.IsType<double>(r.StandardDeviationSampling);
            
            Assert.NotNull(r.StandardDeviationBounds);
            Assert.IsType<Nest.StandardDeviationBounds>(r.StandardDeviationBounds);

            Assert.NotNull(r.StandardDeviationBounds.Upper);
            Assert.IsType<double>(r.StandardDeviationBounds.Upper);
            Assert.NotNull(r.StandardDeviationBounds.UpperPopulation);
            Assert.IsType<double>(r.StandardDeviationBounds.UpperPopulation);
            Assert.NotNull(r.StandardDeviationBounds.UpperSampling);
            Assert.IsType<double>(r.StandardDeviationBounds.UpperSampling);
            Assert.NotNull(r.StandardDeviationBounds.Lower);
            Assert.IsType<double>(r.StandardDeviationBounds.Lower);
            Assert.NotNull(r.StandardDeviationBounds.LowerPopulation);
            Assert.IsType<double>(r.StandardDeviationBounds.LowerPopulation);
            Assert.NotNull(r.StandardDeviationBounds.LowerSampling);
            Assert.IsType<double>(r.StandardDeviationBounds.LowerSampling);

            Assert.NotNull(r.SumOfSquares);
            Assert.IsType<double>(r.SumOfSquares);
            Assert.NotNull(r.Variance);
            Assert.IsType<double>(r.Variance);
            Assert.NotNull(r.VariancePopulation);
            Assert.IsType<double>(r.VariancePopulation);
            Assert.NotNull(r.VarianceSampling);
            Assert.IsType<double>(r.VarianceSampling);
        }
        [Fact]
        public async void ExtendedStatsAggregationQuery_CanExecute_Untyped()
        {
            const string name = "ExtendedStatsAggregationQuery_CanExecute_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.ExtendedStatsAggregation.Name, new DefaultAggregationField("taxful_total_price")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            var r = results.AggregationResults.First() as ExtendedStatsAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);

            Assert.NotNull(r.Average);
            Assert.IsType<double>(r.Average);
            Assert.NotNull(r.Count);
            Assert.IsType<double>(r.Count);
            Assert.NotNull(r.Max);
            Assert.IsType<double>(r.Max);
            Assert.NotNull(r.Min);
            Assert.IsType<double>(r.Min);
            Assert.NotNull(r.Sum);
            Assert.IsType<double>(r.Sum);

            Assert.NotNull(r.StandardDeviation);
            Assert.IsType<double>(r.StandardDeviation);
            Assert.NotNull(r.StandardDeviationPopulation);
            Assert.IsType<double>(r.StandardDeviationPopulation);
            Assert.NotNull(r.StandardDeviationSampling);
            Assert.IsType<double>(r.StandardDeviationSampling);

            Assert.NotNull(r.StandardDeviationBounds);
            Assert.IsType<Nest.StandardDeviationBounds>(r.StandardDeviationBounds);

            Assert.NotNull(r.StandardDeviationBounds.Upper);
            Assert.IsType<double>(r.StandardDeviationBounds.Upper);
            Assert.NotNull(r.StandardDeviationBounds.UpperPopulation);
            Assert.IsType<double>(r.StandardDeviationBounds.UpperPopulation);
            Assert.NotNull(r.StandardDeviationBounds.UpperSampling);
            Assert.IsType<double>(r.StandardDeviationBounds.UpperSampling);
            Assert.NotNull(r.StandardDeviationBounds.Lower);
            Assert.IsType<double>(r.StandardDeviationBounds.Lower);
            Assert.NotNull(r.StandardDeviationBounds.LowerPopulation);
            Assert.IsType<double>(r.StandardDeviationBounds.LowerPopulation);
            Assert.NotNull(r.StandardDeviationBounds.LowerSampling);
            Assert.IsType<double>(r.StandardDeviationBounds.LowerSampling);

            Assert.NotNull(r.SumOfSquares);
            Assert.IsType<double>(r.SumOfSquares);
            Assert.NotNull(r.Variance);
            Assert.IsType<double>(r.Variance);
            Assert.NotNull(r.VariancePopulation);
            Assert.IsType<double>(r.VariancePopulation);
            Assert.NotNull(r.VarianceSampling);
            Assert.IsType<double>(r.VarianceSampling);
        }
        [Fact]
        public async void ExtendedStatsAggregationQuery_FailsOnNonNumeric()
        {
            const string name = "ExtendedStatsAggregationQuery_FailsOnNonNumeric";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.ExtendedStatsAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }
        [Fact]
        public async void ExtendedStatsAggregationQuery_FailsOnNonNumeric_Untyped()
        {
            const string name = "ExtendedStatsAggregationQuery_FailsOnNonNumeric_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(BaseDocument).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.ExtendedStatsAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            DecomissionCluster(cluster);


            Assert.True(results != null);
            Assert.NotEmpty(results.Messages);
            Assert.Equal(-1, results.Total);
            Assert.Null(results.AggregationResults);
            Assert.Null(results.Results);
        }
        [Fact]
        public async void TermsAggregationQuery_CanExecute()
        {
            const string name = "TermsAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.TermsAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as TermsAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotEmpty(r.Buckets);
        }
        [Fact]
        public async void TermsAggregationQuery_RespectsSizeParam()
        {
            const string name = "TermsAggregationQuery_CanExecute";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { new TermsAggregationRequest(new DefaultAggregationField("manufacturer.keyword"), size: 20) });

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as TermsAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotEmpty(r.Buckets);
            var cnt = r.Buckets.Count();
            Assert.True(cnt > 10);
        }
        [Fact]
        public async void TermsAggregationQuery_CanExecuteSubAggs()
        {
            const string name = "TermsAggregationQuery_CanExecuteSubAggs";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria<SampleResult>(
                null,
                SampleIndices,
                aggregationRequests: new[] { 
                    new TermsAggregationRequest( 
                        new DefaultAggregationField("day_of_week"),
                        new[]{ new DefaultAggregationRequest(DefaultAggregationCache.StatsAggregation.Name, new DefaultAggregationField("taxful_total_price")) }
                    )}
                );

            var query = new AggregationQuery<SampleResult>(criteria);
            var results = cluster.Query(query) as AggregationQueryResults<SampleResult>;

            var r = results.AggregationResults.First() as TermsAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotEmpty(r.Buckets);
            Assert.All(r.Buckets, x =>
            {
                var z = x as NestedBucketResult;
                Assert.NotNull(z);
                Assert.NotEmpty(z.Nested);
            });
        }
        [Fact]
        public async void TermsAggregationQuery_CanExecute_Untyped()
        {
            const string name = "TermsAggregationQuery_CanExecute_Untyped";
            var cluster = Cluster.Create(GetArgs(name));

            
            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                aggregationRequests: new[] { new DefaultAggregationRequest(DefaultAggregationCache.TermsAggregation.Name, new DefaultAggregationField("day_of_week")) });

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            var r = results.AggregationResults.First() as TermsAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotEmpty(r.Buckets);
        }
        [Fact]
        public async void TermsAggregationQuery_CanExecuteSubAggs_Untyped()
        {
            const string name = "TermsAggregationQuery_CanExecuteSubAggs_Untyped";
            var cluster = Cluster.Create(GetArgs(name));


            var criteria = new AggregationQueryCriteria(
                null,
                typeof(SampleResult).FullName,
                SampleIndices,
                aggregationRequests: new[] {
                    new TermsAggregationRequest(
                        new DefaultAggregationField("day_of_week"),
                        new[]{ new DefaultAggregationRequest(DefaultAggregationCache.StatsAggregation.Name, new DefaultAggregationField("taxful_total_price")) }
                    )}
                );

            var query = new AggregationQuery(criteria);
            var results = cluster.Query(query) as AggregationQueryResults;

            var r = results.AggregationResults.First() as TermsAggregationResult;

            DecomissionCluster(cluster);

            Assert.True(results != null);
            Assert.NotEmpty(results.AggregationResults);
            Assert.NotNull(r);
            Assert.NotEmpty(r.Buckets);
            Assert.All(r.Buckets, x =>
            {
                var z = x as NestedBucketResult;
                Assert.NotNull(z);
                Assert.NotEmpty(z.Nested);
            });
        }
    }
}
