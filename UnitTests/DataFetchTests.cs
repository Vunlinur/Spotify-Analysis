using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpotifyAnalysis.Data.DataAccessLayer;

namespace UnitTests {
    public class Tests {
        [SetUp]
        public void Setup() {
            SpotifyContext.Configurator = ConfigureInMemory;
        }

        [Test]
        public void GetData_GetsOneTrack() {
            // TODO test
        }

        [TearDown] public void TearDown() {
            //var dbContext = _serviceProvider.GetService<CartDbContext>();
            //dbContext.Database.EnsureDeleted();
        }

        protected static void ConfigureInMemory(DbContextOptionsBuilder options) {
            options.UseInMemoryDatabase(databaseName: "SpotifyDB-test");
        }
    }
}