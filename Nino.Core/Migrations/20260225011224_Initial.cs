using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nino.Core.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AniListCache",
                columns: table => new
                {
                    AniListId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    FetchedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AniListCache", x => x.AniListId);
                });

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiscordId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MentionRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiscordId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentionRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StateCache",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Json = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, collation: "NOCASE"),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Number = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, collation: "NOCASE"),
                    IsDone = table.Column<bool>(type: "INTEGER", nullable: false),
                    AirNotificationPosted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Task",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    IsDone = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LastRemindedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "Episodes_AdditionalStaff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role_Abbreviation = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Role_Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Role_Weight = table.Column<decimal>(type: "TEXT", nullable: false),
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
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Configuration_ProgressResponseType = table.Column<int>(type: "INTEGER", nullable: false),
                    Configuration_ProgressPublishType = table.Column<int>(type: "INTEGER", nullable: false),
                    Configuration_CongaPrefix = table.Column<int>(type: "INTEGER", nullable: false),
                    Configuration_ReleasePrefix = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Configuration_Locale = table.Column<int>(type: "INTEGER", nullable: false),
                    Configuration_PublishPrivateProgress = table.Column<bool>(type: "INTEGER", nullable: false),
                    DiscordId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DiscordId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Groups_Administrators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups_Administrators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Administrators_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Groups_Administrators_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PinchHitter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
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
                    table.ForeignKey(
                        name: "FK_PinchHitter_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, collation: "NOCASE"),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    PosterUrl = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ProjectChannelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdateChannelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReleaseChannelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    CongaParticipants = table.Column<string>(type: "TEXT", nullable: false),
                    Motd = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    AniListId = table.Column<int>(type: "INTEGER", nullable: false),
                    AniListOffset = table.Column<int>(type: "INTEGER", nullable: false),
                    AirNotificationsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CongaRemindersEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    AirNotificationUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AirNotificationRoleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AirNotificationDelay = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    CongaReminderPeriod = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    AirNotificationChannelId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Channels_AirNotificationChannelId",
                        column: x => x.AirNotificationChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Projects_Channels_ProjectChannelId",
                        column: x => x.ProjectChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Channels_ReleaseChannelId",
                        column: x => x.ReleaseChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Channels_UpdateChannelId",
                        column: x => x.UpdateChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projects_MentionRoles_AirNotificationRoleId",
                        column: x => x.AirNotificationRoleId,
                        principalTable: "MentionRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Users_AirNotificationUserId",
                        column: x => x.AirNotificationUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Observers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Blame = table.Column<bool>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ProgressWebhook = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ReleasesWebhook = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Observers_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Observers_Groups_OriginGroupId",
                        column: x => x.OriginGroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Observers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Observers_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects_Administrators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_Projects_Administrators_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects_KeyStaff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role_Abbreviation = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Role_Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Role_Weight = table.Column<decimal>(type: "TEXT", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_Projects_KeyStaff_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alias_ProjectId",
                table: "Alias",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_ProjectId",
                table: "Episodes",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_AdditionalStaff_EpisodeId",
                table: "Episodes_AdditionalStaff",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_AdditionalStaff_UserId",
                table: "Episodes_AdditionalStaff",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_OwnerId",
                table: "Groups",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Administrators_GroupId",
                table: "Groups_Administrators",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Administrators_UserId",
                table: "Groups_Administrators",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Observers_GroupId",
                table: "Observers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Observers_OriginGroupId",
                table: "Observers",
                column: "OriginGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Observers_OwnerId",
                table: "Observers",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Observers_ProjectId",
                table: "Observers",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PinchHitter_EpisodeId",
                table: "PinchHitter",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PinchHitter_UserId",
                table: "PinchHitter",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_AirNotificationChannelId",
                table: "Projects",
                column: "AirNotificationChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_AirNotificationRoleId",
                table: "Projects",
                column: "AirNotificationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_AirNotificationUserId",
                table: "Projects",
                column: "AirNotificationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_GroupId",
                table: "Projects",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OwnerId",
                table: "Projects",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectChannelId",
                table: "Projects",
                column: "ProjectChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ReleaseChannelId",
                table: "Projects",
                column: "ReleaseChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UpdateChannelId",
                table: "Projects",
                column: "UpdateChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Administrators_ProjectId",
                table: "Projects_Administrators",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Administrators_UserId",
                table: "Projects_Administrators",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_KeyStaff_ProjectId",
                table: "Projects_KeyStaff",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_KeyStaff_UserId",
                table: "Projects_KeyStaff",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Task_EpisodeId",
                table: "Task",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DiscordId",
                table: "Users",
                column: "DiscordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GroupId",
                table: "Users",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alias_Projects_ProjectId",
                table: "Alias",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_Projects_ProjectId",
                table: "Episodes",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_AdditionalStaff_Users_UserId",
                table: "Episodes_AdditionalStaff",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Users_OwnerId",
                table: "Groups",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Users_OwnerId",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "Alias");

            migrationBuilder.DropTable(
                name: "AniListCache");

            migrationBuilder.DropTable(
                name: "Episodes_AdditionalStaff");

            migrationBuilder.DropTable(
                name: "Groups_Administrators");

            migrationBuilder.DropTable(
                name: "Observers");

            migrationBuilder.DropTable(
                name: "PinchHitter");

            migrationBuilder.DropTable(
                name: "Projects_Administrators");

            migrationBuilder.DropTable(
                name: "Projects_KeyStaff");

            migrationBuilder.DropTable(
                name: "StateCache");

            migrationBuilder.DropTable(
                name: "Task");

            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "MentionRoles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
