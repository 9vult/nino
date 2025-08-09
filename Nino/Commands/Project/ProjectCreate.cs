using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Services;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("create", "Create a new project")]
        public async Task<RuntimeResult> Create(
            [Summary("nickname", "Short project nickname (no spaces)")] string nickname,
            [Summary("anilistId", "AniList ID")] int aniListId,
            [Summary("private", "Is this project private?")] bool isPrivate,
            [Summary("updateChannel", "Channel to post updates to"), ChannelTypes(ChannelType.Text, ChannelType.News)]IMessageChannel updateChannel,
            [Summary("releaseChannel", "Channel to post releases to"), ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel releaseChannel,
            [Summary("title", "Full series title")] string? title = null,
            [Summary("type", "Project type")] ProjectType? type = null,
            [Summary("length", "Number of episodes"), MinValue(1)] int? length = null,
            [Summary("firstEpisode", "First episode number")] decimal firstEpisode = 1,
            [Summary("poster", "Poster image URL")] string? posterUri = null
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            var guildId = interaction.GuildId ?? 0;
            var guild = Nino.Client.GetGuild(guildId);
            var member = guild.GetUser(interaction.User.Id);
            if (!Utils.VerifyAdministrator(member, guild))
                return await Response.Fail(T("error.notPrivileged", lng), interaction);

            // Get inputs
            var updateChannelId = updateChannel.Id;
            var releaseChannelId = releaseChannel.Id;
            var ownerId = interaction.User.Id;

            // Sanitize input
            nickname = nickname.Trim().ToLowerInvariant().Replace(" ", string.Empty); // remove spaces

            // Verify data
            if (db.Projects.Any(p => p.GuildId == guildId && p.Nickname == nickname))
                return await Response.Fail(
                    T("error.project.nameInUse", lng, nickname),
                    interaction
                );

            var apiResponse = await AniListService.Get(aniListId);
            if (apiResponse is not null && apiResponse.Error is null)
            {
                title ??= apiResponse.Title;
                length ??= apiResponse.EpisodeCount;
                type ??= apiResponse.Type;
                
                if (title is null || length is null)
                {
                    return await Response.Fail(
                        T(apiResponse.Error ?? "error.anilist.create", lng),
                        interaction
                    );
                }
            }
            else
            {
                return await Response.Fail(
                    T(apiResponse?.Error ?? "error.anilist.create", lng),
                    interaction
                );
            }

            if (posterUri is null || !Uri.TryCreate(posterUri, UriKind.Absolute, out _))
            {
                posterUri = apiResponse?.CoverImage ?? AniListService.FallbackPosterUri;
            }

            // Populate data

            var projectData = new Project
            {
                Id = Guid.NewGuid(),
                GuildId = guildId,
                Nickname = nickname,
                Title = title!,
                OwnerId = ownerId,
                Type = type!.Value,
                PosterUri = posterUri,
                UpdateChannelId = updateChannelId,
                ReleaseChannelId = releaseChannelId,
                IsPrivate = isPrivate,
                IsArchived = false,
                AirReminderEnabled = false,
                CongaReminderEnabled = false,
                CongaParticipants = new CongaGraph(),
                AniListId = aniListId,
                Created = DateTimeOffset.UtcNow,
            };

            var episodes = new List<Episode>();
            for (var i = firstEpisode; i < firstEpisode + length; i++)
            {
                episodes.Add(
                    new Episode
                    {
                        GuildId = guildId,
                        ProjectId = projectData.Id,
                        Number = $"{i}",
                        Done = false,
                        ReminderPosted = false,
                        AdditionalStaff = [],
                        PinchHitters = [],
                        Tasks = [],
                    }
                );
            }

            Log.Info(
                $"Creating project {projectData} for M[{ownerId} (@{member.Username})] with {episodes.Count} episodes, starting with episode {firstEpisode}"
            );

            // Add project and episodes to database
            await db.Projects.AddAsync(projectData);
            await db.Episodes.AddRangeAsync(episodes);

            // Create configuration if the guild doesn't have one yet
            if (db.GetConfig(guildId) == null)
            {
                Log.Info($"Creating default configuration for guild {guildId}");
                await db.Configurations.AddAsync(Configuration.CreateDefault(guildId));
            }

            var builder = new StringBuilder();
            builder.AppendLine(T("project.created", lng, nickname));

            if (firstEpisode != 1)
            {
                builder.AppendLine();
                builder.AppendLine(T("project.created.firstEpisode", lng, firstEpisode));
            }

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(builder.ToString())
                .Build();
            await interaction.FollowupAsync(embed: embed);

            // Check progress channel permissions
            if (!PermissionChecker.CheckPermissions(updateChannelId))
                await Response.Info(
                    T("error.missingChannelPerms", lng, $"<#{updateChannelId}>"),
                    interaction
                );
            if (!PermissionChecker.CheckReleasePermissions(releaseChannelId))
                await Response.Info(
                    T("error.missingChannelPermsRelease", lng, $"<#{releaseChannelId}>"),
                    interaction
                );

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
