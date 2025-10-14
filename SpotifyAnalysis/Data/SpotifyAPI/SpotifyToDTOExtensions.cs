using SpotifyAnalysis.Data.Database;
using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyAnalysis.Data.SpotifyAPI {
    public static class SpotifyToDTOExtensions {
        public static UserDTO ToUserDTO(this PublicUser pu) {
			pu.Images.SortImages();
            return new UserDTO() {
                ID = pu.Id,
                Name = pu.DisplayName,
                Updated = DateTime.Now,
                Playlists = [],
				ImageS = pu.Images.FirstOrDefault()?.Url,
				ImageL = pu.Images.LastOrDefault()?.Url,
			};
        }

        public static UserDTO ToUserDTO(this PrivateUser pu) {
			pu.Images.SortImages();
            return new UserDTO() {
                ID = pu.Id,
                Name = pu.DisplayName,
                Updated = DateTime.Now,
                Playlists = [],
				ImageS = pu.Images.FirstOrDefault()?.Url,
				ImageL = pu.Images.LastOrDefault()?.Url,
			};
        }

        public static PlaylistDTO ToPlaylistDTO(this FullPlaylist fp) {
			fp.Images.SortImages();
			return new PlaylistDTO() {
				ID = fp.Id,
				Name = fp.Name,
				OwnerID = fp.Owner.Id,
				OwnerName = fp.Owner.DisplayName,
				SnapshotID = fp.SnapshotId,
				TracksTotal = fp.Tracks.Total,
				Tracks = [],
				ImageS = fp.Images.FirstOrDefault()?.Url,
				ImageL = fp.Images.LastOrDefault()?.Url,
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
			a.Images.SortImages();
			return new AlbumDTO() {
				ID = a.Id,
				Name = a.Name,
				ReleaseDate = a.ReleaseDate,
				TotalTracks = a.TotalTracks,
				Artists = [],
				Tracks = [],
				ImageS = a.Images.FirstOrDefault()?.Url,
				ImageL = a.Images.LastOrDefault()?.Url,
			};
		}

		public static AlbumDTO ToAlbumDTO(this SimpleAlbum a) {
			a.Images.SortImages();
			return new AlbumDTO() {
				ID = a.Id,
				Name = a.Name,
				ReleaseDate = a.ReleaseDate,
				TotalTracks = a.TotalTracks,
				Artists = [],
				Tracks = [],
				ImageS = a.Images.FirstOrDefault()?.Url,
				ImageL = a.Images.LastOrDefault()?.Url,
			};
		}

		public static ArtistDTO ToArtistDTO(this FullArtist fa) {
			fa.Images.SortImages();
			return new ArtistDTO() {
				ID = fa.Id,
				Name = fa.Name,
				Genres = fa.Genres,
				Popularity = fa.Popularity,
				Albums = [],
				ImageS = fa.Images.FirstOrDefault()?.Url,
				ImageL = fa.Images.LastOrDefault()?.Url,
			};
		}

		public static ArtistDTO ToArtistDTO(this SimpleArtist a) {
			return new ArtistDTO() {
				ID = a.Id,
				Name = a.Name,
				Genres = [],
				Albums = []
			};
		}
    }
}
