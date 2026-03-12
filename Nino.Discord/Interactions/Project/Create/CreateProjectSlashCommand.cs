// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Project.Create;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [SlashCommand("create", "Create a project")]
    public async Task<RuntimeResult> CreateAsync(
        [MaxLength(32)] string nickname,
        int anilistId,
        bool isPrivate,
        [ChannelTypes(ChannelType.Text)] IMessageChannel projectChannel,
        [ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel updateChannel,
        [ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel releaseChannel,
        [MaxLength(128)] string? title = null,
        ProjectType? type = null,
        [MinValue(1)] int? length = null,
        [MaxLength(256)] string? posterUrl = null,
        decimal firstEpisode = 1
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        nickname = nickname.Trim();
        title = title?.Trim();
        posterUrl = posterUrl?.Trim();

        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);

        var (userId, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var isDiscordAdmin = member.GuildPermissions.Administrator;

        var projectChannelId = await identityService.GetOrCreateChannelByDiscordIdAsync(
            projectChannel.Id
        );
        var updateChannelId = await identityService.GetOrCreateChannelByDiscordIdAsync(
            updateChannel.Id
        );
        var releaseChannelId = await identityService.GetOrCreateChannelByDiscordIdAsync(
            releaseChannel.Id
        );

        var command = new CreateProjectCommand(groupId, userId, isDiscordAdmin)
        {
            Nickname = nickname,
            AniListId = AniListId.From(anilistId),
            IsPrivate = isPrivate,
            ProjectChannelId = projectChannelId,
            UpdateChannelId = updateChannelId,
            ReleaseChannelId = releaseChannelId,
            Title = title,
            Type = type,
            Length = length,
            PosterUrl = posterUrl,
            FirstEpisode = firstEpisode,
        };

        var response = await createHandler.HandleAsync(command);

        if (!response.IsSuccess)
        {
            Dictionary<string, object> errorParams = new() { ["nickname"] = nickname };
            return await interaction.FailAsync(
                response.Status switch
                {
                    ResultStatus.Unauthorized => "error.permissions",
                    ResultStatus.BadRequest => "project.creation.failed.badRequest",
                    ResultStatus.Conflict => "project.creation.conflict",
                    _ => "project.creation.failed.generic",
                },
                locale,
                errorParams
            );
        }

        var (_, prjNickname, prjTitle, _, prjFirstEpisode, prjType) = response.Value;

        // Check bot permissions in specified channels
        var canUseProjectChannel = botPermissionsService.HasMessagePermissions(projectChannel.Id);
        var canUseUpdateChannel = botPermissionsService.HasMessagePermissions(updateChannel.Id);
        var canUseReleaseChannel = botPermissionsService.HasMessagePermissions(releaseChannel.Id);

        // Build success embed body
        var body = new StringBuilder();
        body.AppendLine(T("project.creation.success", locale, prjNickname));

        if (prjFirstEpisode != 1)
        {
            body.AppendLine();
            body.AppendLine(T("project.creation.firstEpisodeNote", locale, prjFirstEpisode));
        }

        if (isPrivate)
        {
            body.AppendLine();
            body.AppendLine(T("project.creation.publishPrivateProgressInfo", locale));
        }

        if (!canUseProjectChannel || !canUseUpdateChannel || !canUseReleaseChannel)
        {
            body.AppendLine();
            body.AppendLine($"**{T("warning", locale)}**");

            if (!canUseProjectChannel)
                body.AppendLine(T("error.missingMessagePerms", locale, $"<#{projectChannel.Id}>"));
            if (!canUseUpdateChannel)
                body.AppendLine(T("error.missingMessagePerms", locale, $"<#{updateChannel.Id}>"));
            if (!canUseReleaseChannel)
                body.AppendLine(T("error.missingMessagePerms", locale, $"<#{releaseChannel.Id}>"));
        }

        var successEmbed = new EmbedBuilder()
            .WithAuthor($"{prjTitle} ({prjType.ToFriendlyString(locale)})")
            .WithTitle(T("project.creation.title", locale))
            .WithDescription(body.ToString())
            .WithCurrentTimestamp()
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);

        return ExecutionResult.Success;
    }
}
