using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistOwnerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Owner",
                table: "Playlists",
                newName: "OwnerID");

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "Playlists",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "Playlists");

            migrationBuilder.RenameColumn(
                name: "OwnerID",
                table: "Playlists",
                newName: "Owner");
        }
    }
}
