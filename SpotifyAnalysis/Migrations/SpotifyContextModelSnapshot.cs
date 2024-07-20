﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SpotifyAnalysis.Data.DataAccessLayer;

#nullable disable

namespace SpotifyAnalysis.Migrations
{
    [DbContext(typeof(SpotifyContext))]
    partial class SpotifyContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AlbumDTOArtistDTO", b =>
                {
                    b.Property<string>("AlbumsID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ArtistsID")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("AlbumsID", "ArtistsID");

                    b.HasIndex("ArtistsID");

                    b.ToTable("AlbumDTOArtistDTO");
                });

            modelBuilder.Entity("ArtistDTOTrackDTO", b =>
                {
                    b.Property<string>("ArtistsID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("TrackDTOID")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ArtistsID", "TrackDTOID");

                    b.HasIndex("TrackDTOID");

                    b.ToTable("ArtistDTOTrackDTO");
                });

            modelBuilder.Entity("PlaylistDTOTrackDTO", b =>
                {
                    b.Property<string>("PlaylistDTOID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("TracksID")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("PlaylistDTOID", "TracksID");

                    b.HasIndex("TracksID");

                    b.ToTable("PlaylistDTOTrackDTO");
                });

            modelBuilder.Entity("PlaylistDTOUserDTO", b =>
                {
                    b.Property<string>("PlaylistsID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserDTOID")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("PlaylistsID", "UserDTOID");

                    b.HasIndex("UserDTOID");

                    b.ToTable("PlaylistDTOUserDTO");
                });

            modelBuilder.Entity("SpotifyAnalysis.Data.DTO.AlbumDTO", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ReleaseDate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TotalTracks")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("Albums");
                });

            modelBuilder.Entity("SpotifyAnalysis.Data.DTO.ArtistDTO", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Genres")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Popularity")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("SpotifyAnalysis.Data.DTO.ImageDTO", b =>
                {
                    b.Property<int>("ImageID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ImageID"));

                    b.Property<string>("AlbumDTOID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ArtistDTOID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PlaylistDTOID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Resolution")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserDTOID")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ImageID");

                    b.HasIndex("AlbumDTOID");

                    b.HasIndex("ArtistDTOID");

                    b.HasIndex("PlaylistDTOID");

                    b.HasIndex("UserDTOID");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("SpotifyAnalysis.Data.DTO.PlaylistDTO", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Followers")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Owner")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SnapshotID")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TracksTotal")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("Playlists");
                });

            modelBuilder.Entity("SpotifyAnalysis.Data.DTO.TrackDTO", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AlbumID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("DurationMs")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Popularity")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("AlbumID");

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("SpotifyAnalysis.Data.DTO.UserDTO", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("AlbumDTOArtistDTO", b =>
                {
                    b.HasOne("SpotifyAnalysis.Data.DTO.AlbumDTO", null)
                        .WithMany()
                        .HasForeignKey("AlbumsID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SpotifyAnalysis.Data.DTO.ArtistDTO", null)
                        .WithMany()
                        .HasForeignKey("ArtistsID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ArtistDTOTrackDTO", b =>
                {
                    b.HasOne("SpotifyAnalysis.Data.DTO.ArtistDTO", null)
                        .WithMany()
                        .HasForeignKey("ArtistsID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SpotifyAnalysis.Data.DTO.TrackDTO", null)
                        .WithMany()
                        .HasForeignKey("TrackDTOID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PlaylistDTOTrackDTO", b =>
                {
                    b.HasOne("SpotifyAnalysis.Data.DTO.PlaylistDTO", null)
                        .WithMany()
                        .HasForeignKey("PlaylistDTOID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SpotifyAnalysis.Data.DTO.TrackDTO", null)
                        .WithMany()
                        .HasForeignKey("TracksID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PlaylistDTOUserDTO", b =>
                {
                    b.HasOne("SpotifyAnalysis.Data.DTO.PlaylistDTO", null)
                        .WithMany()
                        .HasForeignKey("PlaylistsID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SpotifyAnalysis.Data.DTO.UserDTO", null)
                        .WithMany()
                        .HasForeignKey("UserDTOID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SpotifyAnalysis.Data.DTO.ImageDTO", b =>
                {
                    b.HasOne("SpotifyAnalysis.Data.DTO.AlbumDTO", null)
                        .WithMany("Images")
                        .HasForeignKey("AlbumDTOID");

                    b.HasOne("SpotifyAnalysis.Data.DTO.ArtistDTO", null)
                        .WithMany("Images")
                        .HasForeignKey("ArtistDTOID");

                    b.HasOne("SpotifyAnalysis.Data.DTO.PlaylistDTO", null)
                        .WithMany("Images")
                        .HasForeignKey("PlaylistDTOID");

                    b.HasOne("SpotifyAnalysis.Data.DTO.UserDTO", null)
                        .WithMany("Images")
                        .HasForeignKey("UserDTOID");
                });

            modelBuilder.Entity("SpotifyAnalysis.Data.DTO.TrackDTO", b =>
                {
                    b.HasOne("SpotifyAnalysis.Data.DTO.AlbumDTO", "Album")
                        .WithMany("Tracks")
                        .HasForeignKey("AlbumID");

                    b.Navigation("Album");
                });

            modelBuilder.Entity("SpotifyAnalysis.Data.DTO.AlbumDTO", b =>
                {
                    b.Navigation("Images");

                    b.Navigation("Tracks");
                });

            modelBuilder.Entity("SpotifyAnalysis.Data.DTO.ArtistDTO", b =>
                {
                    b.Navigation("Images");
                });

            modelBuilder.Entity("SpotifyAnalysis.Data.DTO.PlaylistDTO", b =>
                {
                    b.Navigation("Images");
                });

            modelBuilder.Entity("SpotifyAnalysis.Data.DTO.UserDTO", b =>
                {
                    b.Navigation("Images");
                });
#pragma warning restore 612, 618
        }
    }
}
