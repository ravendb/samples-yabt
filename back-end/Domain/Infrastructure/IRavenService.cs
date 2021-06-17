using System.Threading.Tasks;

using Raven.Yabt.Database.Common.Models;

namespace Raven.Yabt.Domain.Infrastructure
{
    public interface IRavenService
    {
        Task<MemoryStatistics> GetMemoryStats();

        Task<CpuStatistics> GetCpuStats();

        Task<DatabaseStatistics> GetDatabaseStatistics(string databaseName);

        Task<DatabaseMetrics> GetDatabaseMetrics(string databaseName);
    }
}
