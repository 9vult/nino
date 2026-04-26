// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using NaturalSort.Extension;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Tasks.BulkMark;
using Nino.Core.Features.Queries.Episodes.Resolve;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Discord.Services;
using Nino.Domain;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Bulk;

public class BulkSlashCommand(
    IInteractionIdentityService interactionIdService,
    GetGenericProjectDataHandler getProjectDataHandler,
    ResolveProjectHandler projectResolver,
    ResolveEpisodeHandler episodeResolver,
    BulkMarkTasksHandler bulkHandler
) : InteractionModuleBase<IInteractionContext>
{
    [SlashCommand("bulk", "Do many episodes' tasks at once")]
    public async Task<RuntimeResult> HandleBulkAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Abbreviation), Autocomplete(typeof(ProjectTaskAutocompleteHandler))]
            Abbreviation abbreviation,
        ProgressType action,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number firstEpisode,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number lastEpisode
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolve = await projectResolver
            .HandleAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
            .ThenAsync(pId =>
                episodeResolver.HandleAsync(new ResolveEpisodeQuery(pId, firstEpisode))
            );

        if (!resolve.IsSuccess)
        {
            return await interaction.FailAsync(
                resolve.Status,
                locale,
                new FailureContext { Alias = alias, Episode = firstEpisode }
            );
        }
        var (projectId, firstEpisodeId) = resolve.Value;

        var lastResolve = await episodeResolver.HandleAsync(
            new ResolveEpisodeQuery(projectId, lastEpisode)
        );
        if (!resolve.IsSuccess)
        {
            return await interaction.FailAsync(
                resolve.Status,
                locale,
                new FailureContext { Alias = alias, Episode = lastEpisode }
            );
        }
        var lastEpisodeId = lastResolve.Value;

        var result = await bulkHandler
            .HandleAsync(
                new BulkMarkTasksCommand(
                    projectId,
                    firstEpisodeId,
                    lastEpisodeId,
                    abbreviation,
                    action,
                    requestedBy
                )
            )
            .ThenAsync(_ =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
            );

        if (!result.IsSuccess)
            return await interaction.FailAsync(result.Status, locale, new FailureContext());

        var completedEpisodes = result.Value.Item1.CompletedEpisodes;
        var pData = result.Value.Item2;

        var body = new StringBuilder();
        body.AppendLine(T("bulk.success", locale, abbreviation, firstEpisode, lastEpisode));

        if (completedEpisodes.Count > 0)
        {
            var args = new Dictionary<string, object> { ["number"] = completedEpisodes.Count };
            var list = completedEpisodes
                .OrderBy(e => e.Item2.Value, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
                .Select(e => e.Item2)
                .ToList();

            body.AppendLine();
            body.AppendLine(T("task.deleted.completedEpisodes", locale, args));
            body.AppendLine(string.Join(", ", list));
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle($"🚧 {T("bulk.response.title", locale)}")
            .WithDescription(body.ToString())
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
