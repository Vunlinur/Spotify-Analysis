using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyAnalysis.Data.SpotifyAPI {
    public static class SpotifyToDTOExtensions {
		public static UserDTO ToUserDTO(this PublicUser pu) {
            return new UserDTO() {
                ID = pu.Id,
                Name = pu.DisplayName,
				Updated = DateTime.Now,
				Playlists = [],
				Images = pu.Images.ToImageDTOs()
			};
		}

		public static PlaylistDTO ToPlaylistDTO(this FullPlaylist fp) {
			return new PlaylistDTO() {
				ID = fp.Id,
				Name = fp.Name,
				OwnerID = fp.Owner.Id,
				OwnerName = fp.Owner.DisplayName,
				SnapshotID = fp.SnapshotId,
				TracksTotal = fp.Tracks.Total,
				Tracks = [],
				Images = fp.Images.ToImageDTOs()
            };
		}

		public static List<PlaylistDTO> ToPlaylistDTOs(this IEnumerable<FullPlaylist> fullPlaylists)
			=> fullPlaylists.Select(p => p.ToPlaylistDTO()).ToList();

		public static TrackDTO ToTrackDTO(this FullTrack ft) {
			return new TrackDTO() {
				ID = ft.Id,
				Name = ft.Name,
				DurationMs = ft.DurationMs,
				Popularity = ft.Popularity,
				Album = null,
				Artists = []
			};
		}

		public static AlbumDTO ToAlbumDTO(this FullAlbum a) {
			return new AlbumDTO() {
				ID = a.Id,
				Name = a.Name,
				ReleaseDate = a.ReleaseDate,
				TotalTracks = a.TotalTracks,
				Artists = [],
				Tracks = [],
				Images = a.Images.ToImageDTOs()
            };
		}

		public static AlbumDTO ToAlbumDTO(this SimpleAlbum a) {
			return new AlbumDTO() {
				ID = a.Id,
				Name = a.Name,
				ReleaseDate = a.ReleaseDate,
				TotalTracks = a.TotalTracks,
				Artists = [],
				Tracks = [],
				Images = a.Images.ToImageDTOs()
            };
		}

		public static ArtistDTO ToArtistDTO(this FullArtist fa) {
			return new ArtistDTO() {
				ID = fa.Id,
				Name = fa.Name,
				Genres = fa.Genres,
				Popularity = fa.Popularity,
				Albums = [],
				Images = fa.Images.ToImageDTOs()
			};
		}

		public static ArtistDTO ToArtistDTO(this SimpleArtist a) {
			return new ArtistDTO() {
				ID = a.Id,
				Name = a.Name,
				Genres = [],
				Albums = [],
				Images = []
			};
		}

		public static ImageDTO ToImageDTO(this Image i) {
			return new ImageDTO() {
                Url = i.Url,
                Resolution = Math.Min(i.Height, i.Width)
			};
		}

		public static List<ImageDTO> ToImageDTOs(this IEnumerable<Image> images)
			=> images?.Select(i => i.ToImageDTO()).ToList();

    }
}
