using SpotifyAPI.Web;

namespace UnitTests {
    internal static class Stubs {

        public static FullArtist FullArtist(int id = 0) =>
            new() {
                Id = $"artist{id}",
                Name = $"Test Artist {id}",
                Genres = ["Rock", "Pop"],
                Popularity = 80,
                Images = Images()
            };

        public static FullPlaylist FullPlaylist(PublicUser owner, IEnumerable<FullTrack> tracks, int id = 0) =>
            new() {
                Id = $"playlist{id}",
                Name = $"Test Playlist {id}",
                Owner = owner,
                SnapshotId = $"snapshot{id}",
                Tracks = PagingFromTracks(tracks),
                Followers = new Followers { Total = id },
                Images = Images()
            };

        public static Paging<PlaylistTrack<IPlayableItem>> PagingFromTracks(IEnumerable<FullTrack> tracks) =>
            new() {
                Items = tracks.Select(t => new PlaylistTrack<IPlayableItem> { Track = t }).ToList(),
                Total = tracks.Count(),
            };

        public static FullTrack FullTrack(List<SimpleArtist> artists, SimpleAlbum album, int id = 0) =>
            new() {
                Id = $"track{id}",
                Name = $"Test Track {id}",
                DurationMs = 180000,
                Popularity = 50,
                Artists = artists,
                Album = album,
            };

        public static List<Image> Images(int id = 0) => [
                new Image { Url = $"http://example.com/image{id}.jpg", Height = 100, Width = 100 }
            ];

        public static PublicUser PublicUser(int id = 0) =>
            new() {
                Id = $"public_user{id}",
                DisplayName = $"Test User {id}",
                Images = Images()
            };

        public static SimpleAlbum SimpleAlbum(List<SimpleArtist> artists, int id = 0) =>
            new() {
                Id = $"album{id}",
                Name = $"Test Album {id}",
                ReleaseDate = new DateTime(2022, 1, 1).AddDays(id).ToString("yyyy-MM-dd"),
                TotalTracks = id,
                Artists = artists,
                Images = Images()
            };

        public static SimpleArtist SimpleArtist(int id = 0) =>
            new() {
                Id = $"artist{id}",
                Name = $"Test Artist {id}",
            };
    }
}