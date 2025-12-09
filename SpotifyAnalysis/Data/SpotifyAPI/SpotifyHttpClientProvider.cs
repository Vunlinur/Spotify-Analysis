using SpotifyAPI.Web.Http;
using System;
using System.Net.Http;
using System.Threading.RateLimiting;

namespace SpotifyAnalysis.Data.SpotifyAPI {
    /// <summary>
    /// A manager for NetHttpClient which is used by the Spotify API client,
    /// as it is best to reuse HttpClient instances:
    /// https://johnnycrazy.github.io/SpotifyAPI-NET/docs/configuration
    /// Uses a rate limiting handler.
    /// 
    /// Presumably, Spotify has "limit of 100 requests per hour for each user token
    /// and 25 requests per second for each application token" according to:
    /// https://apipark.com/technews/O4zBQwTk.html
    /// </summary>
    /// 
    public class SpotifyHttpClientProvider : IDisposable {
        public NetHttpClient HttpClient { get; init; }

        private readonly SlidingWindowRateLimiter rateLimiter;
   
        public SpotifyHttpClientProvider() {
            // TODO drive from settings
            var options = new SlidingWindowRateLimiterOptions {
                PermitLimit = 20,
                Window = TimeSpan.FromSeconds(1),
                SegmentsPerWindow = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 200 // Allow queuing up to 100 requests
            };
            rateLimiter = new SlidingWindowRateLimiter(options);
            var innerHandler = new HttpClientHandler();
            var rateLimitHandler = new RateLimitingDelegatingHandler(rateLimiter) { InnerHandler = innerHandler };
            var httpClient = new HttpClient(rateLimitHandler);
            HttpClient = new NetHttpClient(httpClient);
        }

        public void Dispose() {
            rateLimiter?.Dispose();
        }
    }
}

