using SpotifyAnalysis.Data.DTO;
using SpotifyAnalysis.Data.SpotifyAPI;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyAnalysis.Data.DataAccessLayer {
    public static class DBExtensions {
		public static IEnumerable<TEnt> FindNewEntities<TEnt, TKey>(this IEnumerable<TEnt> current, IEnumerable<TEnt> source, Func<TEnt, TKey> keySelector) where TEnt : class {
			var existingKeys = current.Select(keySelector).ToHashSet();
			return source.Where(e => !existingKeys.Contains(keySelector(e)));
		}

		public static bool UpdateOrAdd(this IDictionary<string, PlaylistDTO> dict, FullPlaylist source, out PlaylistDTO outPlaylist) {
			bool found = dict.TryGetValue(source.Id, out PlaylistDTO playlist);
			if (!found) {
				// this shouldn't really happen as we should already have
				// all the playlists in the db before adding their details
				playlist = source.ToPlaylistDTO();
				dict.Add(playlist.ID, playlist);
			}
			else
				playlist.Update(source);
			outPlaylist = playlist;
			return found;
        }

        public static void Update(this PlaylistDTO playlist, FullPlaylist source) {
            playlist.Name = source.Name;
            playlist.OwnerID = source.Owner.Id;
            playlist.OwnerName = source.Owner.DisplayName;
            playlist.Followers = source.Followers.Total;
            playlist.SnapshotID = source.SnapshotId;
            playlist.TracksTotal = source.Tracks.Total;
            playlist.Images = source.Images.ToImageDTOs();
        }

        public static bool UpdateOrAdd(this IDictionary<string, ArtistDTO> dict, SimpleArtist source, out ArtistDTO outArtist) {
			bool found = dict.TryGetValue(source.Id, out ArtistDTO artist);
			if (!found) {
				artist = source.ToArtistDTO();
				dict.Add(artist.ID, artist);
			}
			else
				artist.Update(source);
			outArtist = artist;
			return found;
        }

        public static void Update(this ArtistDTO artist, SimpleArtist source) {
            artist.Name = source.Name;
        }

        public static void Update(this ArtistDTO artist, FullArtist source) {
            artist.Genres = source.Genres;
            artist.Popularity = source.Popularity;
            artist.Images = source.Images.ToImageDTOs();
        }

        public static bool UpdateOrAdd(this IDictionary<string, AlbumDTO> dict, SimpleAlbum source, out AlbumDTO outAlbum) {
			bool found = dict.TryGetValue(source.Id, out AlbumDTO album);
			if (!found) {
				album = source.ToAlbumDTO();
				dict.Add(album.ID, album);
			}
			else
                album.Update(source);
            outAlbum = album;
			return found;
		}

        public static void Update(this AlbumDTO album, SimpleAlbum source) {
            album.Name = source.Name;
            album.ReleaseDate = source.ReleaseDate;
            album.TotalTracks = source.TotalTracks;
            album.Images = source.Images.ToImageDTOs();
        }

        public static bool UpdateOrAdd(this IDictionary<string, TrackDTO> dict, FullTrack source, out TrackDTO outTrack) {
			bool found = dict.TryGetValue(source.Id, out TrackDTO track);
			if (!found) {
				track = source.ToTrackDTO();
				dict.Add(track.ID, track);
			}
			else
				track.Update(source);
			outTrack = track;
			return found;
        }

        public static void Update(this TrackDTO track, FullTrack source) {
            track.Name = source.Name;
            track.DurationMs = source.DurationMs;
            track.Popularity = source.Popularity;
        }
    }
}
