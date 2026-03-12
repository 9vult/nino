using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nino.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AniListCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    FetchedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AniListCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiscordId = table.Column<string>(type: "TEXT", nullable: true),
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
                    DiscordId = table.Column<string>(type: "TEXT", nullable: true),
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
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DiscordId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiscordId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProgressResponseType = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgressPublishType = table.Column<int>(type: "INTEGER", nullable: false),
                    CongaPrefix = table.Column<int>(type: "INTEGER", nullable: false),
                    ReleasePrefix = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PublishPrivateProgress = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Configuration_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, collation: "NOCASE"),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PosterUrl = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Motd = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    AniListId = table.Column<int>(type: "INTEGER", nullable: false),
                    AniListOffset = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectChannelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdateChannelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReleaseChannelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    AirNotificationEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AirNotificationDelay = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    AirNotificationUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AirNotificationRoleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CongaRemindersEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CongaReminderPeriod = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    CongaParticipants = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
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
                name: "Configuration_Administrators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuration_Administrators", x => new { x.ConfigurationId, x.Id });
                    table.ForeignKey(
                        name: "FK_Configuration_Administrators_Configuration_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configuration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Configuration_Administrators_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alias", x => new { x.ProjectId, x.Id });
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
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Number = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, collation: "NOCASE"),
                    IsDone = table.Column<bool>(type: "INTEGER", nullable: false),
                    AirNotificationPosted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Episodes_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Blame = table.Column<bool>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                        name: "FK_Observers_MentionRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "MentionRoles",
                        principalColumn: "Id");
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
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects_Administrators", x => new { x.ProjectId, x.Id });
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
                name: "PinchHitters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinchHitters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PinchHitters_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PinchHitters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role_Abbreviation = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Role_Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Role_Weight = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsPseudo = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EpisodeId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                    table.CheckConstraint("CK_Staff_SingleOwner", "(ProjectId IS NULL) != (EpisodeId IS NULL)");
                    table.ForeignKey(
                        name: "FK_Staff_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Staff_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Staff_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    IsDone = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LastRemindedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Configuration_GroupId",
                table: "Configuration",
                column: "GroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Configuration_Administrators_UserId",
                table: "Configuration_Administrators",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_GroupId",
                table: "Episodes",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_ProjectId",
                table: "Episodes",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_OwnerId",
                table: "Groups",
                column: "OwnerId");

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
                name: "IX_Observers_RoleId",
                table: "Observers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PinchHitters_EpisodeId",
                table: "PinchHitters",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PinchHitters_UserId",
                table: "PinchHitters",
                column: "UserId");

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
                name: "IX_Projects_Administrators_UserId",
                table: "Projects_Administrators",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_EpisodeId",
                table: "Staff",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_ProjectId",
                table: "Staff",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_UserId",
                table: "Staff",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_EpisodeId",
                table: "Tasks",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DiscordId",
                table: "Users",
                column: "DiscordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alias");

            migrationBuilder.DropTable(
                name: "AniListCache");

            migrationBuilder.DropTable(
                name: "Configuration_Administrators");

            migrationBuilder.DropTable(
                name: "Observers");

            migrationBuilder.DropTable(
                name: "PinchHitters");

            migrationBuilder.DropTable(
                name: "Projects_Administrators");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "StateCache");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Configuration");

            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "MentionRoles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
