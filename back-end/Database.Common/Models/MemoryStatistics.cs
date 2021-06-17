namespace Raven.Yabt.Database.Common.Models
{
    public class MemoryStatistics
    {
        public string PhysicalMemory { get; set; } = default!;

        public string FreeMem { get; set; } = default!;

        public MemoryStatisticsHumane? Humane { get; set; }
    }

    public class MemoryStatisticsHumane 
    {
        public string WorkingSet { get; set; } = default!;

        public string TotalMemoryMapped { get; set; } = default!;
    }
}