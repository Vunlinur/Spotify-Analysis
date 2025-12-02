using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class AddAlbumPopularityAndLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Popularity",
                table: "Albums",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "Popularity",
                table: "Albums");
        }
    }
}
