using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using SpotifyAnalysis.Data.Database;
using SpotifyAnalysis.Data.Common;
using SpotifyAnalysis.Data.SpotifyAPI;
using SpotifyAnalysis.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace SpotifyAnalysis {
    public class Startup(IConfiguration configuration) {
		public IConfiguration Configuration { get; } = configuration;

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services) {
			services.AddRazorPages();
			services.AddServerSideBlazor();
			services.AddMudServices();
            services.AddSpotifyAPI();
            services.AddSingleton<SpotifyMudTheme>();
            services.AddScoped<ScopedData>();
            services.AddScoped<ProtectedLocalStorage>();
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
