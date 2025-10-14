using Microsoft.EntityFrameworkCore;
using Moq;
using SpotifyAnalysis.Data.Database;
using SpotifyAPI.Web;
using NUnit.Framework.Legacy;
using Tests;
using Microsoft.Data.Sqlite;

namespace Tests.GetDataTests {
    public class GetDataBase {
        protected Mock<GetUserProfileDelegate> mockUserProfile;
        protected Mock<GetUsersPublicPlaylistsDelegate> mockPublicPlaylists;
        protected Mock<GetPlaylistAsyncDelegate> mockPlaylist;
        protected Mock<GetTracksAsyncDelegate> mockTracks;
        protected Mock<GetArtistsAsyncDelegate> mockArtists;
        protected Mock<UpdateProgressBarDelegate> mockProgressBar;

        protected SqliteConnection connection;
        protected SpotifyContext dbContext;
        protected PublicUser testUser;

        [OneTimeSetUp]
        public void OneTimeSetup() { }

        [SetUp]
        public void Setup() {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            void ConfigureSQLiteInMemory(DbContextOptionsBuilder options) {
                options.UseSqlite(connection);
                options.EnableSensitiveDataLogging();
            }
            SpotifyContext.Configurator = ConfigureSQLiteInMemory;
            dbContext = new SpotifyContext();
            dbContext.Database.EnsureCreated();


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

        static void ConfigureInMemory(DbContextOptionsBuilder options) {
            options.UseInMemoryDatabase(databaseName: "SpotifyDB-test");
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
            ClassicAssert.AreEqual(testPlaylist.Name, playlist.Name);
            ClassicAssert.AreEqual(testPlaylist.Owner.Id, playlist.OwnerID);
            ClassicAssert.AreEqual(expectedTrackCount, playlist.Tracks.Count);
        }

        protected void AssertTrackData(FullTrack testTrack, string albumID, string[] artistIDs, int inPlaylists = 1) {
            var track = dbContext.Tracks
                .Include(t => t.Playlists)
                .Include(t => t.Album)
                .Include(t => t.Artists)
                .FirstOrDefault(p => p.ID == testTrack.Id);
            ClassicAssert.IsNotNull(track);
            ClassicAssert.AreEqual(albumID, track.Album.ID);
            ClassicAssert.AreEqual(inPlaylists, track.Playlists.Count);
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
                int trackOffset = trackCount * p - p * 10;  // Add trackCount tracks for each playlist, 10 tracks overlapping
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
            connection.Dispose();
        }
    }
}
