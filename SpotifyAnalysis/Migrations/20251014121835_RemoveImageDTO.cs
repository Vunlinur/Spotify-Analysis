using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class RemoveImageDTO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.AddColumn<string>(
                name: "ImageL",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageS",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageL",
                table: "Playlists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageS",
                table: "Playlists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageL",
                table: "Artists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageS",
                table: "Artists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageL",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageS",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageL",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ImageS",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ImageL",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "ImageS",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "ImageL",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "ImageS",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "ImageL",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ImageS",
                table: "Albums");

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Url = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AlbumDTOID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ArtistDTOID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PlaylistDTOID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Resolution = table.Column<int>(type: "int", nullable: false),
                    UserDTOID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Url);
                    table.ForeignKey(
                        name: "FK_Images_Albums_AlbumDTOID",
                        column: x => x.AlbumDTOID,
                        principalTable: "Albums",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Images_Artists_ArtistDTOID",
                        column: x => x.ArtistDTOID,
                        principalTable: "Artists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Images_Playlists_PlaylistDTOID",
                        column: x => x.PlaylistDTOID,
                        principalTable: "Playlists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Images_Users_UserDTOID",
                        column: x => x.UserDTOID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_Images_UserDTOID",
                table: "Images",
                column: "UserDTOID");
        }
    }
}
