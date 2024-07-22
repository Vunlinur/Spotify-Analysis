using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using SpotifyAnalysis.Data;
using SpotifyAnalysis.Data.DataAccessLayer;
using SpotifyAnalysis.Data.DTO;
using SpotifyAnalysis.Data.SpotifyAPI;
using SpotifyAnalysis.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.RateLimiting;

namespace SpotifyAnalysis {
    public class Startup(IConfiguration configuration) {
		public IConfiguration Configuration { get; } = configuration;

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services) {
			services.AddRateLimiter(o => o.AddSlidingWindowLimiter(policyName: "sliding", options => {
				options.PermitLimit = 30;
				options.Window = TimeSpan.FromSeconds(30);
				options.SegmentsPerWindow = 10;
				options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
				options.QueueLimit = 4 * options.PermitLimit;
			}));
			services.AddRazorPages();
			services.AddServerSideBlazor();
			services.AddMudServices();
			services.AddSingleton<WeatherForecastService>();
			services.AddSingleton<SpotifyModule>();
			services.AddSingleton<SpotifyMudTheme>();
			services.AddScoped<ScopedData>();
			services.AddTransient<SpotifyContext>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}
			else {
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints => {
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});
		}
	}
}
