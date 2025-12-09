using Microsoft.Extensions.DependencyInjection;


namespace SpotifyAnalysis.Data.SpotifyAPI {
    public static class ConfigureServices {
        public static IServiceCollection AddSpotifyAPI(this IServiceCollection services)
            => services
            .AddSingleton<SpotifyHttpClientProvider>()
            .AddSingleton<SpotifyClientStatic>()
            .AddScoped<SpotifyClientScoped>()
            .AddScoped(CreateSpotifyModuleFactory);

        private static SpotifyModuleFactory CreateSpotifyModuleFactory(System.IServiceProvider sp)
            => new(sp.GetService<SpotifyClientStatic>(), sp.GetService<SpotifyClientScoped>());
    }
}