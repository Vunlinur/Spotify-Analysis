using Microsoft.Extensions.Configuration;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data.SpotifyAPI {
	public class Spotify {
		public FullArtists AllArtists { get; } = new FullArtists();
		public UserData UserData { get; set; }

		private SpotifyClient SpotifyClient { get; }

		public Spotify() {
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
		public async Task GetUsersPublicPlaylistsAsync(UserData userData) {
			UserData = userData;
			var playlistsTask = await SpotifyClient.Playlists.GetUsers(UserData.ID);
			UserData.FullPlaylists = new FullPlaylists(await SpotifyClient.PaginateAll(playlistsTask));
		}

		/**
		 * Gets all tracks present on the given playlists.
		 * Raw FullPlaylist has no info on its tracks.
		 * Since Playlists.GetItems returns Paging<PlaylistTrack<..>>, we have no simple way to cache separate playlist.
		 */
		public async Task<FullTracks> GetAllTracksAsync(IEnumerable<FullPlaylist> playlists) {
			async Task<Playlist> GetAllPlaylistTracksAsync(FullPlaylist playlist) {
				var fullPlaylist = new Playlist(playlist);
				if (UserData.FullPlaylists[playlist.Id].FullTracks.Any())
					return await Task.Run(() => {
						playlists = playlists.ToList();
						foreach (var track in UserData.FullPlaylists[playlist.Id].FullTracks)
							fullPlaylist.FullTracks.Add(track);
						return fullPlaylist;
					});
				else {
					var tracksTask = await SpotifyClient.Playlists.GetItems(playlist.Id);
					var tracksAllTask = await SpotifyClient.PaginateAll(tracksTask);
					foreach (var song in tracksAllTask)
						fullPlaylist.FullTracks.Add(song.Track as FullTrack);
					return fullPlaylist;
				}
			}

			var getTracksTasks = playlists.Select(p => GetAllPlaylistTracksAsync(p));
			var fullTracks = new FullTracks();
			foreach (var playlist in await Task.WhenAll(getTracksTasks))
				foreach (var track in playlist.FullTracks)
					fullTracks.Add(track);
			return fullTracks;
		}

		/**
		 * Caches the details of all artists listed under given fullTracks, if not cached already.
		 * Most common use is for later retrieval from the cache.
		 */
		public async Task<FullArtists> GetAllArtistsAsync(IEnumerable<FullTrack> fullTracks) {
			var artistsSet = new HashSet<SimpleArtist>();
			foreach (var track in fullTracks)
				foreach (var artist in track.Artists)
					if (!artistsSet.Contains(artist) && !AllArtists.Contains(artist.Id))
						artistsSet.Add(artist);

			int size = 8;
			var tasks = new List<Task<ArtistsResponse>>();
			// Divide into chunks of <size> times HashSet<SimpleArtist>
			var chunks = artistsSet.Select((s, i) => artistsSet.Skip(i * size).Take(size)).Where(a => a.Any());
			foreach (var chunk in chunks) {
				var task = SpotifyClient.Artists.GetSeveral(new ArtistsRequest(chunk.Select(c => c.Id).ToList()));
				tasks.Add(task);
				await Task.Delay(100);
			}
			await Task.WhenAll(tasks.ToArray());
			foreach (var artist in tasks.SelectMany(t => t.Result.Artists))
				AllArtists.Add(artist);
			return AllArtists;
		}
	}
}