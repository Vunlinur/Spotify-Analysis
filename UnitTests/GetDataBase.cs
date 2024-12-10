using Microsoft.EntityFrameworkCore;
using Moq;
using SpotifyAnalysis.Data.Database;
using SpotifyAPI.Web;
using NUnit.Framework.Legacy;

namespace UnitTests {
    public class GetDataBase {
        protected Mock<GetUserProfileDelegate> mockUserProfile;
        protected Mock<GetUsersPublicPlaylistsDelegate> mockPublicPlaylists;
        protected Mock<GetPlaylistAsyncDelegate> mockPlaylist;
        protected Mock<GetTracksAsyncDelegate> mockTracks;
        protected Mock<GetArtistsAsyncDelegate> mockArtists;
        protected Mock<UpdateProgressBarDelegate> mockProgressBar;

        protected SpotifyContext dbContext;
        protected PublicUser testUser;

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

        protected DataFetch CreateDataFetch() =>
            new(
                mockUserProfile.Object,
                mockPublicPlaylists.Object,
                mockPlaylist.Object,
                mockTracks.Object,
                mockArtists.Object,
                mockProgressBar.Object
            );

        protected void AssertDbSetCounts(int playlistCount, int trackCount, int artistCount, int albumCount) {
            ClassicAssert.AreEqual(playlistCount, dbContext.Playlists.Count());
            ClassicAssert.AreEqual(trackCount, dbContext.Tracks.Count());
            ClassicAssert.AreEqual(artistCount, dbContext.Artists.Count());
            ClassicAssert.AreEqual(albumCount, dbContext.Albums.Count());
        }

        protected void AssertPlaylistData(FullPlaylist testPlaylist, int expectedTrackCount) {
            var playlist = dbContext.Playlists
                .Include(t => t.Tracks)
                .FirstOrDefault(p => p.ID == testPlaylist.Id);
            ClassicAssert.IsNotNull(playlist);
            ClassicAssert.AreEqual(testPlaylist.Owner.Id, playlist.OwnerID);
            ClassicAssert.AreEqual(expectedTrackCount, playlist.Tracks.Count);
        }

        protected void AssertTrackData(FullTrack testTrack, string albumID, string[] artistIDs) {
            var track = dbContext.Tracks
                .Include(t => t.Album)
                .Include(t => t.Artists)
                .FirstOrDefault(p => p.ID == testTrack.Id);
            ClassicAssert.IsNotNull(track);
            ClassicAssert.AreEqual(albumID, track.Album.ID);
            ClassicAssert.AreEqual(1, track.Playlists.Count);
            CollectionAssert.AreEquivalent(artistIDs, track.Artists.Select(a => a.ID));
        }

        protected class MockData(PublicUser user) {
            public PublicUser User = user;
            public List<SimpleArtist> SimpleArtists = [];
            public List<FullArtist> FullArtists = [];
            public List<SimpleAlbum> Albums = [];
            public List<FullPlaylist> Playlists = [];
            public List<FullTrack> Tracks = [];
        }

        protected static MockData GenerateLargeMockData() {
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

            int trackCount = 1000;
            // Step 2: Generate 100 playlists, each with 1,000 tracks
            for (int p = 0; p < 100; p++) {
                var tracks = new List<FullTrack>();
                int trackOffset = trackCount * p - (p * 10);  // Add trackCount tracks for each playlist, 10 tracks overlapping
                for (int t = trackOffset; t < trackOffset + trackCount; t++) {
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

        [TearDown]
        public void TearDown() {
            dbContext.Database.EnsureDeleted();
            dbContext?.Dispose();
        }
    }
}
