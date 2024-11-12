using Microsoft.EntityFrameworkCore;
using Moq;
using SpotifyAnalysis.Data.DataAccessLayer;
using SpotifyAPI.Web;

namespace UnitTests {
    public class GetDataTests {
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
        public async Task Add_1Track() {
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
            var albums = await dbContext.Albums.ToListAsync();
            var artists = await dbContext.Artists.ToListAsync();
            var tracks = await dbContext.Tracks.ToListAsync();
            var playlists = await dbContext.Playlists
                .Include(p => p.Tracks).ThenInclude(t => t.Album)
                .Include(p => p.Tracks).ThenInclude(t => t.Artists)
                .ToListAsync();
            Assert.AreEqual(1, albums.Count);
            Assert.AreEqual(1, artists.Count);
            Assert.AreEqual(1, tracks.Count);
            Assert.AreEqual(1, playlists.Count);
            var playlist = playlists[0];
            Assert.AreEqual(testPlaylist.Id, playlist.ID);
            Assert.AreEqual(testPlaylist.Owner.Id, playlist.OwnerID);
            Assert.AreEqual(1, playlist.Tracks.Count);
            var track = playlist.Tracks[0];
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack.Id, track.ID);
            Assert.AreEqual(testAlbum.Id, track.Album.ID);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(testArtist.Id, track.Artists[0].ID);
            Assert.AreEqual(1, track.Playlists.Count);
        }

