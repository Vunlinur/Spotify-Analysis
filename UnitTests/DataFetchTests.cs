using Microsoft.EntityFrameworkCore;
using Moq;
using SpotifyAnalysis.Data.DataAccessLayer;
using SpotifyAPI.Web;

namespace UnitTests {
    public class DataFetchTests {
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
        public async Task GetData_GetOneTrack() {
            // Arrange
            var testUser = Stubs.PublicUser();
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);

            mockUserProfile.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult(testUser));
            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]));
            mockPlaylist.Setup(m => m(It.Is<string>(s => s == testPlaylist.Id)))
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
            await dataFetch.GetData(testUser.Id);

            // Assert
            var playlists = await dbContext.Playlists
                .Include(p => p.Tracks).ThenInclude(t => t.Album)
                .Include(p => p.Tracks).ThenInclude(t => t.Artists)
                .ToArrayAsync();
            Assert.AreEqual(1, playlists.Count());
            var playlist = playlists.FirstOrDefault();
            Assert.AreEqual(testPlaylist.Name, playlist.Name);
            Assert.AreEqual(testPlaylist.Owner.Id, playlist.OwnerID);
            Assert.AreEqual(1, playlist.Tracks.Count);
            var track = playlist.Tracks[0];
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack.Name, track.Name);
            Assert.AreEqual(testAlbum.Name, track.Album.Name);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(1, track.Playlists.Count);
            Assert.AreEqual(testArtist.Name, track.Artists[0].Name);
        }

        [Test]
        public async Task GetData_Get2DifferentTracks() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);

            var testArtist2 = Stubs.FullArtist(2);
            var testSimpleArtist2 = Stubs.SimpleArtist(2);
            var testAlbum2 = Stubs.SimpleAlbum([testSimpleArtist2], 2);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist2], testAlbum2, 2);

            var testUser = Stubs.PublicUser();
            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);

            mockUserProfile.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult(testUser));
            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]));
            mockPlaylist.Setup(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist));
            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist, testArtist2 }));
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
            await dataFetch.GetData(testUser.Id);

            // Assert
            var playlists = await dbContext.Playlists
                .Include(p => p.Tracks).ThenInclude(t => t.Album)
                .Include(p => p.Tracks).ThenInclude(t => t.Artists)
                .ToArrayAsync();
            Assert.AreEqual(1, playlists.Count());
            var playlist = playlists.FirstOrDefault();
            Assert.AreEqual(testPlaylist.Name, playlist.Name);
            Assert.AreEqual(testPlaylist.Owner.Id, playlist.OwnerID);
            Assert.AreEqual(2, playlist.Tracks.Count);
            var track = playlist.Tracks.First(t => t.ID == testTrack.Id);
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack.Name, track.Name);
            Assert.AreEqual(testAlbum.Name, track.Album.Name);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(1, track.Playlists.Count);
            Assert.AreEqual(testArtist.Name, track.Artists[0].Name);
            track = playlist.Tracks.First(t => t.ID == testTrack2.Id);
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack2.Name, track.Name);
            Assert.AreEqual(testAlbum2.Name, track.Album.Name);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(1, track.Playlists.Count);
            Assert.AreEqual(testArtist2.Name, track.Artists[0].Name);
        }

        [TearDown]
        public void TearDown() {
            dbContext.Database.EnsureDeleted();
            dbContext?.Dispose();
        }
    }
}
