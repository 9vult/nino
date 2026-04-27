// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Features.Queries.Projects.ListAliases;
using Nino.Core.Services;

namespace Nino.Discord.Handlers.AutocompleteHandlers;

public sealed class ObserverProjectAutocompleteHandler : AutocompleteHandler
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
            || interaction.Data.Options.FirstOrDefault(o => o.Name == "server-id")?.Value
                is not string rawServerId
            || !ulong.TryParse(rawServerId, out var serverId)
        )
            return AutocompletionResult.FromSuccess();

        var focusedOption = interaction.Data.Current;

        await using var scope = services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<ListAliasesHandler>();
        var idService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var groupId = await idService.GetGroupByDiscordIdAsync(serverId);

        if (groupId is null)
            return AutocompletionResult.FromSuccess();

        var userId = await idService.GetOrCreateUserByDiscordIdAsync(
            interaction.User.Id,
            interaction.User.Username
        );

        var result = await handler.HandleAsync(
            new ListAliasesQuery(userId, groupId.Value, true, false)
        );

        if (!result.IsSuccess)
            return AutocompletionResult.FromSuccess();

        return AutocompletionResult.FromSuccess(
            result
                .Value.Where(r =>
                    r.Value.StartsWith(
                        (string)focusedOption.Value,
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
                .Take(25)
                .Select(r => new AutocompleteResult(r.Value, r.Value))
        );
    }
}
