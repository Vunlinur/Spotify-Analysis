using Microsoft.EntityFrameworkCore;
using Moq;
using SpotifyAnalysis.Data.DataAccessLayer;
using SpotifyAPI.Web;

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
            SpotifyContext.Configurator = Stubs.ConfigureInMemory;
            dbContext = new SpotifyContext();

            // Initialize mock delegates
            mockUserProfile = new Mock<GetUserProfileDelegate>();
            mockPublicPlaylists = new Mock<GetUsersPublicPlaylistsDelegate>();
            mockPlaylist = new Mock<GetPlaylistAsyncDelegate>();
            mockTracks = new Mock<GetTracksAsyncDelegate>();
            mockArtists = new Mock<GetArtistsAsyncDelegate>();
            mockProgressBar = new Mock<UpdateProgressBarDelegate>();
        }

        [Test]
        public async Task GetData_GetsOneTrack() {
            // Arrange
            var testUser = Stubs.PublicUser();
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);

            mockUserProfile.Setup(m => m(It.IsAny<string>()))
                .Returns(Task.FromResult(testUser));

            mockPublicPlaylists.Setup(m => m(It.IsAny<string>()))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]));

            mockPlaylist.Setup(m => m(It.IsAny<string>()))
                .Returns(Task.FromResult(testPlaylist));

            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack }));

            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }));

            mockProgressBar.Setup(m => m(It.IsAny<float>(), It.IsAny<string>()));

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
            var playlists = await dbContext.Playlists
                .Include(p => p.Tracks).ThenInclude(t => t.Album)
                .Include(p => p.Tracks).ThenInclude(t => t.Artists)
                .ToArrayAsync();
            Assert.AreEqual(1, playlists.Length);
            var playlist = playlists.FirstOrDefault();
            Assert.AreEqual(testPlaylist.Name, playlist.Name);
            Assert.AreEqual(1, playlist.Tracks.Count);
            var track = playlist.Tracks.FirstOrDefault();
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack.Name, track.Name);
            Assert.AreEqual(testAlbum.Name, track.Album.Name);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(testArtist.Name, track.Artists[0].Name);
        }

        [TearDown]
        public void TearDown() {
            dbContext.Database.EnsureDeleted();
            dbContext?.Dispose();
        }
    }
}
