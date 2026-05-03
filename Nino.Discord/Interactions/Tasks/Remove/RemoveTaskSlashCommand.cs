// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using NaturalSort.Extension;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Tasks.Remove;
using Nino.Core.Features.Queries.Episodes.Resolve;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Tasks;

public partial class TaskModule
{
    [SlashCommand("remove", "Remove a task from an episode")]
    public async Task<RuntimeResult> RemoveAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Abbreviation), Autocomplete(typeof(ProjectTaskAutocompleteHandler))]
            Abbreviation abbreviation,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number firstEpisode,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number? lastEpisode = null
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolve = await projectResolver
            .HandleAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
            .ThenAsync(prjId =>
                episodeResolver.HandleAsync(new ResolveEpisodeQuery(prjId, firstEpisode))
            );

        if (!resolve.IsSuccess)
        {
            return await interaction.FailAsync(
                resolve.Status,
                locale,
                new FailureContext { Alias = alias, Episode = firstEpisode }
            );
        }

        var (projectId, firstId) = resolve.Value;
        var lastId = firstId;

        // Resolve the last episode if needed
        if (lastEpisode is not null)
        {
            var lastResolve = await episodeResolver.HandleAsync(
                new ResolveEpisodeQuery(projectId, lastEpisode.Value)
            );
            if (!resolve.IsSuccess)
            {
                return await interaction.FailAsync(
                    resolve.Status,
                    locale,
                    new FailureContext { Alias = alias, Episode = lastEpisode.Value }
                );
            }
            lastId = lastResolve.Value;
        }

        var command = new RemoveTaskCommand(
            ProjectId: projectId,
            FirstEpisodeId: firstId,
            LastEpisodeId: lastId,
            RequestedBy: requestedBy,
            Abbreviation: abbreviation
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

        var completedEpisodes = result.Value.Item1.CompletedEpisodes;
        var pData = result.Value.Item2;
        var locKey = lastEpisode is null ? "task.delete.success" : "task.delete.success.range";

        var body = new StringBuilder();
        body.AppendLine(T(locKey, locale, abbreviation, firstEpisode, lastEpisode));

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
