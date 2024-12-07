using Microsoft.EntityFrameworkCore;
using Moq;
using SpotifyAnalysis.Data.Database;
using SpotifyAPI.Web;
using NUnit.Framework.Legacy;

namespace UnitTests {
    public class GetDataTests {
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

        private void AssertPlaylistData(FullPlaylist testPlaylist, int expectedTrackCount) {
            var playlist = dbContext.Playlists
                .Include(t => t.Tracks)
                .FirstOrDefault(p => p.ID == testPlaylist.Id);
            ClassicAssert.IsNotNull(playlist);
            ClassicAssert.AreEqual(testPlaylist.Owner.Id, playlist.OwnerID);
            ClassicAssert.AreEqual(expectedTrackCount, playlist.Tracks.Count);
        }

        private void AssertTrackData(FullTrack testTrack, string albumID, string[] artistIDs) {
            var track = dbContext.Tracks
                .Include(t => t.Album)
                .Include(t => t.Artists)
                .FirstOrDefault(p => p.ID == testTrack.Id);
            ClassicAssert.IsNotNull(track);
            ClassicAssert.AreEqual(albumID, track.Album.ID);
            ClassicAssert.AreEqual(1, track.Playlists.Count);
            CollectionAssert.AreEquivalent(artistIDs, track.Artists.Select(a => a.ID));
        }

