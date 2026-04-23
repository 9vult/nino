// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Features.Queries.Observers.ListNames;
using Nino.Discord.Services;

namespace Nino.Discord.Handlers.AutocompleteHandlers;

public sealed class ObserverAutocompleteHandler : AutocompleteHandler
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

        var focusedOption = interaction.Data.Current;

        await using var scope = services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<ListObserverNamesHandler>();
        var idService = scope.ServiceProvider.GetRequiredService<IInteractionIdentityService>();
        var client = scope.ServiceProvider.GetRequiredService<DiscordSocketClient>();

        var (userId, groupId) = await idService.GetUserAndGroupAsync(interaction);

        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);
        var isDiscordAdmin = member.GuildPermissions.Administrator;

        var result = await handler.HandleAsync(
            new ListObserverNamesQuery(groupId, userId, isDiscordAdmin)
        );

        if (!result.IsSuccess)
            return AutocompletionResult.FromSuccess();

        return AutocompletionResult.FromSuccess(
            result
                .Value.Where(r =>
                    r.Nickname.Value.StartsWith(
                        (string)focusedOption.Value,
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
                .Take(25)
                .Select(r => new AutocompleteResult(
                    $"{r.Nickname} ({r.GroupName})",
                    r.Id.Value.ToString()
                ))
        );
    }
}
