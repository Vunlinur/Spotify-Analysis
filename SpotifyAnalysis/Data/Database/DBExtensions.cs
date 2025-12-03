using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyAnalysis.Data.Database {
    public static class DBExtensions {
        /**
		 * Finds entities in the source collection that do not exist in the database set based on the specified key selector.
		 */
        public static IEnumerable<TEnt> FindNewEntities<TEnt, TKey>(this DbSet<TEnt> current, IEnumerable<TEnt> source, Func<TEnt, TKey> keySelector) where TEnt : class {
			var existingKeys = current.Select(keySelector).ToHashSet();
            return source.Where(s => !existingKeys.Contains(keySelector(s)));
		}

        public static void Update(this PlaylistDTO playlist, FullPlaylist source) {
            source.Images.SortImages();
            playlist.Name = source.Name;
            playlist.OwnerID = source.Owner.Id;
            playlist.OwnerName = source.Owner.DisplayName;
            playlist.Followers = source.Followers.Total;
            playlist.SnapshotID = source.SnapshotId;
            playlist.TracksTotal = source.Tracks.Total;
            playlist.ImageS = source.Images.FirstOrDefault()?.Url;
            playlist.ImageL = source.Images.LastOrDefault()?.Url;
		}

        public static void Update(this ArtistDTO artist, SimpleArtist source) {
            artist.Name = source.Name;
        }

        public static void Update(this ArtistDTO artist, FullArtist source) {
			source.Images.SortImages();
			artist.Name = source.Name;
			artist.Genres = source.Genres;
            artist.Popularity = source.Popularity;
			artist.ImageS = source.Images.FirstOrDefault()?.Url;
			artist.ImageL = source.Images.LastOrDefault()?.Url;
		}

        public static void Update(this AlbumDTO album, SimpleAlbum source) {
			album.Name = source.Name;
            album.ReleaseDate = source.ReleaseDate;
            album.TotalTracks = source.TotalTracks;
		}

        public static void Update(this TrackDTO track, FullTrack source) {
            track.Name = source.Name;
            track.DurationMs = source.DurationMs;
            track.Popularity = source.Popularity;
		}

		public static void SortImages(this List<Image> images) {
			images.Sort(delegate (Image a, Image b) { return a.Width - b.Width; });
		}
	}
}
