using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpotifyAnalysis.Data.DTO;
using System;
using System.IO;


namespace SpotifyAnalysis.Data.DataAccessLayer {
    public class SpotifyContext : DbContext {
        public DbSet<AlbumDTO> Albums { get; set; }
        public DbSet<ArtistDTO> Artists { get; set; }
        public DbSet<ImageDTO> Images { get; set; }
        public DbSet<PlaylistDTO> Playlists { get; set; }
        public DbSet<TrackDTO> Tracks { get; set; }
        public DbSet<UserDTO> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) {
			if (options.IsConfigured)
				return;

			IConfigurationRoot configuration = Program.PrepareConfig();
            options.UseSqlServer(configuration.GetConnectionString("SpotifyDB"));
        }

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<UserDTO>()
				.HasMany(u => u.Playlists)
				.WithMany();

			modelBuilder.Entity<PlaylistDTO>()
				.HasMany(u => u.Tracks)
				.WithMany();
		}
	}
}
