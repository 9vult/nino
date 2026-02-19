// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Actions.Project.Create;
using Nino.Core.Dtos;
using Nino.Core.Enums;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [SlashCommand("create", "Create a project")]
    public async Task<RuntimeResult> CreateAsync(
        string nickname,
        int anilistId,
        bool isPrivate,
        [ChannelTypes(ChannelType.Text)] IMessageChannel projectChannel,
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

        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);

        // Verify user - Administrator required
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

        var dto = new ProjectCreateDto
        {
            Nickname = nickname,
            AniListId = anilistId,
            IsPrivate = isPrivate,
            ProjectChannelId = projectChannelId,
            UpdateChannelId = updateChannelId,
            ReleaseChannelId = releaseChannelId,
            Title = title,
            Type = type,
            Length = length,
            PosterUri = posterUri,
            FirstEpisode = firstEpisode,
        };

        logger.LogTrace("Project creation requested by {UserId}", userId);

        var response = await createHandler.HandleAsync(
            new ProjectCreateAction(dto, groupId, userId, isDiscordAdmin)
        );

        switch (response.Status)
        {
            case ResultStatus.Success:
                await interaction.FollowupAsync(T("project.creation.success", lng, nickname));
                break;
            case ResultStatus.Unauthorized:
                return await interaction.FailAsync(T("error.permissions", lng));
            case ResultStatus.Conflict:
                return await interaction.FailAsync(T("project.creation.conflict", lng, nickname));
            case ResultStatus.BadRequest:
                return await interaction.FailAsync(T("project.creation.failed", lng));
            case ResultStatus.Error:
                return await interaction.FailAsync(T("project.creation.failed.generic", lng));
        }

        return ExecutionResult.Success;
    }
}
