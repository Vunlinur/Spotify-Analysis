using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistNeedsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedsUpdate",
                table: "Playlists",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsUpdate",
                table: "Playlists");
        }
    }
}
