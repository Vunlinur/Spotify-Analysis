using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SpotifyAPI.Web;


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

        public SpotifyClientStatic() {
            InitializeSpotifyClient();
        }

        private void InitializeSpotifyClient() {
			var config = SpotifyClientConfig.CreateDefault();

			var credentials = new ClientCredentialsRequest(
				Program.Config.GetValue<string>("ClientId"),
				Program.Config.GetValue<string>("ClientSecret")
			);

			try {
				// TODO handle server errors like Error SQL80001: An expression of non-boolean type specified in a context where a condition is expected.
				var response = new OAuthClient(config).RequestToken(credentials);
				response.Wait(); // Async await did not return with an error but timed out instead: TODO test when API's down
				SpotifyClient = new SpotifyClient(config.WithToken(response.Result.AccessToken));
			}
			catch (SecurityTokenExpiredException) {
				throw; // TODO refresh token
			}
		}
    }
}