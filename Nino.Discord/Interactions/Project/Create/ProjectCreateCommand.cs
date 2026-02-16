// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Actions;
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
        if (
            !member.GuildPermissions.Administrator // Discord admin
            && !await verificationService.VerifyGroupPermissionsAsync(
                groupId,
                userId,
                PermissionsLevel.Administrator
            )
        )
            return await interaction.FailAsync("");

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
            UpdateChannelId = updateChannelId,
            ReleaseChannelId = releaseChannelId,
            Title = title,
            Type = type,
            Length = length,
            PosterUri = posterUri,
            FirstEpisode = firstEpisode,
        };

        var response = await createHandler.HandleAsync(
            new ProjectCreateAction(dto, groupId, userId)
        );

        switch (response.Status)
        {
            case ResultStatus.Success:
                await interaction.FollowupAsync("Created project");
                break;
            case ResultStatus.Conflict:
                await interaction.FollowupAsync("Conflicting project");
                break;
            case ResultStatus.Error:
                break;
        }

        return ExecutionResult.Success;
    }
}
