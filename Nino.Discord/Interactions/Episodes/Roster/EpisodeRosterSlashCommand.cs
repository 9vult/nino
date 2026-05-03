// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Queries.Episodes.Resolve;
using Nino.Core.Features.Queries.Episodes.Roster;
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
    [SlashCommand("roster", "Show who's working on an episode")]
    public async Task<RuntimeResult> RosterAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number episodeNumber
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolve = await projectResolver
            .HandleAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
            .ThenAsync(pId =>
                episodeResolver.HandleAsync(new ResolveEpisodeQuery(pId, episodeNumber))
            );

        if (!resolve.IsSuccess)
        {
            return await interaction.FailAsync(
                resolve.Status,
                locale,
                new FailureContext { Alias = alias, Episode = episodeNumber }
            );
        }

        var (projectId, episodeId) = resolve.Value;

        var command = new EpisodeRosterQuery(
            ProjectId: projectId,
            EpisodeId: episodeId,
            RequestedBy: requestedBy
        );

        var result = await rosterHandler
            .HandleAsync(command)
            .ThenAsync(_ =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
            );

        if (!result.IsSuccess)
        {
            return await interaction.FailAsync(
                result.Status,
                locale,
                new FailureContext { Alias = alias, Episode = episodeNumber }
            );
        }

        var bData = result.Value.Item1;
        var pData = result.Value.Item2;

        var b = new StringBuilder();

        foreach (var task in bData.Statuses.OrderBy(t => t.Weight))
        {
            b.Append($"{task.Abbreviation}");
            if (task.IsPseudo)
                b.Append(@"\*");
            b.Append(": ");

            if (task.Assignee.DiscordId.HasValue)
                b.Append($"<@{task.Assignee.DiscordId.Value}>");
            else
                b.Append(await identityService.GetUserNameAsync(task.Assignee.Id));
            b.AppendLine(); // add newline
        }

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(
                !bData.IsSingleEpisodeMovie
                    ? T("episode.title", locale, bData.EpisodeNumber)
                    : string.Empty
            )
            .WithDescription(b.ToString().TrimEnd())
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
