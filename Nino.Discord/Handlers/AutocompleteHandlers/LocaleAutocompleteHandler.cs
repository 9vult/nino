// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Localization;

namespace Nino.Discord.Handlers.AutocompleteHandlers;

public sealed class LocaleAutocompleteHandler : AutocompleteHandler
{
    private static readonly Dictionary<string, string> Options = Enum.GetValues<Locale>()
        .ToDictionary(locale => locale.ToFriendlyString(), locale => $"{(int)locale}");

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

        List<AutocompleteResult> choices = [];
        var query = ((string)focusedOption.Value).Trim();
        choices.AddRange(
            Options
                .Where(o => o.Key.StartsWith(query, StringComparison.InvariantCultureIgnoreCase))
                .Select(o => new AutocompleteResult(o.Key, o.Value))
        );
        return AutocompletionResult.FromSuccess(choices.Take(25));
    }
}
