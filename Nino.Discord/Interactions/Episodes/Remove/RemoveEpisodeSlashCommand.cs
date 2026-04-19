// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Episodes.Remove;
using Nino.Core.Features.Queries.Episodes.Resolve;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;
using RuntimeResult = Discord.Interactions.RuntimeResult;

namespace Nino.Discord.Interactions.Episodes;

public partial class EpisodesModule
{
    [SlashCommand("remove", "Remove episodes from a project")]
    public async Task<RuntimeResult> RemoveAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))] Number first,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number? last = null
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolve = await projectResolver
            .HandleAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
            .ThenAsync(prjId => episodeResolver.HandleAsync(new ResolveEpisodeQuery(prjId, first)));

        if (!resolve.IsSuccess)
        {
            return await interaction.FailAsync(
                resolve.Status,
                locale,
                new FailureContext { Alias = alias, Episode = first }
            );
        }

        var (projectId, firstId) = resolve.Value;
        var lastId = firstId;

        // Resolve the last episode if needed
        if (last is not null)
        {
            var lastResolve = await episodeResolver.HandleAsync(
                new ResolveEpisodeQuery(projectId, last.Value)
            );
            if (!resolve.IsSuccess)
            {
                return await interaction.FailAsync(
                    resolve.Status,
                    locale,
                    new FailureContext { Alias = alias, Episode = last.Value }
                );
            }
            lastId = lastResolve.Value;
        }

        var command = new RemoveEpisodeCommand(
            ProjectId: projectId,
            FirstEpisodeId: firstId,
            LastEpisodeId: lastId,
            RequestedBy: requestedBy
        );

        var result = await removeHandler
            .HandleAsync(command)
            .ThenAsync(_ =>
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

        var removedEpisodeCount = result.Value.Item1.RemovedEpisodeCount;
        var pData = result.Value.Item2;

        var responseArgs = new Dictionary<string, object> { ["number"] = removedEpisodeCount };

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(T("episode.delete.success", locale, responseArgs))
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
