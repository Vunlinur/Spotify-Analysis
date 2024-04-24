using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistTrackMany2Many : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_Playlists_PlaylistDTOID",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_PlaylistDTOID",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "PlaylistDTOID",
                table: "Tracks");

            migrationBuilder.CreateTable(
                name: "PlaylistDTOTrackDTO",
                columns: table => new
                {
                    PlaylistDTOID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TracksID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistDTOTrackDTO", x => new { x.PlaylistDTOID, x.TracksID });
                    table.ForeignKey(
                        name: "FK_PlaylistDTOTrackDTO_Playlists_PlaylistDTOID",
                        column: x => x.PlaylistDTOID,
                        principalTable: "Playlists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistDTOTrackDTO_Tracks_TracksID",
                        column: x => x.TracksID,
                        principalTable: "Tracks",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistDTOTrackDTO_TracksID",
                table: "PlaylistDTOTrackDTO",
                column: "TracksID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaylistDTOTrackDTO");

            migrationBuilder.AddColumn<string>(
                name: "PlaylistDTOID",
                table: "Tracks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_PlaylistDTOID",
                table: "Tracks",
                column: "PlaylistDTOID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_Playlists_PlaylistDTOID",
                table: "Tracks",
                column: "PlaylistDTOID",
                principalTable: "Playlists",
                principalColumn: "ID");
        }
    }
}
