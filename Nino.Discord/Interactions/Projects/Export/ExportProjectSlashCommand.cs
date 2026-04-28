// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.Json;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Queries.Projects.Export;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Projects;

public partial class ProjectModule
{
    [SlashCommand("export", "Export a project")]
    public async Task<RuntimeResult> ExportAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var isDiscordAdmin = member.GuildPermissions.Administrator;

        var request = await projectResolver
            .HandleAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
            .ThenAsync(pId =>
                exportHandler.HandleAsync(new ExportProjectQuery(pId, requestedBy, isDiscordAdmin))
            );

        if (!request.IsSuccess)
        {
            return await interaction.FailAsync(
                request.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var (projectId, exportData) = request.Value;

        var json = JsonSerializer.Serialize(exportData, JsonOptions);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        await interaction.FollowupWithFileAsync(stream, $"{projectId}.json");

        return ExecutionResult.Success;
    }
}
