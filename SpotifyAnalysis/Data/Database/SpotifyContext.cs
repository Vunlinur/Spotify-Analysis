using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpotifyAnalysis.Data.DTO;
using System;
using System.Collections.Generic;
using System.IO;


namespace SpotifyAnalysis.Data.DataAccessLayer {
    public class SpotifyContext : DbContext {
        public static Action<DbContextOptionsBuilder> Configurator { get; set; } = ConfigureSqlServer;

        public DbSet<AlbumDTO> Albums { get; set; }
        public DbSet<ArtistDTO> Artists { get; set; }
        public DbSet<ImageDTO> Images { get; set; }
        public DbSet<PlaylistDTO> Playlists { get; set; }
        public DbSet<TrackDTO> Tracks { get; set; }
        public DbSet<UserDTO> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) {
			if (options.IsConfigured)
				return;

            Configurator(options);
        }

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<UserDTO>()
				.HasMany(u => u.Playlists)
				.WithMany();

            modelBuilder.Entity<TrackDTO>()
                .HasMany(u => u.Artists)
                .WithMany();

            modelBuilder.Entity<PlaylistDTO>()
                .HasMany(p => p.Tracks)
                .WithMany(t => t.Playlists)
                .UsingEntity<Dictionary<string, object>>(
                    "PlaylistDTOTrackDTO",  // let's keep the auto convention
                    j => j.HasOne<TrackDTO>().WithMany().HasForeignKey("TracksID"),
                    j => j.HasOne<PlaylistDTO>().WithMany().HasForeignKey("PlaylistDTOID"),
                    j => {
                        j.HasKey("PlaylistDTOID", "TracksID");
                        j.Property<string>("PlaylistDTOID");
                        j.Property<string>("TracksID");
                    });
        }

        protected static void ConfigureSqlServer(DbContextOptionsBuilder options) {
            IConfigurationRoot configuration = Program.PrepareConfig();
            options.UseSqlServer(configuration.GetConnectionString("SpotifyDB"));
            options.LogTo(Console.WriteLine, LogLevel.Information);
        }
    }
}
