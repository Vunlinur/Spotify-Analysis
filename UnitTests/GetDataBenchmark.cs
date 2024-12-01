using Moq;
using SpotifyAnalysis.Data.Database;
using SpotifyAPI.Web;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace UnitTests {
    [MemoryDiagnoser]
    public class GetDataBenchmark : GetDataBase {

        private DataFetch? dataFetch;
        private MockData? data;
        
        [GlobalSetup]
        public void GlobalSetup() {
            OneTimeSetup();
        }

        [IterationSetup]
        public void IterationSetup() {
            Setup();

            data = GenerateLargeMockData();
            mockPublicPlaylists.Setup(m => m(It.Is<string>(s => s == testUser.Id)))
                .Returns(Task.FromResult<IList<FullPlaylist>>(data.Playlists));
            mockPlaylist.Setup(m => m(It.IsAny<string>()))
                .Returns<string>(playlistId => Task.FromResult(data.Playlists.FirstOrDefault(p => p.Id == playlistId)));
            mockTracks.Setup(m => m(It.IsAny<Paging<PlaylistTrack<IPlayableItem>>>()))
                .Returns<Paging<PlaylistTrack<IPlayableItem>>>(paging => Task.FromResult(paging.Items.Select(i => (FullTrack)i.Track).ToList()));
            mockArtists.Setup(m => m(It.IsAny<IList<string>>()))
                .Returns<IList<string>>(ids => Task.FromResult(data.FullArtists.Where(a => ids.Contains(a.Id)).ToList()));

            dataFetch = CreateDataFetch();
        }

        [Benchmark]
        public async Task BenchmarkGetData() {
            await dataFetch.GetData(data.User.Id);
        }

        [IterationCleanup]
        public void IterationCleanup() {
            TearDown();
        }
    }
}
