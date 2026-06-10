// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Commands.Projects.Release.Custom;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Release;

public partial class ReleaseModule
{
    [SlashCommand("custom", "Release something special!")]
    public async Task<RuntimeResult> ReleaseCustomAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        string url,
        string? label = null,
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
        label = label?.Trim();

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

        var command = new ReleaseCustomCommand(
            projectId,
            requestedBy,
            label,
            urls,
            primaryRoleId,
            secondaryRoleId,
            tertiaryRoleId,
            commentary
        );

        var result = await getProjectDataHandler.HandleAsync(
            new GetGenericProjectDataQuery(projectId)
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

        var body = new StringBuilder();
        body.AppendLine(T("release.custom.confirm", locale));
        body.AppendLine();
        body.Append("> ");
        body.AppendLine(
            !string.IsNullOrEmpty(label)
                ? T("release.broadcast.custom.labeled", locale, pData.ProjectTitle, label)
                : T("release.broadcast.custom.notLabeled", locale, pData.ProjectTitle)
        );

        // Prompt user to confirm release details
        var questionEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("release.incomplete.title", locale))
            .WithDescription(body.ToString())
            .Build();

        var stateId = await stateService.SaveStateAsync(command);
        var cancelId = $"nino.release.cancel:{stateId}";
        var confirmId = $"nino.release.custom.confirm:{stateId}";

        var component = new ComponentBuilder()
            .WithButton(T("button.cancel", locale), cancelId, ButtonStyle.Danger)
            .WithButton(T("button.releaseIt", locale), confirmId, ButtonStyle.Secondary)
            .Build();

        await interaction.FollowupAsync(embed: questionEmbed, components: component);
        return ExecutionResult.Success;
    }
}
