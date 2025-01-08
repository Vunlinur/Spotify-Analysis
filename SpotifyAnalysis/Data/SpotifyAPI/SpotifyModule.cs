using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace SpotifyAnalysis.Data.SpotifyAPI {
    /// <summary>
    /// A collection of thin Spotify API abstractions.
    /// </summary>
    /// <param name="spotifyClient">A SpotifyClient to use for connecting. Can be logged in or not.</param>
    public class SpotifyModule(SpotifyClient spotifyClient) {
		private readonly SpotifyClient SpotifyClient = spotifyClient;

        /**
		 * Get public profile information about a Spotify user.
		 * https://developer.spotify.com/documentation/web-api/reference/get-users-profile
		 */
        public async Task<PublicUser> GetUserProfile(string userID) {
			try {
	            return await SpotifyClient.UserProfile.Get(userID);
            }
			catch (APIException e) {
				// The only error this endpoint seems to return is 500 - throw if anything else
                if (e.Response.StatusCode == HttpStatusCode.InternalServerError)
					return null;
				throw;
            }
		}

		/**
		 * Get a list of the playlists owned or followed by a Spotify user.
		 * https://developer.spotify.com/documentation/web-api/reference/get-list-users-playlists
		 */
		public async Task<IList<FullPlaylist>> GetUsersPublicPlaylistsAsync(string userID) {
			var playlistsPages = await SpotifyClient.Playlists.GetUsers(userID);  // Returns also priv playlists when queried for the logged-in user
            return await SpotifyClient.PaginateAll(playlistsPages);
		}

		/**
		 * Get full details of the playlist with given ID.
		 * https://developer.spotify.com/documentation/web-api/reference/get-playlists-tracks
		 */
		public async Task<FullPlaylist> GetPlaylistAsync(string playlistId) {
			return await SpotifyClient.Playlists.Get(playlistId);
		}

		/**
		 * Get tracks from the given Paging. Omits local tracks.
		 * https://developer.spotify.com/documentation/web-api/reference/get-playlists-tracks
		 */
		public async Task<List<FullTrack>> GetTracksAsync(Paging<PlaylistTrack<IPlayableItem>> paging) {
			var allPlayableItems = await SpotifyClient.PaginateAll(paging);
			return PlayableItemTo<FullTrack>(allPlayableItems).Where(t => t.Id is not null).ToList();  // ID is null when track is local
        }

        /**
		 * Get multiple artists by their ids.
		 */
        public async Task<List<FullArtist>> GetArtistsAsync(IList<string> ids) {
            var artistsResponse = await SpotifyClient.Artists.GetSeveral(new ArtistsRequest(ids));
            return artistsResponse.Artists;
        }

        private static IEnumerable<T> PlayableItemTo<T>(IEnumerable<PlaylistTrack<IPlayableItem>> playableItems) where T : IPlayableItem {
            foreach (PlaylistTrack<IPlayableItem> item in playableItems)
                if (item.Track is T track)
                    yield return track;
        }
    }
}