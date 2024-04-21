using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;

namespace SpotifyAnalysis.Data.SpotifyAPI {
    public static class SpotifyToDTOExtensions {
        public static PlaylistDTO ToPlaylistDTO(this FullPlaylist fp) {
            return new PlaylistDTO() {
                ID = fp.Id,
                Name = fp.Name,
                Followers = fp.Followers.Total,
                Owner = fp.Owner.Id,
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
                Artists = [],
                Album = null
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
    }
}
