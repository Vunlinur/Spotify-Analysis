using Moq;
using SpotifyAPI.Web;
using UnitTests;

namespace Tests.GetDataTests {
    public class TestPlaylists : GetDataBase {
        [Test]
        public async Task Add_Playlist() {
            // Arrange
            var testPlaylist = Stubs.FullPlaylist(testUser, []);

            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]));
            mockPlaylist.Setup(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist));

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

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(0, 0, 0, 0);
        }

        [Test]
        public async Task Rename_Playlist() {
            // Arrange
            var testPlaylist = Stubs.FullPlaylist(testUser, []);
            var testPlaylistRenamed = Stubs.FullPlaylist(testUser, []);
            testPlaylistRenamed.SnapshotId = "new snapshot";
            testPlaylistRenamed.Name = "new name";

            mockPublicPlaylists.SetupSequence(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylist]))
                .Returns(Task.FromResult<IList<FullPlaylist>>([testPlaylistRenamed]));
            mockPlaylist.SetupSequence(m => m(It.Is<string>(s => s == testPlaylist.Id)))
                .Returns(Task.FromResult(testPlaylist))
                .Returns(Task.FromResult(testPlaylistRenamed));

            // Act
            var dataFetch = CreateDataFetch();
            await dataFetch.GetData(testUser.Id);
            await dataFetch.GetData(testUser.Id);

            // Assert
            AssertDbSetCounts(1, 0, 0, 0);
            AssertPlaylistData(testPlaylistRenamed, 0);
        }
    }
}
