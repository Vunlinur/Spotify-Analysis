using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistFollowers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Followers",
                table: "Playlists",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Followers",
                table: "Playlists");
        }
    }
}
