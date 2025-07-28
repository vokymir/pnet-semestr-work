using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class NotValidRecordsRethought : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Records_Events_EventId",
                table: "Records");

            migrationBuilder.DropIndex(
                name: "IX_Records_EventId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Records");

            migrationBuilder.CreateTable(
                name: "InvalidRecordsInEvents",
                columns: table => new
                {
                    InvalidInEventsId = table.Column<int>(type: "INTEGER", nullable: false),
                    NotValidRecordsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvalidRecordsInEvents", x => new { x.InvalidInEventsId, x.NotValidRecordsId });
                    table.ForeignKey(
                        name: "FK_InvalidRecordsInEvents_Events_InvalidInEventsId",
                        column: x => x.InvalidInEventsId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvalidRecordsInEvents_Records_NotValidRecordsId",
                        column: x => x.NotValidRecordsId,
                        principalTable: "Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvalidRecordsInEvents_NotValidRecordsId",
                table: "InvalidRecordsInEvents",
                column: "NotValidRecordsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvalidRecordsInEvents");

            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "Records",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Records_EventId",
                table: "Records",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Records_Events_EventId",
                table: "Records",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id");
        }
    }
}
