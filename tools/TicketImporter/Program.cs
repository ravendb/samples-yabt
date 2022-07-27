using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace Raven.Yabt.TicketImporter;

class Program
{
	public static Task Main(string[] args)
	{
		return Startup.CreateHostBuilder(args)
		              .RunConsoleAsync();
	}		
}