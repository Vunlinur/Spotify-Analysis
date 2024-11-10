using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SpotifyAnalysis.Data.DataAccessLayer;
using SpotifyAnalysis.Data.DTO;
using SpotifyAnalysis.Data.SpotifyAPI;
using SpotifyAnalysis.Migrations;
using SpotifyAPI.Web; // Assuming SpotifyAPI.Web is used for models like PublicUser, FullPlaylist, FullTrack, etc.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTests {
    public class Tests {
        private Mock<GetUserProfileDelegate> mockUserProfile;
        private Mock<GetUsersPublicPlaylistsDelegate> mockPublicPlaylists;
        private Mock<GetPlaylistAsyncDelegate> mockPlaylist;
        private Mock<GetTracksAsyncDelegate> mockTracks;
        private Mock<GetArtistsAsyncDelegate> mockArtists;
        private Mock<UpdateProgressBarDelegate> mockProgressBar;

        private SpotifyContext dbContext;

        public static List<Image> Images(int id = 0) => [
                new Image { Url = $"http://example.com/image{id}.jpg", Height = 100, Width = 100 }
            ];

        public static PublicUser PublicUser(int id = 0) =>
            new() {
                Id = $"public_user{id}",
                DisplayName = $"Test User {id}",
                Images = Images()
            };

        public static SimpleArtist SimpleArtist(int id = 0) =>
            new() {
                Id = $"artist{id}",
                Name = $"Test Artist {id}",
            };

        public static FullArtist FullArtist(int id = 0) =>
            new() {
                Id = $"artist{id}",
                Name = $"Test Artist {id}",
                Genres = ["Rock", "Pop"],
                Popularity = 80,
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

        public static FullTrack FullTrack(List<SimpleArtist> artists, SimpleAlbum album, int id = 0) =>
            new() {
                Id = $"track{id}",
                Name = $"Test Track {id}",
                DurationMs = 180000,
                Popularity = 50,
                Artists = artists,
                Album = album,
            };

        public static FullPlaylist FullPlaylist(PublicUser owner, IEnumerable<FullTrack> tracks, int id = 0) =>
            new() {
                Id = $"playlist{id}",
                Name = $"Test Playlist{id}",
                Owner = owner,
                SnapshotId = $"snapshot{id}",
                Tracks = new Paging<PlaylistTrack<IPlayableItem>> {
                    Items = tracks.Select(t => new PlaylistTrack<IPlayableItem> { Track = t }).ToList(),
                    Total = tracks.Count(),
                },
                Followers = new Followers { Total = id },
                Images = Images()
            };

        [SetUp]
        public void Setup() {
            // Configure in-memory database for testing
            SpotifyContext.Configurator = ConfigureInMemory;
            dbContext = new SpotifyContext();

            // Initialize mock delegates
            mockUserProfile = new Mock<GetUserProfileDelegate>();
            mockPublicPlaylists = new Mock<GetUsersPublicPlaylistsDelegate>();
            mockPlaylist = new Mock<GetPlaylistAsyncDelegate>();
            mockTracks = new Mock<GetTracksAsyncDelegate>();
            mockArtists = new Mock<GetArtistsAsyncDelegate>();
            mockProgressBar = new Mock<UpdateProgressBarDelegate>();

            // Initialize mock data to be used by multiple mocks
            var testUser = PublicUser();
            var testArtist = FullArtist();
            var testSimpleArtist = SimpleArtist();
            var testAlbum = SimpleAlbum([testSimpleArtist]);
            var testTrack = FullTrack([testSimpleArtist], testAlbum);
            var testPlaylist = FullPlaylist(testUser, [testTrack]);

            // Set up mocks using the shared test data
            mockUserProfile.Setup(m => m(It.IsAny<string>()))
                .Returns(Task.FromResult(testUser));

            mockPublicPlaylists.Setup(m => m(It.IsAny<string>()))
                .Returns(Task.FromResult<IList<FullPlaylist>>(new List<FullPlaylist> { testPlaylist }));

            mockPlaylist.Setup(m => m(It.IsAny<string>()))
                .Returns(Task.FromResult(testPlaylist));

            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack }));

            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }));

            mockProgressBar.Setup(m => m(It.IsAny<float>(), It.IsAny<string>()));
        }

        [Test]
        public async Task GetData_GetsOneTrack() {
            // Act
            var dataFetch = new DataFetch(
                mockUserProfile.Object,
                mockPublicPlaylists.Object,
                mockPlaylist.Object,
                mockTracks.Object,
                mockArtists.Object,
                mockProgressBar.Object
            );
            await dataFetch.GetData("test_user");

            // Assert - verify the track, album, and artist were loaded into the database
            var track = await dbContext.Tracks.Include(t => t.Album).Include(t => t.Artists).FirstOrDefaultAsync(t => t.ID == "track0");
            Assert.IsNotNull(track);
            Assert.AreEqual("Test Track 0", track.Name);
            Assert.AreEqual("Test Album 0", track.Album.Name);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual("Test Artist 0", track.Artists[0].Name);
        }

        [TearDown]
        public void TearDown() {
            dbContext.Database.EnsureDeleted();
            dbContext?.Dispose();
        }

        protected static void ConfigureInMemory(DbContextOptionsBuilder options) {
            options.UseInMemoryDatabase(databaseName: "SpotifyDB-test");
        }
    }
}
