using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleaseDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalTracks = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Owner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Followers = table.Column<int>(type: "int", nullable: false),
                    UserDTOID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Playlists_Users_UserDTOID",
                        column: x => x.UserDTOID,
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    Popularity = table.Column<int>(type: "int", nullable: false),
                    AlbumID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PlaylistDTOID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tracks_Albums_AlbumID",
                        column: x => x.AlbumID,
                        principalTable: "Albums",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Tracks_Playlists_PlaylistDTOID",
                        column: x => x.PlaylistDTOID,
                        principalTable: "Playlists",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Popularity = table.Column<int>(type: "int", nullable: false),
                    Genres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrackDTOID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Artists_Tracks_TrackDTOID",
                        column: x => x.TrackDTOID,
                        principalTable: "Tracks",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "AlbumDTOArtistDTO",
                columns: table => new
                {
                    AlbumsID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ArtistsID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumDTOArtistDTO", x => new { x.AlbumsID, x.ArtistsID });
                    table.ForeignKey(
                        name: "FK_AlbumDTOArtistDTO_Albums_AlbumsID",
                        column: x => x.AlbumsID,
                        principalTable: "Albums",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumDTOArtistDTO_Artists_ArtistsID",
                        column: x => x.ArtistsID,
                        principalTable: "Artists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resolution = table.Column<int>(type: "int", nullable: false),
                    AlbumDTOID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ArtistDTOID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PlaylistDTOID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Images_Albums_AlbumDTOID",
                        column: x => x.AlbumDTOID,
                        principalTable: "Albums",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Images_Artists_ArtistDTOID",
                        column: x => x.ArtistDTOID,
                        principalTable: "Artists",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Images_Playlists_PlaylistDTOID",
                        column: x => x.PlaylistDTOID,
                        principalTable: "Playlists",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumDTOArtistDTO_ArtistsID",
                table: "AlbumDTOArtistDTO",
                column: "ArtistsID");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_TrackDTOID",
                table: "Artists",
                column: "TrackDTOID");

            migrationBuilder.CreateIndex(
                name: "IX_Images_AlbumDTOID",
                table: "Images",
                column: "AlbumDTOID");

            migrationBuilder.CreateIndex(
                name: "IX_Images_ArtistDTOID",
                table: "Images",
                column: "ArtistDTOID");

            migrationBuilder.CreateIndex(
                name: "IX_Images_PlaylistDTOID",
                table: "Images",
                column: "PlaylistDTOID");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_UserDTOID",
                table: "Playlists",
                column: "UserDTOID");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_AlbumID",
                table: "Tracks",
                column: "AlbumID");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_PlaylistDTOID",
                table: "Tracks",
                column: "PlaylistDTOID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumDTOArtistDTO");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.DropTable(
                name: "Tracks");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