        [Test]
        public async Task Add_1Track() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);

            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]));
            mockPlaylist.Setup(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist));
            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack }));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 1, 1, 1);
            AssertPlaylistData(testPlaylist, 1);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);

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

            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);

            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]));
            mockPlaylist.Setup(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist));
            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist, testArtist2 }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 2, 2, 2);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum2.Id, [testArtist2.Id]);
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

            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);

            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]));
            mockPlaylist.Setup(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist));
            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 2, 1, 2);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum2.Id, [testArtist.Id]);
        }

        [Test]
        public async Task Add_2TracksSameArtistSameAlbum() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist], testAlbum, 2);
            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);

            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]));
            mockPlaylist.Setup(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist));
            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist, testArtist }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 2, 1, 1);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum.Id, [testArtist.Id]);
        }

        [Test]
        public async Task Add_2TracksDiffArtistsSameAlbumByOneArtist() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testArtist2 = Stubs.FullArtist(2);
            var testSimpleArtist2 = Stubs.SimpleArtist(2);

            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist2], testAlbum, 2);
            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);

            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]));
            mockPlaylist.Setup(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist));
            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist, testArtist2 }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 2, 2, 1);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum.Id, [testArtist2.Id]);
        }

        [Test]
        public async Task Add_2TracksDiffArtistsSameAlbumByManyArtist() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testArtist2 = Stubs.FullArtist(2);
            var testSimpleArtist2 = Stubs.SimpleArtist(2);

            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist, testSimpleArtist2]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist2], testAlbum, 2);
            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);

            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]));
            mockPlaylist.Setup(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist));
            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist, testArtist2 }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 2, 2, 1);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum.Id, [testArtist2.Id]);
        }

        [Test]
        public async Task Add_2TracksVariousArtistsSameAlbum() { // e.g. Spelljams
            // Arrange
            var testVariousArtist = Stubs.SimpleArtist(3);
            var testAlbum = Stubs.SimpleAlbum([testVariousArtist]);

            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);

            var testArtist2 = Stubs.FullArtist(2);
            var testSimpleArtist2 = Stubs.SimpleArtist(2);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist2], testAlbum, 2);

            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);

            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]));
            mockPlaylist.Setup(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist));
            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist, testArtist2 }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 2, 3, 1);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum.Id, [testArtist2.Id]);
            ClassicAssert.NotNull(dbContext.Artists.FirstOrDefault(a => a.ID == testVariousArtist.Id));
        }

        [Test]
        public async Task Update_2DifferentTracks() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testArtist2 = Stubs.FullArtist(2);
            var testSimpleArtist2 = Stubs.SimpleArtist(2);
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testAlbum2 = Stubs.SimpleAlbum([testSimpleArtist2], 2);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist2], testAlbum2, 2);

            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);
            var testPlaylist2 = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);
            testPlaylist2.SnapshotId = "new snapshot";

            mockPublicPlaylists.SetupSequence(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist2]));
            mockPlaylist.SetupSequence(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist))
                .Returns(Task.FromResult(testPlaylist2));
            mockTracks.SetupSequence(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack }))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }));
            mockArtists.SetupSequence(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist, testArtist2 }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 2, 2, 2);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum2.Id, [testArtist2.Id]);
        }

        [Test]
        public async Task Update_2TracksDiffArtistsSameAlbumByOneArtist() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testArtist2 = Stubs.FullArtist(2);
            var testSimpleArtist2 = Stubs.SimpleArtist(2);
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist2], testAlbum, 2);

            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);
            var testPlaylist2 = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);
            testPlaylist2.SnapshotId = "new snapshot";

            mockPublicPlaylists.SetupSequence(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist2]));
            mockPlaylist.SetupSequence(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist))
                .Returns(Task.FromResult(testPlaylist2));
            mockTracks.SetupSequence(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack }))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }));
            mockArtists.SetupSequence(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist, testArtist2 }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 2, 2, 1);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum.Id, [testArtist2.Id]);
        }

        [Test]
        public async Task Update_2TracksDiffArtistsSameAlbumByManyArtists() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testArtist2 = Stubs.FullArtist(2);
            var testSimpleArtist2 = Stubs.SimpleArtist(2);
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist, testSimpleArtist2]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist2], testAlbum, 2);

            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);
            var testPlaylist2 = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);
            testPlaylist2.SnapshotId = "new snapshot";

            mockPublicPlaylists.SetupSequence(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist2]));
            mockPlaylist.SetupSequence(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist))
                .Returns(Task.FromResult(testPlaylist2));
            mockTracks.SetupSequence(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack }))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }));
            mockArtists.SetupSequence(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist, testArtist2 }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 2, 2, 1);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum.Id, [testArtist2.Id]);
        }

        [Test]
        public async Task Update_2TracksSameArtistDiffAlbums() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testAlbum2 = Stubs.SimpleAlbum([testSimpleArtist], 2);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist], testAlbum2, 2);

            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);
            var testPlaylist2 = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);
            testPlaylist2.SnapshotId = "new snapshot";

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
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 2, 1, 2);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum2.Id, [testArtist.Id]);
        }

        [Test]
        public async Task Update_2TracksSameArtistSameAlbum() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist], testAlbum, 2);

            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);
            var testPlaylist2 = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);
            testPlaylist2.SnapshotId = "new snapshot";

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
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 2, 1, 1);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum.Id, [testArtist.Id]);
        }

        [Test]
        public async Task Update_2TracksVariousArtistsSameAlbum() {
            // Arrange
            var testVariousArtist = Stubs.SimpleArtist(3);
            var testAlbum = Stubs.SimpleAlbum([testVariousArtist]);

            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testArtist2 = Stubs.FullArtist(2);
            var testSimpleArtist2 = Stubs.SimpleArtist(2);

            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist2], testAlbum, 2);

            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);
            var testPlaylist2 = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);
            testPlaylist2.SnapshotId = "new snapshot";

            mockPublicPlaylists.SetupSequence(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist2]));
            mockPlaylist.SetupSequence(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist))
                .Returns(Task.FromResult(testPlaylist2));
            mockTracks.SetupSequence(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack }))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }));
            mockArtists.SetupSequence(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist, testArtist2 }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 2, 3, 1);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum.Id, [testArtist2.Id]);
            ClassicAssert.NotNull(dbContext.Artists.FirstOrDefault(a => a.ID == testVariousArtist.Id));
        }

        [TearDown]
        public void TearDown() {
            dbContext.Database.EnsureDeleted();
            dbContext?.Dispose();
        }
    }
}
