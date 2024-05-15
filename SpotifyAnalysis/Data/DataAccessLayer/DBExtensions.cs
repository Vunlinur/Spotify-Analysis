using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SpotifyAnalysis.Data.DTO;
using SpotifyAnalysis.Data.SpotifyAPI;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data.DataAccessLayer {
	public static class DBExtensions {
		public static EntityEntry<TEnt> AddIfNotExists<TEnt, TKey>(this DbSet<TEnt> dbSet, TEnt entity) where TEnt : class {
			var exists = dbSet.Any(c => c == entity);
			return exists ? null : dbSet.Add(entity);
		}

		public static IEnumerable<TEnt> FindNewEntities<TEnt, TKey>(this IEnumerable<TEnt> current, IEnumerable<TEnt> source, Func<TEnt, TKey> keySelector) where TEnt : class {
			var existingKeys = current.Select(keySelector).ToHashSet();
			return source.Where(e => !existingKeys.Contains(keySelector(e)));
		}

		public static async Task AddRangeIfNotExists<TEnt, TKey>(this DbSet<TEnt> dbSet, IEnumerable<TEnt> entities, Func<TEnt, TKey> keySelector) where TEnt : class {
			var newEntities = FindNewEntities(dbSet, entities, keySelector);
			await dbSet.AddRangeAsync(newEntities);
		}

		public static bool UpdateOrAdd(this Dictionary<string, PlaylistDTO> dict, FullPlaylist source, out PlaylistDTO outPlaylist) {
			bool found = dict.TryGetValue(source.Id, out PlaylistDTO playlist);
			if (!found) {
				// this shouldn't really happen as we should already have
				// all the playlistsin the db before adding their details
				playlist = source.ToPlaylistDTO();
				dict.Add(playlist.ID, playlist);
			}
			else {
				playlist.Name = source.Name;
				playlist.SnapshotID = source.SnapshotId;
				playlist.TracksTotal = source.Tracks.Total;
				// TODO update remaining properties
			}
			outPlaylist = playlist;
			return found;
		}

		public static bool UpdateOrAdd(this Dictionary<string, ArtistDTO> dict, SimpleArtist source, out ArtistDTO outArtist) {
			bool found = dict.TryGetValue(source.Id, out ArtistDTO artist);
			if (!found) {
				artist = source.ToArtistDTO();
				dict.Add(artist.ID, artist);
			}
			else {
				artist.Name = source.Name;
			}
			outArtist = artist;
			return found;
		}

		public static bool UpdateOrAdd(this Dictionary<string, AlbumDTO> dict, SimpleAlbum source, out AlbumDTO outAlbum) {
			bool found = dict.TryGetValue(source.Id, out AlbumDTO album);
			if (!found) {
				album = source.ToAlbumDTO();
				dict.Add(album.ID, album);
			}
			else {
				album.Name = source.Name;
				album.ReleaseDate = source.ReleaseDate;
				album.TotalTracks = source.TotalTracks;
			}
			outAlbum = album;
			return found;
		}

		public static bool UpdateOrAdd(this Dictionary<string, TrackDTO> dict, FullTrack source, out TrackDTO outTrack) {
			bool found = dict.TryGetValue(source.Id, out TrackDTO track);
			if (!found) {
				track = source.ToTrackDTO();
				dict.Add(track.ID, track);
			}
			else {
				track.Name = source.Name;
				track.DurationMs = source.DurationMs;
				track.Popularity = source.Popularity;
			}
			outTrack = track;
			return found;
		}
	}
}
