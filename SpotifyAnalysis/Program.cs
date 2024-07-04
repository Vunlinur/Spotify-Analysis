using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;

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
				.ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseWebRoot("wwwroot");
                    webBuilder.UseStaticWebAssets();
                });
    }
}