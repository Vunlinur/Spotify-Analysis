using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data {
	public class Spotify {

		public SpotifyClient spotifyClient { get; private set; }
		
		public Spotify() {
			var config = SpotifyClientConfig.CreateDefault();


			var credentials = new ClientCredentialsRequest(
				Program.Config.ClientId,
				Program.Config.ClientSecret
			);
			var response = new OAuthClient(config).RequestToken(credentials);
			response.Wait();

			spotifyClient = new SpotifyClient(config.WithToken(response.Result.AccessToken));
		}
	}
}
