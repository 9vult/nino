// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Features.Queries.Groups.ListGroupsForDebug;
using Nino.Discord.Services;

namespace Nino.Discord.Handlers.AutocompleteHandlers.Debug;

public sealed class DebugGroupAutocompleteHandler : AutocompleteHandler
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
        )
            return AutocompletionResult.FromSuccess();

        var focusedOption = interaction.Data.Current;

        await using var scope = services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<ListGroupsForDebugHandler>();
        var idService = scope.ServiceProvider.GetRequiredService<IInteractionIdentityService>();

        var (userId, _) = await idService.GetUserAndGroupAsync(interaction);
        var result = await handler.HandleAsync(
            new ListGroupsForDebugQuery(userId)
        );

        if (!result.IsSuccess)
            return AutocompletionResult.FromSuccess();

        return AutocompletionResult.FromSuccess(
            result
                .Value.Where(r =>
                    r.Name.StartsWith(
                        (string)focusedOption.Value,
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
                .Take(25)
                .Select(r => new AutocompleteResult(r.Name, r.GroupId.Value))
        );
    }
}
