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
            string nickname,
            int anilistId,
            bool isPrivate,
            [ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel updateChannel,
            [ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel releaseChannel,
            string? title = null,
            ProjectType? type = null,
            [MinValue(1)] int? length = null,
            string? posterUri = null,
            decimal firstEpisode = 1
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            var guildId = interaction.GuildId ?? 0;
            var guild = Nino.Client.GetGuild(guildId);
            var member = guild.GetUser(interaction.User.Id);
            if (!Utils.VerifyAdministrator(db, member, guild))
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

            var defaultFieldNames = string.Join(
                ", ",
                new[] { nameof(title), nameof(length), nameof(type), nameof(posterUri) }
                    .Zip(new object?[] { title, length, type, posterUri })
                    .Where(p => p.Second is null)
                    .Select(p => p.First)
            );

            if (defaultFieldNames.Length > 0)
                Log.Info(
                    $"AniList will be used in the construction of project '{nickname}' for the following fields: {defaultFieldNames}"
                );

            var apiResponse = await AniListService.Get(anilistId);
            if (apiResponse is not null && apiResponse.Error is null)
            {
                title ??= apiResponse.Title;
                length ??= apiResponse.EpisodeCount;
                type ??= apiResponse.Type;

                if (title is null || length is null || length < 1)
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
                posterUri = apiResponse.CoverImage ?? AniListService.FallbackPosterUri;
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
                AniListId = anilistId,
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

            // Add project and episodes to database
            await db.Projects.AddAsync(projectData);
            await db.Episodes.AddRangeAsync(episodes);

            // Create configuration if the guild doesn't have one yet
            if (db.GetConfig(guildId) == null)
            {
                Log.Info($"Creating default configuration for guild {guildId}");
                await db.Configurations.AddAsync(Configuration.CreateDefault(guildId));
            }

            Log.Info(
                $"Creating project {projectData} for M[{ownerId} (@{member.Username})] with {episodes.Count} episodes, starting with episode {firstEpisode}"
            );

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

            // Inform about private project behavior
            if (projectData.IsPrivate)
                await Response.Info(T("info.publishPrivateProgress", lng), interaction);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
