namespace Raven.Yabt.Database.Common.Models
{
    public class DatabaseMetrics
    {
        public DatabaseMetricsRequests Requests { get; set; } = default!;

        public DatabaseMetricsIndexes MapIndexes { get; set; } = default!;
    }

    public class DatabaseMetricsRequests
    {
        public Metric RequestsPerSec { get; set; } = default!;
    }

    public class DatabaseMetricsIndexes
    {
        public Metric IndexedPerSec { get; set; } = default!;
    }
}