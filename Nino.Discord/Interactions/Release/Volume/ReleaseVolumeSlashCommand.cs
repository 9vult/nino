// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Release.Volume;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Release;

public partial class ReleaseModule
{
    [SlashCommand("volume", "Release a volume")]
    public async Task<RuntimeResult> ReleaseVolumeAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Number)] Number volume,
        string url,
        SocketRole? primaryRole = null,
        SocketRole? secondaryRole = null,
        SocketRole? tertiaryRole = null,
        [MaxLength(Length.Commentary)] string? commentary = null
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        commentary = commentary?.Trim();

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

        var projectId = resolve.Value;

        var primaryRoleId = primaryRole is not null
            ? (RoleId?)await identityService.GetOrCreateRoleByDiscordIdAsync(primaryRole.Id)
            : null;
        var secondaryRoleId = secondaryRole is not null
            ? (RoleId?)await identityService.GetOrCreateRoleByDiscordIdAsync(secondaryRole.Id)
            : null;
        var tertiaryRoleId = tertiaryRole is not null
            ? (RoleId?)await identityService.GetOrCreateRoleByDiscordIdAsync(tertiaryRole.Id)
            : null;

        var urls = url.Split('|', StringSplitOptions.TrimEntries).ToList();

        var command = new ReleaseVolumeCommand(
            projectId,
            requestedBy,
            volume,
            urls,
            primaryRoleId,
            secondaryRoleId,
            tertiaryRoleId,
            commentary
        );

        var result = await releaseVolumeHandler
            .HandleAsync(command)
            .BindAsync(() =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(command.ProjectId))
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
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("release.title", locale))
            .WithDescription(T("release.volume.success", locale, volume))
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
