using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SpotifyAnalysis.Data.DataAccessLayer;
using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data.SpotifyAPI {


	public class SpotifyModule {
		private const int maxDegreeOfParallelism = 3; // Adjust based on Spotify API capacity
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
		 * Get full details of the items of a playlist with given ID.
		 * https://developer.spotify.com/documentation/web-api/reference/get-playlists-tracks
		 */
		public async Task GetPlaylistTracksAsync(PlaylistDTO playlist) {
			var fullPlaylist = await SpotifyClient.Playlists.Get(playlist.ID);
			var allPlayableItems = await SpotifyClient.PaginateAll(fullPlaylist.Tracks);
			var allTracks = allPlayableItems.ToFullTracks().Select(p => p.ToTrackDTO());
			var newTracks = allTracks.FindNewEntities(allTracks, t => t.ID);
			playlist.Tracks.AddRange(newTracks);
			playlist.Followers = fullPlaylist.Followers.Total;
			playlist.TracksTotal = playlist.Tracks.Count;
		}

		/**
		 * Get full details of the items of multiple playlists with given IDs.
		 * https://developer.spotify.com/documentation/web-api/reference/get-playlists-tracks
		 */
		public async Task GetMultiplePlaylistsTracksAsync(IEnumerable<PlaylistDTO> playlists) {
			// TODO cancellation token?
			var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
			var tasks = new List<Task>();

			// Create tasks to process each playlist ID asynchronously
			foreach (var playlist in playlists) {
				async Task GetTracks() {
					try {
						await GetPlaylistTracksAsync(playlist);
					} finally {
						semaphore.Release(); // Release the semaphore slot when done
					}
				}

				await semaphore.WaitAsync(); // Wait for a semaphore slot to limit concurrency
				tasks.Add(Task.Run(GetTracks));
			}
			await Task.WhenAll(tasks);
		}
	}
}