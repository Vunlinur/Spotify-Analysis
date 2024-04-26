using Microsoft.Extensions.Configuration;
using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data.SpotifyAPI {


	public class SpotifyModule {
		private const int maxDegreeOfParallelism = 3; // Adjust this based on your requirements
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
		public async Task<IList<TrackDTO>> GetPlaylistTracksAsync(string playlistId) {
			// TODO maybe rework to Playlists.Get() so we can fill the extra data we don't get in SimplePlaylist
			var tracksPages = await SpotifyClient.Playlists.GetItems(playlistId);
			var tracksAll = await SpotifyClient.PaginateAll(tracksPages);
			return tracksAll.ToFullTracks().Select(p => p.ToTrackDTO()).ToList();
		}

		/**
		 * Get full details of the items of multiple playlists with given IDs.
		 * https://developer.spotify.com/documentation/web-api/reference/get-playlists-tracks
		 */
		public async Task<IList<TrackDTO>> GetMultiplePlaylistsTracksAsync(IEnumerable<string> playlistIds) {
			// TODO cancellation token?
			var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
			var tasks = new List<Task<IList<TrackDTO>>>();

			// Create tasks to process each playlist ID asynchronously
			foreach (var playlistId in playlistIds) {
				async Task<IList<TrackDTO>> GetTracks() {
					try {
						return await GetPlaylistTracksAsync(playlistId);
					} finally {
						semaphore.Release(); // Release the semaphore slot when done
					}
				}

				await semaphore.WaitAsync(); // Wait for a semaphore slot to limit concurrency
				tasks.Add(Task.Run(GetTracks));
			}
			var results = await Task.WhenAll(tasks);
			return results.SelectMany(x => x).ToList();
		}
	}
}