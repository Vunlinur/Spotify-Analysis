using Moq;
using SpotifyAPI.Web;
using NUnit.Framework.Legacy;
using UnitTests;

namespace Tests.GetDataTests {
    public class TestRemoveTracks : GetDataBase {
        [Test]
        public async Task Remove_1Track0Remains() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testSimpleArtist2 = Stubs.SimpleArtist(2);
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);

            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);
            var testPlaylist2 = Stubs.FullPlaylist(testUser, []);
            testPlaylist2.SnapshotId = "new snapshot";

            mockPublicPlaylists.SetupSequence(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist2]));
            mockPlaylist.SetupSequence(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist))
                .Returns(Task.FromResult(testPlaylist2));
            mockTracks.SetupSequence(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack }))
                .Returns(Task.FromResult(new List<FullTrack> { }));
            mockArtists.SetupSequence(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }))
                .Returns(Task.FromResult(new List<FullArtist> { }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);
            await dataFetch.GetData(testUser.Id);

            // Assert
            //AssertDbSetCounts(1, 0, 2, 2);
            AssertPlaylistData(testPlaylist, 0);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id], 0);
        }


        [Test]
        public async Task Remove_1Track1Remains() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testArtist2 = Stubs.FullArtist(2);
            var testSimpleArtist2 = Stubs.SimpleArtist(2);
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testAlbum2 = Stubs.SimpleAlbum([testSimpleArtist2], 2);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testTrack2 = Stubs.FullTrack([testSimpleArtist2], testAlbum2, 2);

            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack, testTrack2]);
            var testPlaylist2 = Stubs.FullPlaylist(testUser, [testTrack]);
            testPlaylist2.SnapshotId = "new snapshot";

            mockPublicPlaylists.SetupSequence(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist2]));
            mockPlaylist.SetupSequence(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist))
                .Returns(Task.FromResult(testPlaylist2));
            mockTracks.SetupSequence(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack, testTrack2 }))
                .Returns(Task.FromResult(new List<FullTrack> { testTrack }));
            mockArtists.SetupSequence(m => m(It.IsAny<IList<string>>()))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist, testArtist2 }))
                .Returns(Task.FromResult(new List<FullArtist> { testArtist }));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);
            await dataFetch.GetData(testUser.Id);

            // Assert
            //AssertDbSetCounts(1, 1, 2, 2);
            AssertPlaylistData(testPlaylist, 1);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum2.Id, [testArtist2.Id], 0);
        }
    }
}
