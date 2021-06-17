using System;

namespace Raven.Yabt.Database.Common.Models
{
    public class CpuStatistics
    {
        public CpuStatisticsItem[] CpuStats { get; set; } = default!;
    }

    public class CpuStatisticsItem 
    {
        public short ProcessorAffinity { get; set; }

        public string UserProcessorTime { get; set; }
    }
}