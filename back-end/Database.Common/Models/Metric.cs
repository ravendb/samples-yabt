namespace Raven.Yabt.Database.Common.Models
{
    public class Metric
    {
        public int Current { get; set; }

        public long Count { get; set; }

        public double MeanRate { get; set; }

        public double OneMinuteRate { get; set; }

        public double FiveMinuteRate { get; set; }

        public double FifteenMinuteRate { get; set; }
    } 
}