        [Test]
        public async Task Add_2DifferentTracks() {
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
            var albums = await dbContext.Albums.ToListAsync();
            var artists = await dbContext.Artists.ToListAsync();
            var tracks = await dbContext.Tracks.ToListAsync();
            var playlists = await dbContext.Playlists
                .Include(p => p.Tracks).ThenInclude(t => t.Album)
                .Include(p => p.Tracks).ThenInclude(t => t.Artists)
                .ToListAsync();
            Assert.AreEqual(2, albums.Count);
            Assert.AreEqual(2, artists.Count);
            Assert.AreEqual(2, tracks.Count);
            Assert.AreEqual(1, playlists.Count);
            var playlist = playlists[0];
            Assert.AreEqual(testPlaylist.Id, playlist.ID);
            Assert.AreEqual(testPlaylist.Owner.Id, playlist.OwnerID);
            Assert.AreEqual(2, playlist.Tracks.Count);
            var track = playlist.Tracks.First(t => t.ID == testTrack.Id);
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack.Id, track.ID);
            Assert.AreEqual(testAlbum.Id, track.Album.ID);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(testArtist.Id, track.Artists[0].ID);
            Assert.AreEqual(1, track.Playlists.Count);
            track = playlist.Tracks.First(t => t.ID == testTrack2.Id);
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack2.Id, track.ID);
            Assert.AreEqual(testAlbum2.Id, track.Album.ID);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(testArtist2.Id, track.Artists[0].ID);
            Assert.AreEqual(1, track.Playlists.Count);
        }

        [Test]
        public async Task Add_2TracksSameArtistDiffAlbums() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);

            var testAlbum2 = Stubs.SimpleAlbum([testSimpleArtist], 2);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist], testAlbum2, 2);

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
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }));

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
            var albums = await dbContext.Albums.ToListAsync();
            var artists = await dbContext.Artists.ToListAsync();
            var tracks = await dbContext.Tracks.ToListAsync();
            var playlists = await dbContext.Playlists
                .Include(p => p.Tracks).ThenInclude(t => t.Album)
                .Include(p => p.Tracks).ThenInclude(t => t.Artists)
                .ToListAsync();
            Assert.AreEqual(2, albums.Count);
            Assert.AreEqual(1, artists.Count);
            Assert.AreEqual(2, tracks.Count);
            Assert.AreEqual(1, playlists.Count);
            var playlist = playlists.FirstOrDefault();
            Assert.AreEqual(testPlaylist.Id, playlist.ID);
            Assert.AreEqual(testPlaylist.Owner.Id, playlist.OwnerID);
            Assert.AreEqual(2, playlist.Tracks.Count);
            var track = playlist.Tracks.First(t => t.ID == testTrack.Id);
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack.Id, track.ID);
            Assert.AreEqual(testAlbum.Id, track.Album.ID);
            Assert.AreEqual(testArtist.Id, track.Artists[0].ID);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(1, track.Playlists.Count);
            track = playlist.Tracks.First(t => t.ID == testTrack2.Id);
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack2.Id, track.ID);
            Assert.AreEqual(testAlbum2.Id, track.Album.ID);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(1, track.Playlists.Count);
            Assert.AreEqual(testArtist.Id, track.Artists[0].ID);
        }

        [Test]
        public async Task Add_2TracksSameArtistSameAlbums() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist], testAlbum, 2);

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
                .Returns(Task.FromResult(new List<FullArtist> { testArtist, testArtist }));

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
            var albums = await dbContext.Albums.ToListAsync();
            var artists = await dbContext.Artists.ToListAsync();
            var tracks = await dbContext.Tracks.ToListAsync();
            var playlists = await dbContext.Playlists
                .Include(p => p.Tracks).ThenInclude(t => t.Album)
                .Include(p => p.Tracks).ThenInclude(t => t.Artists)
                .ToListAsync();
            Assert.AreEqual(1, albums.Count);
            Assert.AreEqual(1, artists.Count);
            Assert.AreEqual(2, tracks.Count);
            Assert.AreEqual(1, playlists.Count);
            var playlist = playlists.FirstOrDefault();
            Assert.AreEqual(testPlaylist.Id, playlist.ID);
            Assert.AreEqual(testPlaylist.Owner.Id, playlist.OwnerID);
            Assert.AreEqual(2, playlist.Tracks.Count);
            var track = playlist.Tracks.First(t => t.ID == testTrack.Id);
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack.Id, track.ID);
            Assert.AreEqual(testAlbum.Id, track.Album.ID);
            Assert.AreEqual(testArtist.Id, track.Artists[0].ID);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(1, track.Playlists.Count);
            track = playlist.Tracks.First(t => t.ID == testTrack2.Id);
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack2.Id, track.ID);
            Assert.AreEqual(testAlbum.Id, track.Album.ID);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(1, track.Playlists.Count);
            Assert.AreEqual(testArtist.Id, track.Artists[0].ID);
        }

        [Test]
        public async Task Update_2TracksSameArtistSameAlbums() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist], testAlbum, 2);

            var testUser = Stubs.PublicUser();
            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);
            var testPlaylist2 = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);
            testPlaylist2.SnapshotId = "new snapshot";

            mockUserProfile.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult(testUser));
            mockPublicPlaylists.SetupSequence(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist2]));
            mockPlaylist.SetupSequence(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist))
                .Returns(Task.FromResult(testPlaylist2));
            mockTracks.SetupSequence(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack }))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }));

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
            await dataFetch.GetData(testUser.Id);

            // Assert
            var albums = await dbContext.Albums.ToListAsync();
            var artists = await dbContext.Artists.ToListAsync();
            var tracks = await dbContext.Tracks.ToListAsync();
            var playlists = await dbContext.Playlists
                .Include(p => p.Tracks).ThenInclude(t => t.Album)
                .Include(p => p.Tracks).ThenInclude(t => t.Artists)
                .ToListAsync();
            Assert.AreEqual(1, albums.Count);
            Assert.AreEqual(1, artists.Count);
            Assert.AreEqual(2, tracks.Count);
            Assert.AreEqual(1, playlists.Count);
            var playlist = playlists.FirstOrDefault();
            Assert.AreEqual(testPlaylist.Id, playlist.ID);
            Assert.AreEqual(testPlaylist.Owner.Id, playlist.OwnerID);
            Assert.AreEqual(2, playlist.Tracks.Count);
            var track = playlist.Tracks.First(t => t.ID == testTrack.Id);
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack.Id, track.ID);
            Assert.AreEqual(testAlbum.Id, track.Album.ID);
            Assert.AreEqual(testArtist.Id, track.Artists[0].ID);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(1, track.Playlists.Count);
            track = playlist.Tracks.First(t => t.ID == testTrack2.Id);
            Assert.IsNotNull(track);
            Assert.AreEqual(testTrack2.Id, track.ID);
            Assert.AreEqual(testAlbum.Id, track.Album.ID);
            Assert.AreEqual(1, track.Artists.Count);
            Assert.AreEqual(1, track.Playlists.Count);
            Assert.AreEqual(testArtist.Id, track.Artists[0].ID);
        }

        [TearDown]
        public void TearDown() {
            dbContext.Database.EnsureDeleted();
            dbContext?.Dispose();
        }
    }
}
