using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class TrackArtistMany2Many : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Artists_Tracks_TrackDTOID",
                table: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_Artists_TrackDTOID",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "NeedsUpdate",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "TrackDTOID",
                table: "Artists");

            migrationBuilder.CreateTable(
                name: "ArtistDTOTrackDTO",
                columns: table => new
                {
                    ArtistsID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TrackDTOID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistDTOTrackDTO", x => new { x.ArtistsID, x.TrackDTOID });
                    table.ForeignKey(
                        name: "FK_ArtistDTOTrackDTO_Artists_ArtistsID",
                        column: x => x.ArtistsID,
                        principalTable: "Artists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistDTOTrackDTO_Tracks_TrackDTOID",
                        column: x => x.TrackDTOID,
                        principalTable: "Tracks",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistDTOTrackDTO_TrackDTOID",
                table: "ArtistDTOTrackDTO",
                column: "TrackDTOID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistDTOTrackDTO");

            migrationBuilder.AddColumn<bool>(
                name: "NeedsUpdate",
                table: "Playlists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TrackDTOID",
                table: "Artists",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artists_TrackDTOID",
                table: "Artists",
                column: "TrackDTOID");

            migrationBuilder.AddForeignKey(
                name: "FK_Artists_Tracks_TrackDTOID",
                table: "Artists",
                column: "TrackDTOID",
                principalTable: "Tracks",
                principalColumn: "ID");
        }
    }
}
