using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class EventScopedNotValidRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
