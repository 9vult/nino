// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Release.Episode;
using Nino.Core.Features.Queries.Episodes.ValidateRelease;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Release;

public partial class ReleaseModule
{
    [SlashCommand("episode", "Release an episode")]
    public async Task<RuntimeResult> ReleaseEpisodeAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Number)] Number episodeNumber,
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

        var command = new ReleaseEpisodeCommand(
            projectId,
            requestedBy,
            episodeNumber,
            urls,
            primaryRoleId,
            secondaryRoleId,
            tertiaryRoleId,
            commentary
        );

        var validate = await validateReleaseHandler
            .HandleAsync(new ValidateReleaseQuery(projectId, episodeNumber, episodeNumber))
            .ThenAsync(_ =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
            );
        if (!validate.IsSuccess)
        {
            return await interaction.FailAsync(
                validate.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var (episodeValidation, pData) = validate.Value;
        if (episodeValidation)
        {
            var result = await releaseEpisodeHandler.HandleAsync(command);
            if (!result.IsSuccess)
            {
                return await interaction.FailAsync(
                    result.Status,
                    locale,
                    new FailureContext { Alias = alias }
                );
            }

            var successEmbed = new EmbedBuilder()
                .WithProjectInfo(pData, locale)
                .WithTitle(T("release.title", locale))
                .WithDescription(T("release.episode.success", locale, episodeNumber))
                .Build();

            await interaction.FollowupAsync(embed: successEmbed);
            return ExecutionResult.Success;
        }

        // Prompt user to confirm incomplete episode release
        var questionEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("release.incomplete.title", locale))
            .WithDescription(T("release.incomplete.episode", locale, episodeNumber))
            .Build();

        var stateId = await stateService.SaveStateAsync(command);
        var cancelId = $"nino.release.cancel:{stateId}";
        var confirmId = $"nino.release.episode.confirm:{stateId}";

        var component = new ComponentBuilder()
            .WithButton(T("button.cancel", locale), cancelId, ButtonStyle.Danger)
            .WithButton(T("button.releaseIt", locale), confirmId, ButtonStyle.Secondary)
            .Build();

        await interaction.FollowupAsync(embed: questionEmbed, components: component);
        return ExecutionResult.Success;
    }
}
