// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.Json;
using Discord.Interactions;
using Nino.Core.Features.Queries.Projects.GetDebugData;
using Nino.Discord.Handlers.AutocompleteHandlers.Debug;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Debug;

public partial class DebugModule
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [SlashCommand("info", "Project information")]
    public async Task<RuntimeResult> GetInfoAsync(
        [Summary("group"), Autocomplete(typeof(DebugGroupAutocompleteHandler))] string rawGroupId,
        [Summary("project"), Autocomplete(typeof(DebugProjectAutocompleteHandler))]
            string rawProjectId
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        if (
            !GroupId.TryParse(rawGroupId, out _)
            || !ProjectId.TryParse(rawProjectId, out var projectId)
        )
            return await interaction.FailAsync(T("nino.debug.invalidId", locale));

        var request = await dataHandler.HandleAsync(new GetDebugDataQuery(projectId));
        if (!request.IsSuccess)
            return await interaction.FailAsync(request.Status, locale);

        var body = new StringBuilder();
        body.AppendLine("```json");
        body.AppendLine(JsonSerializer.Serialize(request.Value, JsonOptions));
        body.AppendLine("```");

        await interaction.FollowupAsync(body.ToString());

        return ExecutionResult.Success;
    }
}
