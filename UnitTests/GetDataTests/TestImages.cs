using ApexCharts;
using Moq;
using NUnit.Framework.Legacy;
using SpotifyAPI.Web;
using UnitTests;

namespace Tests.GetDataTests {
    public class TestImages : GetDataBase {

        [Test]
        public async Task Add_2TracksDiffAlbumsSameImage() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();

            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            testAlbum.Images = Stubs.Images("Album", 0);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);

            var testAlbum2 = Stubs.SimpleAlbum([testSimpleArtist], 2);
            testAlbum2.Images = Stubs.Images("Album", 0);
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
            AssertDbSetCounts(1, 2, 1, 2, 4);
            AssertPlaylistData(testPlaylist, 2);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
            AssertTrackData(testTrack2, testAlbum2.Id, [testArtist.Id]);
        }

        [Test]
        public async Task Add_1TrackPlaylistSameImage() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);
            testPlaylist.Images = Stubs.Images("shared", 0);
            testAlbum.Images = Stubs.Images("shared", 0);

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
            AssertDbSetCounts(1, 1, 1, 1, 3);
            AssertPlaylistData(testPlaylist, 1);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
        }

        [Test]
        public async Task Add_1TrackArtistSameImage() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);
            testPlaylist.Images = Stubs.Images("shared", 0);
            testArtist.Images = Stubs.Images("shared", 0);

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
            AssertDbSetCounts(1, 1, 1, 1, 3);
            AssertPlaylistData(testPlaylist, 1);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
        }

        [Test]
        public async Task Add_1TrackArtistAlbumSameImage() {
            // Arrange
            var testArtist = Stubs.FullArtist();
            var testSimpleArtist = Stubs.SimpleArtist();
            var testAlbum = Stubs.SimpleAlbum([testSimpleArtist]);
            var testTrack = Stubs.FullTrack([testSimpleArtist], testAlbum);
            var testPlaylist = Stubs.FullPlaylist(testUser, [testTrack]);
            testPlaylist.Images = Stubs.Images("shared", 0);
            testAlbum.Images = Stubs.Images("shared", 0);
            testArtist.Images = Stubs.Images("shared", 0);

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
            AssertDbSetCounts(1, 1, 1, 1, 2);
            AssertPlaylistData(testPlaylist, 1);
            AssertTrackData(testTrack, testAlbum.Id, [testArtist.Id]);
        }
    }
}
