using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nino.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    UpdateDisplay = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgressDisplay = table.Column<int>(type: "INTEGER", nullable: false),
                    CongaPrefix = table.Column<int>(type: "INTEGER", nullable: false),
                    ReleasePrefix = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Locale = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GuildId = table.Column<string>(type: "TEXT", nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    PosterUri = table.Column<string>(type: "TEXT", nullable: false),
                    UpdateChannelId = table.Column<string>(type: "TEXT", nullable: false),
                    ReleaseChannelId = table.Column<string>(type: "TEXT", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    CongaParticipants = table.Column<string>(type: "TEXT", nullable: false),
                    Motd = table.Column<string>(type: "TEXT", nullable: true),
                    AniListId = table.Column<int>(type: "INTEGER", nullable: true),
                    AniListOffset = table.Column<int>(type: "INTEGER", nullable: true),
                    AirReminderEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AirReminderChannelId = table.Column<string>(type: "TEXT", nullable: true),
                    AirReminderRoleId = table.Column<string>(type: "TEXT", nullable: true),
                    AirReminderUserId = table.Column<string>(type: "TEXT", nullable: true),
                    CongaReminderEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CongaReminderPeriod = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    CongaReminderChannelId = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configurations_Administrators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations_Administrators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Configurations_Administrators_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alias_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GuildId = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Done = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReminderPosted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Episodes_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Observers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GuildId = table.Column<string>(type: "TEXT", nullable: false),
                    OriginGuildId = table.Column<string>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Blame = table.Column<bool>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: true),
                    ProgressWebhook = table.Column<string>(type: "TEXT", nullable: true),
                    ReleasesWebhook = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Observers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects_Administrators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects_Administrators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Administrators_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects_KeyStaff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Role_Abbreviation = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Role_Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Role_Weight = table.Column<decimal>(type: "TEXT", nullable: true),
                    IsPseudo = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects_KeyStaff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_KeyStaff_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Episodes_AdditionalStaff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Role_Abbreviation = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Role_Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Role_Weight = table.Column<decimal>(type: "TEXT", nullable: true),
                    IsPseudo = table.Column<bool>(type: "INTEGER", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes_AdditionalStaff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Episodes_AdditionalStaff_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PinchHitter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinchHitter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PinchHitter_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Task",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", nullable: false),
                    Done = table.Column<bool>(type: "INTEGER", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastReminded = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EpisodeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Task", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Task_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alias_ProjectId",
                table: "Alias",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_Administrators_ConfigurationId",
                table: "Configurations_Administrators",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_ProjectId",
                table: "Episodes",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_AdditionalStaff_EpisodeId",
                table: "Episodes_AdditionalStaff",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Observers_ProjectId",
                table: "Observers",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PinchHitter_EpisodeId",
                table: "PinchHitter",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Administrators_ProjectId",
                table: "Projects_Administrators",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_KeyStaff_ProjectId",
                table: "Projects_KeyStaff",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Task_EpisodeId",
                table: "Task",
                column: "EpisodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alias");

            migrationBuilder.DropTable(
                name: "Configurations_Administrators");

            migrationBuilder.DropTable(
                name: "Episodes_AdditionalStaff");

            migrationBuilder.DropTable(
                name: "Observers");

            migrationBuilder.DropTable(
                name: "PinchHitter");

            migrationBuilder.DropTable(
                name: "Projects_Administrators");

            migrationBuilder.DropTable(
                name: "Projects_KeyStaff");

            migrationBuilder.DropTable(
                name: "Task");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
