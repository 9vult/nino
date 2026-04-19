// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Features.Queries.Projects.ListAliases;
using Nino.Discord.Services;

namespace Nino.Discord.Handlers.AutocompleteHandlers;

public sealed class ProjectAutocompleteHandler : AutocompleteHandler
{
    /// <inheritdoc />
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services
    )
    {
        if (context.Interaction is not SocketAutocompleteInteraction interaction)
            return AutocompletionResult.FromSuccess();

        var includeObservers = interaction.Data.CommandName is "blame" or "blameall";
        var includeArchived =
            includeObservers || interaction.Data.Options.FirstOrDefault()?.Name == "delete";

        await using var scope = services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<ListAliasesHandler>();
        var idService = scope.ServiceProvider.GetRequiredService<IInteractionIdentityService>();

        var (userId, groupId) = await idService.GetUserAndGroupAsync(interaction);
        var result = await handler.HandleAsync(
            new ListAliasesQuery(userId, groupId, includeObservers, includeArchived)
        );

        if (!result.IsSuccess)
            return AutocompletionResult.FromSuccess();

        return AutocompletionResult.FromSuccess(
            result.Value.Take(25).Select(r => new AutocompleteResult(r.Value, r))
        );
    }
}
