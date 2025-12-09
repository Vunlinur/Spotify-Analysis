using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.RateLimiting;
using System.Net;

namespace SpotifyAnalysis.Data.SpotifyAPI {
    /// <summary>
    /// HTTP message handler that applies rate limiting to Spotify API requests.
    /// Intercepts all HTTP requests and ensures they comply with rate limits before forwarding.
    /// </summary>
    public class RateLimitingDelegatingHandler(RateLimiter rateLimiter) : DelegatingHandler {
        private readonly RateLimiter rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            // Only rate limit requests to Spotify API
            if (ShouldRateLimit(request))
                return await base.SendAsync(request, cancellationToken);

            using var lease = await rateLimiter.AcquireAsync(1, cancellationToken);
            if (lease.IsAcquired)
                return await base.SendAsync(request, cancellationToken);

            // If we can't acquire a permit, wait for the retry delay
            if (!lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                // No retry info, return 429 immediately
                return RateLimitExceeded();

            await Task.Delay(retryAfter, cancellationToken);

            // Try again after waiting
            using var retryLease = await rateLimiter.AcquireAsync(1, cancellationToken);
            if (!retryLease.IsAcquired)
                // Still can't acquire, return 429
                return RateLimitExceeded();

            // Forward the request to the next handler in the pipeline
            return await base.SendAsync(request, cancellationToken);
        }

        static bool ShouldRateLimit(HttpRequestMessage request) =>
            request.RequestUri?.Host.Contains(".spotify.com", StringComparison.OrdinalIgnoreCase) == true;

        static HttpResponseMessage RateLimitExceeded() =>
            new (HttpStatusCode.TooManyRequests) { ReasonPhrase = "Rate limit exceeded" };
    }
}

