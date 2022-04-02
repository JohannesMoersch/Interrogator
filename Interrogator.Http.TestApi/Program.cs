using Microsoft.AspNetCore;

namespace Interrogator.Http.TestApi
{
	public class Program
	{
		public static void Main(string[] args)
			=> CreateWebHostBuilder(args)
				.Build()
				.Run();

		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
			=> WebHost
				.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
	}
}
