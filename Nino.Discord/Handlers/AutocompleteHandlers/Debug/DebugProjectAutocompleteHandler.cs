// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Features.Queries.Projects.ListProjectsForDebug;
using Nino.Discord.Services;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Handlers.AutocompleteHandlers.Debug;

public sealed class DebugProjectAutocompleteHandler : AutocompleteHandler
{
    /// <inheritdoc />
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services
    )
    {
        if (
            context.Interaction is not SocketAutocompleteInteraction interaction
            || interaction.Data.Options.FirstOrDefault(o => o.Name == "group")?.Value
                is not string rawGroupId || !GroupId.TryParse(rawGroupId, out var groupId)
        )
            return AutocompletionResult.FromSuccess();

        var focusedOption = interaction.Data.Current;

        await using var scope = services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<ListProjectsForDebugHandler>();
        var idService = scope.ServiceProvider.GetRequiredService<IInteractionIdentityService>();

        var (userId, _) = await idService.GetUserAndGroupAsync(interaction);
        var result = await handler.HandleAsync(
            new ListProjectsForDebugQuery(groupId, userId)
        );

        if (!result.IsSuccess)
            return AutocompletionResult.FromSuccess();

        return AutocompletionResult.FromSuccess(
            result
                .Value.Where(r =>
                    r.Alias.Value.StartsWith(
                        (string)focusedOption.Value,
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
                .Take(25)
                .Select(r => new AutocompleteResult(r.Alias.Value, r.ProjectId.Value))
        );
    }
}
