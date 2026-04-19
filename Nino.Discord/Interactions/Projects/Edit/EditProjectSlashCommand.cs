// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Edit;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Projects;

public partial class ProjectModule
{
    [SlashCommand("edit", "Edit a project")]
    public async Task<RuntimeResult> EditAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Alias)] Alias? nickname = null,
        [MaxLength(Length.Title)] string? title = null,
        [MaxLength(Length.PosterUrl)] string? posterUrl = null,
        [MaxLength(Length.Motd)] string? motd = null,
        [MinValue(0)] int? anilistId = null,
        int? anilistOffset = null,
        bool? isPrivate = null,
        IMessageChannel? projectChannel = null,
        IMessageChannel? updateChannel = null,
        IMessageChannel? releaseChannel = null
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        title = title?.Trim();
        motd = motd?.Trim();
        posterUrl = posterUrl?.Trim();

        // Special empty key for Discord
        if (motd == "-")
            motd = string.Empty;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolve = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy)
        );

        if (!resolve.IsSuccess)
        {
            return await interaction.FailAsync(
                resolve.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var projectChannelId = projectChannel is null
            ? null
            : (ChannelId?)
                await identityService.GetOrCreateChannelByDiscordIdAsync(projectChannel.Id);
        var updateChannelId = updateChannel is null
            ? null
            : (ChannelId?)
                await identityService.GetOrCreateChannelByDiscordIdAsync(updateChannel.Id);
        var releaseChannelId = releaseChannel is null
            ? null
            : (ChannelId?)
                await identityService.GetOrCreateChannelByDiscordIdAsync(releaseChannel.Id);

        var projectId = resolve.Value;

        var command = new EditProjectCommand(
            ProjectId: projectId,
            RequestedBy: requestedBy,
            Nickname: nickname,
            Title: title,
            PosterUrl: posterUrl,
            Motd: motd,
            AniListId: anilistId is not null ? AniListId.From(anilistId.Value) : null,
            AniListOffset: anilistOffset,
            IsPrivate: isPrivate,
            ProjectChannelId: projectChannelId,
            UpdateChannelId: updateChannelId,
            ReleaseChannelId: releaseChannelId
        );

        var result = await editHandler
            .HandleAsync(command)
            .BindAsync(() =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
            );

        if (!result.IsSuccess)
        {
            return await interaction.FailAsync(
                result.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var pData = result.Value;

        // Check bot permissions in specified channels
        var canUseProjectChannel =
            projectChannel is null
            || botPermissionsService.HasMessagePermissions(projectChannel.Id);
        var canUseUpdateChannel =
            updateChannel is null || botPermissionsService.HasMessagePermissions(updateChannel.Id);
        var canUseReleaseChannel =
            releaseChannel is null
            || botPermissionsService.HasMessagePermissions(releaseChannel.Id);

        // Build success embed body
        var body = new StringBuilder();
        body.AppendLine(T("project.edit.success", locale));

        // Inform user about the empty key
        if (!string.IsNullOrEmpty(motd))
        {
            body.AppendLine();
            body.AppendLine(T("project.edit.clearable", locale));
        }

        if (!canUseProjectChannel || !canUseUpdateChannel || !canUseReleaseChannel)
        {
            body.AppendLine();
            body.AppendLine($"**{T("warning", locale)}**");

            if (!canUseProjectChannel)
                body.AppendLine(T("error.missingMessagePerms", locale, $"<#{projectChannel!.Id}>"));
            if (!canUseUpdateChannel)
                body.AppendLine(T("error.missingMessagePerms", locale, $"<#{updateChannel!.Id}>"));
            if (!canUseReleaseChannel)
                body.AppendLine(T("error.missingMessagePerms", locale, $"<#{releaseChannel!.Id}>"));
        }

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(body.ToString())
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
