using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Raven.Yabt.Database.Common.Configuration;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.WebApi.Controllers.DTOs;

namespace Raven.Yabt.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        #region GET requests --------------------

		/// <summary>
		///		Get various database stats such as memory, cpu usage, etc.
		/// </summary>
        [HttpGet("database-stats")]
        public async Task<DatabaseServerStatsResponse> GetDatabaseStats([FromServices]IRavenService service, [FromServices]DatabaseSettings dbSettings)
        {
            var tasks = new List<Task>();

            var cpuStats = service.GetCpuStats();
            var memStats = service.GetMemoryStats();
            var dbMetrics = service.GetDatabaseMetrics(dbSettings.DbName);
            var dbStats = service.GetDatabaseStatistics(dbSettings.DbName);

            tasks.AddRange(new Task[] { cpuStats, memStats, dbMetrics, dbStats });

            await Task.WhenAll(tasks);

            return await Task.FromResult(new DatabaseServerStatsResponse()
            {
                FreeMemory = memStats.Result.FreeMem,
                PhysicalMemory = memStats.Result.PhysicalMemory,
                TotalMemoryMapped = memStats.Result.Humane!.TotalMemoryMapped,
                WorkingSet = memStats.Result.Humane!.WorkingSet,
                CountOfDocuments = dbStats.Result.CountOfDocuments,
                CountOfIndexes = dbStats.Result.CountOfIndexes,
                RequestsPerSecond_FiveMinuteRate = dbMetrics.Result.Requests.RequestsPerSec.FiveMinuteRate,
                IndexedPerSecond_FiveMinuteRate = dbMetrics.Result.MapIndexes.IndexedPerSec.FiveMinuteRate
            });
        }
        #endregion / GET requests --------------------
    }
}