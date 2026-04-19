// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using NaturalSort.Extension;
using Nino.Core.Features;
using Nino.Core.Features.Commands.TemplateStaff;
using Nino.Core.Features.Commands.TemplateStaff.Remove;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Core.Features.Queries.TemplateStaff.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Discord.Interactions.TemplateStaff;

public partial class TemplateStaffModule
{
    [SlashCommand("remove", "Remove a template staff from a project")]
    public async Task<RuntimeResult> RemoveAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Abbreviation), Autocomplete(typeof(TemplateStaffAutocompleteHandler))]
            Abbreviation abbreviation,
        TemplateStaffApplicator applyTo
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolve = await projectResolver
            .HandleAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
            .ThenAsync(prjId =>
                staffResolver.HandleAsync(new ResolveTemplateStaffQuery(prjId, abbreviation))
            );

        if (!resolve.IsSuccess)
        {
            return await interaction.FailAsync(
                resolve.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var (projectId, staffId) = resolve.Value;

        var command = new RemoveTemplateStaffCommand(
            ProjectId: projectId,
            TemplateStaffId: staffId,
            Applicator: applyTo,
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

        var completedEpisodes = result.Value.Item1.CompletedEpisodes;
        var pData = result.Value.Item2;

        var body = new StringBuilder();
        body.AppendLine(
            T(
                "templateStaff.delete.success",
                locale,
                abbreviation,
                applyTo.ToFriendlyString(locale)
            )
        );

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
