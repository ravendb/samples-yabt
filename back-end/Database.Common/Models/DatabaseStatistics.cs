namespace Raven.Yabt.Database.Common.Models
{
    public class DatabaseStatistics
    {
        public int CountOfIndexes { get; set; }

        public long CountOfDocuments { get; set; }

        public DatabaseSize SizeOnDisk { get; set; } = default!;
    }

    public class DatabaseSize
    {
        public string HumaneSize { get; set; } = default!;

        public long SizeInBytes { get; set; }
    } 
}