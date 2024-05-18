using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data.SpotifyAPI {


	public class SpotifyModule {
		private SpotifyClient SpotifyClient;


		public SpotifyModule() {
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

		/**
		 * Get public profile information about a Spotify user.
		 * https://developer.spotify.com/documentation/web-api/reference/get-users-profile
		 */
		public async Task<UserDTO> GetUserProfile(string userID) {
			var userData = await SpotifyClient.UserProfile.Get(userID);
			return userData.ToUserDTO();
		}

		/**
		 * Get a list of the playlists owned or followed by a Spotify user.
		 * https://developer.spotify.com/documentation/web-api/reference/get-list-users-playlists
		 */
		public async Task<IList<PlaylistDTO>> GetUsersPublicPlaylistsAsync(string userID) {
			var playlistsPages = await SpotifyClient.Playlists.GetUsers(userID);
			var fullPlaylists = await SpotifyClient.PaginateAll(playlistsPages);
			return fullPlaylists.Select(p => p.ToPlaylistDTO()).ToList();
		}

		/**
		 * Get full details of the playlist with given ID.
		 * https://developer.spotify.com/documentation/web-api/reference/get-playlists-tracks
		 */
		public async Task<FullPlaylist> GetPlaylistAsync(PlaylistDTO playlist) {
			return await SpotifyClient.Playlists.Get(playlist.ID);
		}

		/**
		 * Get tracks from the given Paging.
		 * https://developer.spotify.com/documentation/web-api/reference/get-playlists-tracks
		 */
		public async Task<List<FullTrack>> GetTracksAsync(Paging<PlaylistTrack<IPlayableItem>> paging) {
			var allPlayableItems = await SpotifyClient.PaginateAll(paging);
			return allPlayableItems.ToFullTracks().ToList();
        }

        /**
		 * Get multiple artists by their ids.
		 */
        public async Task<List<FullArtist>> GetArtistsAsync(IList<string> ids) {
            var artistsResponse = await SpotifyClient.Artists.GetSeveral(new ArtistsRequest(ids));
            return artistsResponse.Artists;
        }
    }
}