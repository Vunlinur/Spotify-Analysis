using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistDTOIDCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Albums_AlbumDTOID",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Artists_ArtistDTOID",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Playlists_PlaylistDTOID",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Users_UserDTOID",
                table: "Images");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Albums_AlbumDTOID",
                table: "Images",
                column: "AlbumDTOID",
                principalTable: "Albums",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Artists_ArtistDTOID",
                table: "Images",
                column: "ArtistDTOID",
                principalTable: "Artists",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Playlists_PlaylistDTOID",
                table: "Images",
                column: "PlaylistDTOID",
                principalTable: "Playlists",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Users_UserDTOID",
                table: "Images",
                column: "UserDTOID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Albums_AlbumDTOID",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Artists_ArtistDTOID",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Playlists_PlaylistDTOID",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Users_UserDTOID",
                table: "Images");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Albums_AlbumDTOID",
                table: "Images",
                column: "AlbumDTOID",
                principalTable: "Albums",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Artists_ArtistDTOID",
                table: "Images",
                column: "ArtistDTOID",
                principalTable: "Artists",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Playlists_PlaylistDTOID",
                table: "Images",
                column: "PlaylistDTOID",
                principalTable: "Playlists",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Users_UserDTOID",
                table: "Images",
                column: "UserDTOID",
                principalTable: "Users",
                principalColumn: "ID");
        }
    }
}
