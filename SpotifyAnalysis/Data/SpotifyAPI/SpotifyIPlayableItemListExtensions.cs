using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyAnalysis.Data.SpotifyAPI {
    public static class SpotifyIPlayableItemListExtensions {
		public static IEnumerable<FullTrack> ToFullTracks(this IEnumerable<PlaylistTrack<IPlayableItem>> playableItems) {
			foreach (PlaylistTrack<IPlayableItem> item in playableItems)
				if (item.Track is FullTrack track)
					yield return track;
		}

		public static IEnumerable<FullEpisode> ToFullEpisodes(this IEnumerable<PlaylistTrack<IPlayableItem>> playableItems) {
			foreach (PlaylistTrack<IPlayableItem> item in playableItems)
				if (item.Track is FullEpisode track)
					yield return track;
		}

		public static void RemoveDuplicateAlbums(this IEnumerable<TrackDTO> collection, Dictionary<string, AlbumDTO> disctinct) {
			foreach (TrackDTO entity in collection)
				if (disctinct.TryGetValue(entity.Album.ID, out AlbumDTO current))
					entity.Album = current;
		}

		public static void RemoveDuplicateArtists(this IEnumerable<TrackDTO> collection, Dictionary<string, ArtistDTO> disctinct) {
			foreach (TrackDTO entity in collection) {
				ArtistDTO artist;
				for (int i = 0; i < entity.Artists.Count; i++) {
					artist = entity.Artists[i];
					if (disctinct.TryGetValue(artist.ID, out ArtistDTO current))
						entity.Artists[i] = current;
				}
			}
		}
	}
}
