using Microsoft.EntityFrameworkCore;
using Moq;
using SpotifyAnalysis.Data.Database;
using SpotifyAPI.Web;
using NUnit.Framework.Legacy;

namespace UnitTests {
    public class GetDataTests : GetDataBase {
        [Test]
        public async Task Add_Playlist() {
            // Arrange
            var testPlaylist = Stubs.FullPlaylist(testUser, []);

            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]));
            mockPlaylist.Setup(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist));
            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { }));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 0, 0, 0);
            AssertPlaylistData(testPlaylist, 0);
        }

        [Test]
        public async Task Remove_Playlist() {
            // Arrange
            var testPlaylist = Stubs.FullPlaylist(testUser, []);

            mockPublicPlaylists.SetupSequence(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]))
                .Returns(Task.FromResult<IList<FullPlaylist>>([]));
            mockPlaylist.Setup(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist));
            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { }));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(0, 0, 0, 0);
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

        [Test]
        public async Task Rename_Playlist() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();

            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);

            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);
            var testPlaylistRenamed = Stubs.FullPlaylist(testUser, [testTrack]);
            testPlaylistRenamed.SnapshotId = "new snapshot";
            testPlaylistRenamed.Name = "new name";

            mockPublicPlaylists.SetupSequence(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylistRenamed]));
            mockPlaylist.SetupSequence(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist))
                .Returns(Task.FromResult(testPlaylistRenamed));
            mockTracks.SetupSequence(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack }))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack }));
            mockArtists.SetupSequence(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 1, 1, 1);
            AssertPlaylistData(testPlaylistRenamed, 1);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
        }
    }
}
