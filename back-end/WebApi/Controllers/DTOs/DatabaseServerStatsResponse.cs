using System;

namespace Raven.Yabt.WebApi.Controllers.DTOs
{
    public class DatabaseServerStatsResponse
    {
        public string PhysicalMemory { get; set; } = default!;

        public string FreeMemory { get; set; } = default!;

        public string WorkingSet { get; set; } = default!;

        public string TotalMemoryMapped { get; set; } = default!;

        public int ProcessorAffinity { get; set; }

        public TimeSpan UserProcessorTime { get; set; }

        public int CountOfIndexes { get; set; }

        public long CountOfDocuments { get; set; }

        public double RequestsPerSecond_FiveMinuteRate { get; set; }

        public double IndexedPerSecond_FiveMinuteRate { get; set; }
    }
}