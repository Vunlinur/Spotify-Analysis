using Microsoft.Extensions.Configuration;
using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data.SpotifyAPI {

/*
 * Playlists
1. get user's playlists from spotify
2. get the IDs of these playlists
3. check which IDs are present in DB
4. check if SnapshotIDs of the playlist we have match latest
4. get playlist with remaining IDs from spotify
5. load these playlits to DB

* Tracks
1. get a playlist from spotify
2. cast the Tracks to FullTrack or FullEpisode
CACHING???
 */

	public class SpotifyModule {
		private SpotifyClient SpotifyClient { get; }

		public SpotifyModule() {
			var config = SpotifyClientConfig.CreateDefault();

			var credentials = new ClientCredentialsRequest(
				Program.Config.GetValue<string>("ClientId"),
				Program.Config.GetValue<string>("ClientSecret")
			);
			var response = new OAuthClient(config).RequestToken(credentials);
			response.Wait();

			// TODO refresh token
			SpotifyClient = new SpotifyClient(config.WithToken(response.Result.AccessToken));
		}

        /**
		 * Gets all public playlists of the given userID.
		 */
        public async Task<IList<PlaylistDTO>> GetUsersPublicPlaylistsAsync(string userID) {
            var playlistsTask = await SpotifyClient.Playlists.GetUsers(userID);
			var fullPlaylists = await SpotifyClient.PaginateAll(playlistsTask);
			return fullPlaylists.Select(p => p.ToPlaylistDTO()).ToList();
        }
    }
}