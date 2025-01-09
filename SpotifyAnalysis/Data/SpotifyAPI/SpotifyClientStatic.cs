﻿using Microsoft.Extensions.Configuration;
using SpotifyAPI.Web;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace SpotifyAnalysis.Data.SpotifyAPI {
    /// <summary>
    /// Provides a static SpotifyClient instance for general server API interactions without user-specific context.
    /// This client is initialized using Spotify's Client Credentials Flow and is suitable for operations
    /// that do not require user authentication, such as retrieving public data.
    /// </summary>
    /// <remarks>
    /// The client is initialized during application startup with credentials retrieved from configuration.
    /// Ensure `ClientId` and `ClientSecret` are correctly configured in the application secrets or settings.
    /// </remarks>
    public class SpotifyClientStatic {
		public SpotifyClient SpotifyClient { get; set; }

        private Timer refreshTimer;

        public SpotifyClientStatic() {
            Task.Run(InitializeSpotifyClient);
        }

        private void InitializeSpotifyClient() {
			var config = SpotifyClientConfig.CreateDefault();
			var credentials = new ClientCredentialsRequest(
				Program.Config.GetValue<string>("ClientId"),
				Program.Config.GetValue<string>("ClientSecret")
			);

            int refreshTime;
            try {
                // TODO handle server errors like Error SQL80001: An expression of non-boolean type specified in a context where a condition is expected.
                var response = new OAuthClient(config).RequestToken(credentials);
                response.Wait(); // Async await did not return with an error but timed out instead: TODO test when API's down
                SpotifyClient = new SpotifyClient(config.WithToken(response.Result.AccessToken));

                refreshTime = response.Result.ExpiresIn - 30; // Refresh 30 seconds before expiry
            }
            catch (Exception e) {
                refreshTime = 30;  // retry in 30 sec
                // TODO logging
            }

            // TODO no need to refresh the token when noone is connected to the server:
            // https://stackoverflow.com/questions/19313339/refreshing-access-token-only-when-necessary
            // https://www.reddit.com/r/Blazor/comments/10s5t7p/blazor_server_how_to_count_active_connections/
            refreshTimer?.Dispose();
            refreshTimer = new Timer(_ => InitializeSpotifyClient(), null, TimeSpan.FromSeconds(refreshTime), Timeout.InfiniteTimeSpan);
        }
    }
}