using Microsoft.EntityFrameworkCore;
using Moq;
using SpotifyAnalysis.Data.Database;
using SpotifyAPI.Web;
using NUnit.Framework.Legacy;
using System.Linq;

namespace UnitTests {
    public class GetDataPerfTests {
        private Mock<GetUserProfileDelegate> mockUserProfile;
        private Mock<GetUsersPublicPlaylistsDelegate> mockPublicPlaylists;
        private Mock<GetPlaylistAsyncDelegate> mockPlaylist;
        private Mock<GetTracksAsyncDelegate> mockTracks;
        private Mock<GetArtistsAsyncDelegate> mockArtists;
        private Mock<UpdateProgressBarDelegate> mockProgressBar;

        private SpotifyContext dbContext;

        private PublicUser testUser;

        [OneTimeSetUp]
        public void OneTimeSetup() {
            // Configure in-memory database for testing
            SpotifyContext.Configurator = Stubs.ConfigureInMemory;
        }

        [SetUp]
        public void Setup() {
            dbContext = new SpotifyContext();

            // Initialize mock delegates
            mockUserProfile = new Mock<GetUserProfileDelegate>();
            mockPublicPlaylists = new Mock<GetUsersPublicPlaylistsDelegate>();
            mockPlaylist = new Mock<GetPlaylistAsyncDelegate>();
            mockTracks = new Mock<GetTracksAsyncDelegate>();
            mockArtists = new Mock<GetArtistsAsyncDelegate>();
            mockProgressBar = new Mock<UpdateProgressBarDelegate>();

            testUser = Stubs.PublicUser();
            mockUserProfile.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult(testUser));
        }

        private DataFetch CreateDataFetch() =>
            new(
                mockUserProfile.Object,
                mockPublicPlaylists.Object,
                mockPlaylist.Object,
                mockTracks.Object,
                mockArtists.Object,
                mockProgressBar.Object
            );

        private void AssertDbSetCounts(int playlistCount, int trackCount, int artistCount, int albumCount) {
            ClassicAssert.AreEqual(playlistCount, dbContext.Playlists.Count());
            ClassicAssert.AreEqual(trackCount, dbContext.Tracks.Count());
            ClassicAssert.AreEqual(artistCount, dbContext.Artists.Count());
            ClassicAssert.AreEqual(albumCount, dbContext.Albums.Count());
        }

        class MockData(PublicUser user) {
            public PublicUser User = user;
            public List<SimpleArtist> SimpleArtists = [];
            public List<FullArtist> FullArtists = [];
            public List<SimpleAlbum> Albums = [];
            public List<FullPlaylist> Playlists = [];
            public List<FullTrack> Tracks = [];
        }

        private static MockData GenerateLargeMockData() {
            // Step 1: Create shared mock artists and albums
            var data = new MockData(Stubs.PublicUser()) {
                SimpleArtists = Enumerable.Range(1, 1000)
                .Select(Stubs.SimpleArtist)
                .ToList(),
                FullArtists = Enumerable.Range(1, 1000)
                .Select(Stubs.FullArtist)
                .ToList()
            };
            data.Albums = Enumerable.Range(1, 5000)
                .Select(i => Stubs.SimpleAlbum(data.SimpleArtists.Take(1 + i % 2).ToList(), i))
                .ToList();

            // Step 2: Generate 100 playlists, each with 1,000 tracks
            for (int p = 0; p < 100; p++) {
                var tracks = new List<FullTrack>();
                int trackOffset = 1000 * p - (p * 10);  // Add 1000 for each playlist, 10 tracks overlapping
                for (int t = trackOffset; t < trackOffset + 1000; t++) {
                    // Assign an album and a subset of artists
                    var album = data.Albums[t % data.Albums.Count];
                    var trackArtists = data.SimpleArtists.Skip(t % data.SimpleArtists.Count).Take(1 + t % 2).ToList();
                    var track = Stubs.FullTrack(trackArtists, album, t);

                    tracks.Add(track);
                }

                // Step 3: Assign the user to the playlists and collect them
                var playlist = Stubs.FullPlaylist(data.User, tracks, p);
                data.Tracks.AddRange(tracks);
                data.Playlists.Add(playlist);
            }
            return data;
        }

        [Test]
        public async Task Add_LargeMockData() {
            // Arrange
            var data = GenerateLargeMockData();
            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>(data.Playlists));
            mockPlaylist.Setup(m => m(It.IsAny<string>()))
                .Returns<string>(playlistId => Task.FromResult(data.Playlists.FirstOrDefault(p => p.Id == playlistId)));
            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns<Paging<PlaylistTrack<IPlayableItem>>>(paging => Task.FromResult(paging.Items.Select(i => (FullTrack)i.Track).ToList()));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns<IList<string>>(ids => Task.FromResult(data.FullArtists.Where(a => ids.Contains(a.Id)).ToList()));

            // Act
            var dataFetch = CreateDataFetch();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await dataFetch.GetData(testUser.Id);
            stopwatch.Stop();

            // Assert
            Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
            ClassicAssert.AreEqual(1, dbContext.Users.Count());
            AssertDbSetCounts(100, 99010, 1000, 5000);
            ClassicAssert.Less(stopwatch.ElapsedMilliseconds, 10000, "Data fetch should complete in under 10 seconds");
        }

        [TearDown]
        public void TearDown() {
            dbContext.Database.EnsureDeleted();
            dbContext?.Dispose();
        }
    }
}
