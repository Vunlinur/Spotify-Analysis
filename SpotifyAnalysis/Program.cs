using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MudBlazor.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace SpotifyAnalysis {
	public class Program {
		public static IConfigurationRoot Config { get; private set; }

		public static void Main(string[] args) {
            Config = PrepareConfig();
            CreateHostBuilder(args).Build().Run();
		}

		public static IConfigurationRoot PrepareConfig() {
			return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true)
                .Build();
        }

		public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
				.ConfigureServices(ConfigureServices)
				.ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseWebRoot("wwwroot");
                    webBuilder.UseStaticWebAssets();
                });

        public static void ConfigureServices(IServiceCollection services) {
            services.AddRateLimiter(o => o.AddSlidingWindowLimiter(policyName: "sliding", options => {
                    options.PermitLimit = 30;
                    options.Window = TimeSpan.FromSeconds(30);
                    options.SegmentsPerWindow = 10;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 4 * options.PermitLimit;
                }));
            services.AddMudServices();
        }
    }
}