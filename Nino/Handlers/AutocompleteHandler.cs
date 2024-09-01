using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Utilities;

namespace Nino.Handlers
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    /// <summary>
    /// Autocompletion for project names/aliases
    /// </summary>
    public class ProjectAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var commandName = interaction.Data.CommandName;
            var focusedOption = interaction.Data.Current;
            var guildId = interaction.GuildId ?? 0;
            var userId = interaction.User.Id;

            List<AutocompleteResult> choices = [];
            var alias = ((string)focusedOption.Value).Trim();
            if (alias != null)
            {
                choices.AddRange(Getters.GetFilteredAliases(guildId, userId, (string)focusedOption.Value)
                    .Select(m => new AutocompleteResult(m, m))
                );
            }
            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }

    /// <summary>
    /// Autocompletion for episode numbers
    /// </summary>
    public class EpisodeAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var commandName = interaction.Data.CommandName;
            var focusedOption = interaction.Data.Current;
            var guildId = interaction.GuildId ?? 0;
            var userId = interaction.User.Id;

            List<AutocompleteResult> choices = [];
            var alias = ((string?)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")?.Value)?.Trim();
            if (alias != null)
            {
                var cachedProject = Utils.ResolveAlias(alias, interaction);
                if (cachedProject != null)
                {
                    choices.AddRange(Cache.GetEpisodes(cachedProject.Id)
                        .Where(e => e.Number.ToString().StartsWith((string)focusedOption.Value))
                        .Select(e => new AutocompleteResult(e.Number.ToString(), e.Number))
                    );
                }
            }
            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }

    /// <summary>
    /// Autocompletion for task abbreviations
    /// </summary>
    public class AbbreviationAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var commandName = interaction.Data.CommandName;
            var focusedOption = interaction.Data.Current;
            var guildId = interaction.GuildId ?? 0;
            var userId = interaction.User.Id;

            List<AutocompleteResult> choices = [];
            var alias = ((string?)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")?.Value)?.Trim();
            var episodeInput = (double?)interaction.Data.Options.FirstOrDefault(o => o.Name == "episode")?.Value;
            if (alias != null)
            {
                var cachedProject = Utils.ResolveAlias(alias, interaction);
                if (cachedProject != null)
                {
                    if (episodeInput != null)
                    {
                        var episodeNumber = Convert.ToDecimal(episodeInput);
                        var cachedEpisode = Cache.GetEpisodes(cachedProject.Id).FirstOrDefault(e => e.Number == episodeNumber);
                        if (cachedEpisode == null)
                        {
                            // Return list of key staff
                            choices.AddRange(cachedProject.KeyStaff
                                .Where(ks => ks.Role.Abbreviation.StartsWith((string)focusedOption.Value))
                                .Select(t => new AutocompleteResult(t.Role.Abbreviation, t.Role.Abbreviation))
                            );
                        }
                        else
                        {
                            // Return list of episode tasks
                            choices.AddRange(cachedEpisode.Tasks
                                .Where(t => t.Abbreviation.StartsWith((string)focusedOption.Value))
                                .Select(t => new AutocompleteResult(t.Abbreviation, t.Abbreviation))
                            );
                        }
                    }
                }
            }
            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }

    /// <summary>
    /// Autocompletion for Key Staff
    /// </summary>
    public class KeyStaffAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var commandName = interaction.Data.CommandName;
            var focusedOption = interaction.Data.Current;
            var guildId = interaction.GuildId ?? 0;
            var userId = interaction.User.Id;

            List<AutocompleteResult> choices = [];
            var alias = ((string?)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")?.Value)?.Trim();
            if (alias != null)
            {
                var cachedProject = Utils.ResolveAlias(alias, interaction);
                if (cachedProject != null)
                {
                    // Return list of key staff
                    choices.AddRange(cachedProject.KeyStaff
                        .Where(ks => ks.Role.Abbreviation.StartsWith((string)focusedOption.Value))
                        .Select(t => new AutocompleteResult(t.Role.Abbreviation, t.Role.Abbreviation))
                    );
                }
            }
            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
