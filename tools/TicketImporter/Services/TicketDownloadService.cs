using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Raven.Yabt.TicketImporter.Infrastructure;

namespace Raven.Yabt.TicketImporter.Services
{
	internal class TicketDownloadService : IHostedService
	{
		private readonly IGitHubService _gitHubService;

		public TicketDownloadService(IGitHubService gitHubService)
		{
			_gitHubService = gitHubService;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
