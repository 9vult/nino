// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features;
using Nino.Core.Features.Commands.TemplateStaff;
using Nino.Core.Features.Commands.TemplateStaff.Edit;
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
    [SlashCommand("edit", "Edit a template staff")]
    public async Task<RuntimeResult> EditAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Abbreviation), Autocomplete(typeof(TemplateStaffAutocompleteHandler))]
            Abbreviation abbreviation,
        TemplateStaffApplicator applyTo,
        [MaxLength(Length.Abbreviation)] Abbreviation? newAbbreviation = null,
        [MaxLength(Length.RoleName)] string? fullName = null,
        SocketUser? assignee = null,
        decimal? weight = null,
        bool? isPseudo = null
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        fullName = fullName?.Trim();

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

        var assigneeId = assignee is null
            ? null
            : (UserId?)
                await identityService.GetOrCreateUserByDiscordIdAsync(
                    assignee.Id,
                    assignee.Username
                );

        var command = new EditTemplateStaffCommand(
            ProjectId: projectId,
            TemplateStaffId: staffId,
            RequestedBy: requestedBy,
            Applicator: applyTo,
            AssigneeId: assigneeId,
            NewAbbreviation: newAbbreviation,
            Name: fullName,
            Weight: weight,
            IsPseudo: isPseudo
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

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(
                T(
                    "templateStaff.edit.success",
                    locale,
                    abbreviation,
                    applyTo.ToFriendlyString(locale)
                )
            )
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
