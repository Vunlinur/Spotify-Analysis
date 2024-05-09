using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyAnalysis.Data.SpotifyAPI {
    public static class SpotifyExtensions {
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

		public static void RemoveDuplicatePlaylists(this UserDTO user, Dictionary<string, PlaylistDTO> disctinct) {
			for (int i = 0; i < user.Playlists.Count; i++)
				if (disctinct.TryGetValue(user.Playlists[i].ID, out PlaylistDTO current))
					user.Playlists[i] = current;
				else
					disctinct.Add(user.Playlists[i].ID, user.Playlists[i]);
		}

		public static void RemoveDuplicateTracks(this IEnumerable<PlaylistDTO> collection, Dictionary<string, TrackDTO> disctinct) {
			foreach (PlaylistDTO entity in collection)
				for (int i = 0; i < entity.Tracks.Count; i++)
					if (disctinct.TryGetValue(entity.Tracks[i].ID, out TrackDTO current)) {
						current.Update(entity.Tracks[i]);
						entity.Tracks[i] = current;
					}
					else
						disctinct.Add(entity.Tracks[i].ID, entity.Tracks[i]);
		}

		public static void RemoveDuplicateAlbums(this IEnumerable<TrackDTO> collection, Dictionary<string, AlbumDTO> disctinct) {
			foreach (TrackDTO entity in collection)
				if (disctinct.TryGetValue(entity.Album.ID, out AlbumDTO current)) {
					current.Update(entity.Album);
					entity.Album = current;
				}
				else
					disctinct.Add(entity.Album.ID, entity.Album);
		}

		public static void RemoveDuplicateArtists(this IEnumerable<TrackDTO> collection, Dictionary<string, ArtistDTO> disctinct) {
			foreach (TrackDTO entity in collection) {
				for (int i = 0; i < entity.Artists.Count; i++)
					if (disctinct.TryGetValue(entity.Artists[i].ID, out ArtistDTO current)) {
						current.Update(entity.Artists[i]);
						entity.Artists[i] = current;
					}
					else
						disctinct.Add(entity.Artists[i].ID, entity.Artists[i]);
			}
		}

		public static void RemoveDuplicateArtists(this IEnumerable<AlbumDTO> collection, Dictionary<string, ArtistDTO> disctinct) {
			foreach (AlbumDTO entity in collection) {
				for (int i = 0; i < entity.Artists.Count; i++)
					if (disctinct.TryGetValue(entity.Artists[i].ID, out ArtistDTO current)) {
						current.Update(entity.Artists[i]);
						entity.Artists[i] = current;
					}
					else
						disctinct.Add(entity.Artists[i].ID, entity.Artists[i]);
			}
		}
	}
}
