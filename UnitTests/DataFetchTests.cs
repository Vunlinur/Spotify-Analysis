using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SpotifyAnalysis.Data.DataAccessLayer;
using SpotifyAnalysis.Data.DTO;
using SpotifyAnalysis.Data.SpotifyAPI;
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
            var testImages = new List<Image> { new Image { Url = "http://example.com/image.jpg", Height = 100, Width = 100 } };
            var testUser = new PublicUser { Id = "test_user", DisplayName = "Test User", Images = testImages };

            var testArtist = new FullArtist {
                Id = "artist1",
                Name = "Test Artist",
                Genres = ["Rock", "Pop"],
                Popularity = 80,
                Images = testImages
            };

            var simpleTestArtist = new SimpleArtist {
                Id = testArtist.Id,
                Name = testArtist.Name,
            };

            var testAlbum = new SimpleAlbum {
                Id = "album1",
                Name = "Test Album",
                ReleaseDate = "2022-01-01",
                TotalTracks = 10,
                Artists = [simpleTestArtist],
                Images = testImages
            };

            var testTrack = new FullTrack {
                Id = "track1",
                Name = "Test Track",
                DurationMs = 180000,
                Popularity = 50,
                Album = testAlbum,
                Artists = [simpleTestArtist],
            };

            var testPlaylist = new FullPlaylist {
                Id = "playlist1",
                Name = "Test Playlist",
                Owner = new PublicUser { Id = "test_owner", DisplayName = "Owner Name" },
                SnapshotId = "snapshot1",
                Tracks = new Paging<PlaylistTrack<IPlayableItem>> {
                    Items = [new PlaylistTrack<IPlayableItem> { Track = testTrack }],
                    Total = 1
                },
                Followers = new Followers { Total = 1 },
                Images = testImages
            };

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
            var track = await dbContext.Tracks.Include(t => t.Album).Include(t => t.Artists).FirstOrDefaultAsync(t => t.ID == "track1");
            Assert.IsNotNull(track);
            Assert.AreEqual("Test Track", track.Name);
            Assert.AreEqual("Test Album", track.Album.Name);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual("Test Artist", track.Artists[0].Name);
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
