// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using NaturalSort.Extension;
using Nino.Core.Features;
using Nino.Core.Features.Queries.Episodes.BlameAll;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Core.Services;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Discord.Services;
using Nino.Domain;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.BlameAll;

public sealed class BlameAllSlashCommand(
    IInteractionIdentityService interactionIdService,
    IStateService stateService,
    GetGenericProjectDataHandler getProjectDataHandler,
    ResolveProjectHandler projectResolver,
    BlameAllHandler blameHandler,
    ILogger<BlameAllSlashCommand> logger
) : InteractionModuleBase<IInteractionContext>
{
    [SlashCommand("blameall", "Check the status of a project")]
    public async Task<RuntimeResult> BlameAllAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        BlameAllFilter filter = BlameAllFilter.All,
        bool includePseudo = false
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var projectResolve = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy, IncludeObservers: true)
        );

        if (!projectResolve.IsSuccess)
        {
            return await interaction.FailAsync(
                projectResolve.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var projectId = projectResolve.Value;

        logger.LogInformation(
            "Generating Blame All for project {ProjectId} for {User}",
            projectId,
            requestedBy
        );

        var command = new BlameAllQuery(
            ProjectId: projectId,
            Filter: filter,
            IncludePseudo: includePseudo,
            Page: null,
            RequestedBy: requestedBy
        );

        var result = await blameHandler
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

        var b = new StringBuilder();

        if (!string.IsNullOrEmpty(bData.Motd))
            b.AppendLine(bData.Motd);

        foreach (
            var episode in bData.Episodes.OrderBy(
                e => e.EpisodeNumber.Value,
                StringComparer.OrdinalIgnoreCase.WithNaturalSort()
            )
        )
        {
            b.Append($"{episode.EpisodeNumber}: ");
            if (episode.Statuses.All(t => !t.IsDone))
            {
                if (episode.AiredAt is not null && episode.AiredAt.Value > DateTimeOffset.UtcNow)
                    b.AppendLine('*' + T("blameAll.notAired", locale) + '*');
                else
                    b.AppendLine('*' + T("blameAll.notStarted", locale) + '*');
                continue;
            }

            foreach (var task in episode.Statuses.OrderBy(t => t.Weight))
            {
                if (task.IsDone)
                    b.Append($"~~{task.Abbreviation}~~ ");
                else
                    b.Append($"**{task.Abbreviation}** ");
            }
            b.AppendLine(); // Adds newline to the end
        }

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("blameAll.title", locale))
            .WithDescription(b.ToString().TrimEnd())
            .WithFooter(T("blameAll.footer", locale, bData.Page + 1, bData.PageCount))
            .Build();

        // Buttons?
        ComponentBuilder? component = null;
        if (bData.PageCount != 1)
        {
            var stateId = await stateService.SaveStateAsync(command with { Page = bData.Page });

            var prevId = $"nino.blameAll.prev:{stateId}";
            var nextId = $"nino.blameAll.next:{stateId}";

            var hasPrev = bData.Page != 0;
            var hasNext = bData.Page < bData.PageCount - 1;

            component = new ComponentBuilder()
                .WithButton("◀", prevId, disabled: !hasPrev)
                .WithButton("▶", nextId, disabled: !hasNext);
        }

        await interaction.FollowupAsync(embed: successEmbed, components: component?.Build());
        return ExecutionResult.Success;
    }
}
