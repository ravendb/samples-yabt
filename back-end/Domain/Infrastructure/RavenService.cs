using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Raven.Yabt.Database.Common.Models;

namespace Raven.Yabt.Domain.Infrastructure
{
	public class RavenService : IRavenService
	{
        private readonly HttpClient _httpClient;

        protected string GetFullUrl(string relative) => 
            $"{_httpClient.BaseAddress!.ToString().TrimEnd('/')}/{relative.TrimStart('/')}";

        public RavenService(HttpClient httpClient)
        {
            _httpClient = httpClient; 
        }

		public Task<MemoryStatistics> GetMemoryStats()
            => Get<MemoryStatistics>("/admin/debug/memory/stats");

		public Task<CpuStatistics> GetCpuStats()
            => Get<CpuStatistics>("/admin/debug/cpu/stats");

		public Task<DatabaseStatistics> GetDatabaseStatistics(string database)
            => Get<DatabaseStatistics>($"/databases/{database}/stats");

		public Task<DatabaseMetrics> GetDatabaseMetrics(string database)
            => Get<DatabaseMetrics>($"/databases/{database}/metrics");

        protected async Task<T> Get<T>(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GetFullUrl(url));

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.ReasonPhrase, default, response.StatusCode);
            }

            using var stream = await response.Content.ReadAsStreamAsync();

            var data = await JsonSerializer.DeserializeAsync<T>(stream);

            return data!;
        }
	}
}
