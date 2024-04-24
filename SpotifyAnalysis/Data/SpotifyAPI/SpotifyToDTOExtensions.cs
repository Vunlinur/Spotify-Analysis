﻿using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;

namespace SpotifyAnalysis.Data.SpotifyAPI {
    public static class SpotifyToDTOExtensions {
		public static UserDTO ToUserDTO(this PublicUser pu) {
            return new UserDTO() {
                ID = pu.Id,
                Name = pu.DisplayName,
				Playlists = []
			};
		}

		public static PlaylistDTO ToPlaylistDTO(this FullPlaylist fp) {
            return new PlaylistDTO() {
                ID = fp.Id,
                Name = fp.Name,
                Owner = fp.Owner.Id,
                SnapshotID = fp.SnapshotId,
                Tracks = [],
                Images = []
			};
		}

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
				Artists = [], // SimpleArtist instead of FullArtist
				Tracks = [],
				Images = []
			};
		}

		public static ArtistDTO ToArtistDTO(this FullArtist fa) {
			return new ArtistDTO() {
				ID = fa.Id,
				Name = fa.Name,
				Genres = fa.Genres,
				Popularity = fa.Popularity,
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
	}
}
