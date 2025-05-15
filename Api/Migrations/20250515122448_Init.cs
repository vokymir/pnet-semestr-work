using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Birds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Genus = table.Column<string>(type: "TEXT", nullable: false),
                    Species = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Birds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    End = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AddingDeadline = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MainAdminId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Users_MainAdminId",
                        column: x => x.MainAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Watchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MainCuratorId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Watchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Watchers_Users_MainCuratorId",
                        column: x => x.MainCuratorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventAdmins",
                columns: table => new
                {
                    AdministeredEventsId = table.Column<int>(type: "INTEGER", nullable: false),
                    AdminsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAdmins", x => new { x.AdministeredEventsId, x.AdminsId });
                    table.ForeignKey(
                        name: "FK_EventAdmins_Events_AdministeredEventsId",
                        column: x => x.AdministeredEventsId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventAdmins_Users_AdminsId",
                        column: x => x.AdminsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventParticipants",
                columns: table => new
                {
                    ParticipantsId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParticipatingId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventParticipants", x => new { x.ParticipantsId, x.ParticipatingId });
                    table.ForeignKey(
                        name: "FK_EventParticipants_Events_ParticipatingId",
                        column: x => x.ParticipatingId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventParticipants_Watchers_ParticipantsId",
                        column: x => x.ParticipantsId,
                        principalTable: "Watchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateSeen = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BirdId = table.Column<int>(type: "INTEGER", nullable: false),
                    WatcherId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Records_Birds_BirdId",
                        column: x => x.BirdId,
                        principalTable: "Birds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Records_Watchers_WatcherId",
                        column: x => x.WatcherId,
                        principalTable: "Watchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WatcherCurators",
                columns: table => new
                {
                    CuratedWatchersId = table.Column<int>(type: "INTEGER", nullable: false),
                    CuratorsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatcherCurators", x => new { x.CuratedWatchersId, x.CuratorsId });
                    table.ForeignKey(
                        name: "FK_WatcherCurators_Users_CuratorsId",
                        column: x => x.CuratorsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatcherCurators_Watchers_CuratedWatchersId",
                        column: x => x.CuratedWatchersId,
                        principalTable: "Watchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmins_AdminsId",
                table: "EventAdmins",
                column: "AdminsId");

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipants_ParticipatingId",
                table: "EventParticipants",
                column: "ParticipatingId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_MainAdminId",
                table: "Events",
                column: "MainAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Records_BirdId",
                table: "Records",
                column: "BirdId");

            migrationBuilder.CreateIndex(
                name: "IX_Records_WatcherId",
                table: "Records",
                column: "WatcherId");

            migrationBuilder.CreateIndex(
                name: "IX_WatcherCurators_CuratorsId",
                table: "WatcherCurators",
                column: "CuratorsId");

            migrationBuilder.CreateIndex(
                name: "IX_Watchers_MainCuratorId",
                table: "Watchers",
                column: "MainCuratorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventAdmins");

            migrationBuilder.DropTable(
                name: "EventParticipants");

            migrationBuilder.DropTable(
                name: "Records");

            migrationBuilder.DropTable(
                name: "WatcherCurators");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Birds");

            migrationBuilder.DropTable(
                name: "Watchers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
