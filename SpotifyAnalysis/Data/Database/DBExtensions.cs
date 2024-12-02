using SpotifyAnalysis.Data.DTO;
using SpotifyAnalysis.Data.SpotifyAPI;
using SpotifyAPI.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyAnalysis.Data.Database {
    public static class DBExtensions {
		public static IEnumerable<TEnt> FindNewEntities<TEnt, TKey>(this IEnumerable<TEnt> current, IEnumerable<TEnt> source, Func<TEnt, TKey> keySelector) where TEnt : class {
			var existingKeys = current.Select(keySelector).ToHashSet();
			foreach (TEnt element in source)
				if (!existingKeys.Contains(keySelector(element)))
					yield return element;
		}

		public static bool UpdateOrAdd(this ConcurrentDictionary<string, PlaylistDTO> dict, FullPlaylist source, out PlaylistDTO dto) {
            bool found = false;
            dto = dict.AddOrUpdate(
                source.Id,
                id => {
                    // this shouldn't really happen as we should already have
                    // all the playlists in the db before adding their details
                    found = false;
                    return source.ToPlaylistDTO();
                },
                (id, existing) => {
                    found = true;
                    existing.Update(source);
                    return existing;
                }
            );
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

        public static bool UpdateOrAdd(this ConcurrentDictionary<string, ArtistDTO> dict, SimpleArtist source, out ArtistDTO dto) {
            bool found = false;
            dto = dict.AddOrUpdate(
                source.Id,
                id => {
                    found = false;
                    return source.ToArtistDTO();
                },
                (id, existing) => {
                    found = true;
                    existing.Update(source);
                    return existing;
                }
            );
            return found;
        }

        public static void Update(this ArtistDTO artist, SimpleArtist source) {
            artist.Name = source.Name;
        }

        public static void Update(this ArtistDTO artist, FullArtist source) {
            artist.Genres = source.Genres;
            artist.Popularity = source.Popularity;
			artist.Genres = source.Genres;
            artist.Images = source.Images.ToImageDTOs();
        }

        public static bool UpdateOrAdd(this ConcurrentDictionary<string, AlbumDTO> dict, SimpleAlbum source, out AlbumDTO dto) {
            bool found = false;
            dto = dict.AddOrUpdate(
                source.Id,
                id => {
                    found = false;
                    return source.ToAlbumDTO();
                },
                (id, existing) => {
                    found = true;
                    existing.Update(source);
                    return existing;
                }
            );
            return found;
        }

        public static void Update(this AlbumDTO album, SimpleAlbum source) {
            album.Name = source.Name;
            album.ReleaseDate = source.ReleaseDate;
            album.TotalTracks = source.TotalTracks;
            album.Images = source.Images.ToImageDTOs();
        }

        public static bool UpdateOrAdd(this ConcurrentDictionary<string, TrackDTO> dict, FullTrack source, out TrackDTO dto) {
            bool found = false;
            dto = dict.AddOrUpdate(
                source.Id,
                id => {
                    found = false;
                    return source.ToTrackDTO();
                },
                (id, existing) => {
                    found = true;
                    existing.Update(source);
                    return existing;
                }
            );
            return found;
        }

        public static void Update(this TrackDTO track, FullTrack source) {
            track.Name = source.Name;
            track.DurationMs = source.DurationMs;
            track.Popularity = source.Popularity;
        }
    }
}
