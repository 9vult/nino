// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features;
using Nino.Core.Features.Commands.TemplateStaff;
using Nino.Core.Features.Commands.TemplateStaff.Add;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Discord.Interactions.TemplateStaff;

public partial class TemplateStaffModule
{
    [SlashCommand("add", "Add a template staff to a project")]
    public async Task<RuntimeResult> AddAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Abbreviation)] Abbreviation abbreviation,
        [MaxLength(Length.RoleName)] string fullName,
        SocketUser assignee,
        bool isPseudo,
        TemplateStaffApplicator applyTo
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        fullName = fullName.Trim();

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

        var assigneeId = await identityService.GetOrCreateUserByDiscordIdAsync(
            assignee.Id,
            assignee.Username
        );

        var command = new AddTemplateStaffCommand(
            ProjectId: projectId,
            Applicator: applyTo,
            RequestedBy: requestedBy,
            AssigneeId: assigneeId,
            Abbreviation: abbreviation,
            Name: fullName,
            IsPseudo: isPseudo
        );

        var result = await addHandler
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
        var staffMention = $"<@{assignee.Id}>";
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(
                T(
                    "templateStaff.creation.success",
                    locale,
                    staffMention,
                    fullName,
                    applyTo.ToFriendlyString(locale)
                )
            )
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
