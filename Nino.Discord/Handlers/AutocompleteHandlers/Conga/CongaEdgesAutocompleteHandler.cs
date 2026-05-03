// SPDX-License-Identifier: MPL-2.0

using System.Text.Json;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Features;
using Nino.Core.Features.Queries.Projects.Conga.ListEdges;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Services;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Handlers.AutocompleteHandlers.Conga;

public sealed class CongaEdgesAutocompleteHandler : AutocompleteHandler
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
            || interaction.Data.Options.FirstOrDefault(o => o.Name == "alias")?.Value
                is not string alias
        )
            return AutocompletionResult.FromSuccess();

        var focusedOption = interaction.Data.Current;
        var includeObservers = interaction.Data.CommandName is "blame" or "blameall";

        await using var scope = services.CreateAsyncScope();
        var idService = scope.ServiceProvider.GetRequiredService<IInteractionIdentityService>();
        var projectResolver = scope.ServiceProvider.GetRequiredService<ResolveProjectHandler>();
        var handler = scope.ServiceProvider.GetRequiredService<ListCongaEdgesHandler>();

        var (userId, groupId) = await idService.GetUserAndGroupAsync(interaction);

        var result = await projectResolver
            .HandleAsync(
                new ResolveProjectQuery(Alias.From(alias), groupId, userId, includeObservers)
            )
            .BindAsync(projectId => handler.HandleAsync(new ListCongaEdgesQuery(projectId)));

        if (!result.IsSuccess)
            return AutocompletionResult.FromSuccess();

        return AutocompletionResult.FromSuccess(
            result
                .Value.Where(r =>
                    r.From.Value.StartsWith(
                        (string)focusedOption.Value,
                        StringComparison.InvariantCultureIgnoreCase
                    )
                    || r.To.Value.StartsWith(
                        (string)focusedOption.Value,
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
                .Take(25)
                .Select(r => new AutocompleteResult(
                    $"{r.From} → {r.To}",
                    JsonSerializer.Serialize(r)
                ))
        );
    }
}
