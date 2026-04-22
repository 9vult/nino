// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Core.Features.Queries.Projects.Roster;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;
using RuntimeResult = Discord.Interactions.RuntimeResult;

namespace Nino.Discord.Interactions.Projects;

public partial class ProjectModule
{
    [SlashCommand("roster", "Show who's working on a project")]
    public async Task<RuntimeResult> RosterAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

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

        var command = new ProjectRosterQuery(ProjectId: projectId, RequestedBy: requestedBy);

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
                new FailureContext { Alias = alias }
            );
        }

        var bData = result.Value.Item1;
        var pData = result.Value.Item2;

        // Cache for non-Discord names
        Dictionary<UserId, string> nameCache = [];

        var b = new StringBuilder();
        foreach (var task in bData.OrderBy(t => t.Weight))
        {
            b.Append($"{task.Abbreviation}: ");

            var assigneeParts = new List<string>();
            foreach (var assignee in task.Assignees)
            {
                string assigneeName;
                if (assignee.Assignee.DiscordId.HasValue)
                    assigneeName = $"<@{assignee.Assignee.DiscordId.Value}>";
                else if (nameCache.TryGetValue(assignee.Assignee.Id, out var name))
                    assigneeName = name;
                else
                {
                    name = await identityService.GetUserNameAsync(assignee.Assignee.Id);
                    nameCache[assignee.Assignee.Id] = name ?? "?";
                    assigneeName = name ?? "?";
                }

                var segments = assignee.Ranges.Select(range =>
                    range.Item1 == range.Item2 ? range.Item1.Value : $"{range.Item1}-{range.Item2}"
                );
                assigneeParts.Add($"{assigneeName} ({string.Join(", ", segments)})");
            }

            b.AppendLine(string.Join(", ", assigneeParts));
        }

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("project.roster.title", locale))
            .WithDescription(b.ToString().TrimEnd())
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
