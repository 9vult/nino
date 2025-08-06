using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Localizer;
using Microsoft.EntityFrameworkCore;
using Nino.Records;
using Nino.Utilities;

namespace Nino.Handlers
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    /// <summary>
    /// Autocompletion for project names/aliases
    /// </summary>
    public class ProjectAutocompleteHandler(DataContext db) : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var commandName = interaction.Data.CommandName;
            var focusedOption = interaction.Data.Current;
            var guildId = interaction.GuildId ?? 0;
            var userId = interaction.User.Id;

            var includeObservers = commandName is "blame" or "blameall";
            var includeArchived =
                includeObservers || interaction.Data.Options.FirstOrDefault()?.Name == "delete";

            List<AutocompleteResult> choices = [];
            var query = ((string)focusedOption.Value).Trim();

            var guildAdmins = db.GetConfig(guildId)?.Administrators ?? [];

            List<Project> projects = [];

            // Local guild projects
            projects.AddRange(db.Projects.Where(p => p.GuildId == guildId));

            // Observing guild projects
            if (includeObservers)
                projects.AddRange(
                    db.Observers.Include(o => o.Project)
                        .Where(o => o.GuildId == guildId)
                        .Select(o => o.Project)
                );

            if (!includeArchived)
                projects = projects.Where(p => !p.IsArchived).ToList();

            // Filter (Not including Additional Staff here because that'd be a royal pita to get)
            var aliases = projects
                .Where(p =>
                    !p.IsPrivate
                    || p.OwnerId == userId
                    || p.Administrators.Any(a => a.UserId == userId)
                    || guildAdmins.Any(a => a.UserId == userId)
                    || p.KeyStaff.Any(ks => ks.UserId == userId)
                )
                .SelectMany(p => new[] { p.Nickname }.Concat(p.Aliases))
                .Where(a => a.StartsWith(query, StringComparison.InvariantCultureIgnoreCase))
                .Distinct()
                .Order()
                .ToList();

            choices.AddRange(aliases.Select(m => new AutocompleteResult(m, m)));
            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }

    /// <summary>
    /// Autocompletion for episode numbers
    /// </summary>
    public class EpisodeAutocompleteHandler(DataContext db) : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var focusedOption = interaction.Data.Current;

            List<AutocompleteResult> choices = [];
            var alias = (
                (string?)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")?.Value
            )?.Trim();
            if (alias is null)
                return AutocompletionResult.FromSuccess([]);

            var cachedProject = db.ResolveAlias(alias, interaction);
            if (cachedProject is null)
                return AutocompletionResult.FromSuccess([]);

            choices.AddRange(
                cachedProject
                    .Episodes.Where(e =>
                        e.Number.ToString()
                            .StartsWith(
                                (string)focusedOption.Value,
                                StringComparison.InvariantCultureIgnoreCase
                            )
                    )
                    .Select(e => new AutocompleteResult(e.Number.ToString(), e.Number))
            );
            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }

    /// <summary>
    /// Autocompletion for task abbreviations
    /// </summary>
    public class AbbreviationAutocompleteHandler(DataContext db) : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var focusedOption = interaction.Data.Current;

            List<AutocompleteResult> choices = [];
            var alias = (
                (string?)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")?.Value
            )?.Trim();
            var episodeInput = interaction
                .Data.Options.FirstOrDefault(o => o.Name == "episode")
                ?.Value;
            string? episodeNumber;
            if (alias is null)
                return AutocompletionResult.FromSuccess([]);

            var cachedProject = db.ResolveAlias(alias, interaction);
            if (cachedProject is null)
                return AutocompletionResult.FromSuccess([]);

            episodeNumber = episodeInput is null
                ? cachedProject.Episodes.FirstOrDefault(e => !e.Done)?.Number
                : Utils.CanonicalizeEpisodeNumber((string)episodeInput);

            if (episodeNumber is null)
                return AutocompletionResult.FromSuccess([]);

            var cachedEpisode = cachedProject.Episodes.FirstOrDefault(e =>
                e.Number == episodeNumber
            );
            if (cachedEpisode is null)
            {
                var value = ((string)focusedOption.Value).ToUpperInvariant();
                // Return list of key staff
                choices.AddRange(
                    cachedProject
                        .KeyStaff.Where(ks => ks.Role.Abbreviation.StartsWith(value))
                        .Select(t => new AutocompleteResult(
                            t.Role.Abbreviation,
                            t.Role.Abbreviation
                        ))
                );
            }
            else
            {
                var value = ((string)focusedOption.Value).ToUpperInvariant();
                // Return list of episode tasks
                choices.AddRange(
                    cachedEpisode
                        .Tasks.Where(t => t.Abbreviation.StartsWith(value))
                        .Select(t => new AutocompleteResult(t.Abbreviation, t.Abbreviation))
                );
            }
            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }

    /// <summary>
    /// Autocompletion for Key Staff
    /// </summary>
    public class KeyStaffAutocompleteHandler(DataContext db) : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var focusedOption = interaction.Data.Current;

            List<AutocompleteResult> choices = [];
            var alias = (
                (string?)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")?.Value
            )?.Trim();
            if (alias is null)
                return AutocompletionResult.FromSuccess([]);

            var cachedProject = db.ResolveAlias(alias, interaction);
            if (cachedProject is null)
                return AutocompletionResult.FromSuccess([]);

            var value = ((string)focusedOption.Value).ToUpperInvariant();
            // Return list of key staff
            choices.AddRange(
                cachedProject
                    .KeyStaff.Where(ks => ks.Role.Abbreviation.StartsWith(value))
                    .Select(t => new AutocompleteResult(t.Role.Abbreviation, t.Role.Abbreviation))
            );
            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }

    /// <summary>
    /// Autocompletion for Additional Staff
    /// </summary>
    public class AdditionalStaffAutocompleteHandler(DataContext db) : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var focusedOption = interaction.Data.Current;

            List<AutocompleteResult> choices = [];
            var alias = (
                (string?)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")?.Value
            )?.Trim();
            var episodeInput = (string?)
                interaction.Data.Options.FirstOrDefault(o => o.Name == "episode")?.Value;
            if (alias is null || episodeInput is null)
                return AutocompletionResult.FromSuccess([]);

            var episodeNumber = Utils.CanonicalizeEpisodeNumber(episodeInput);
            var cachedProject = db.ResolveAlias(alias, interaction);
            if (cachedProject is null)
                return AutocompletionResult.FromSuccess([]);

            var cachedEpisode = cachedProject.Episodes.FirstOrDefault(e =>
                e.Number == episodeNumber
            );
            if (cachedEpisode is null)
                return AutocompletionResult.FromSuccess([]);

            var value = ((string)focusedOption.Value).ToUpperInvariant();
            // Return list of additional staff
            choices.AddRange(
                cachedEpisode
                    .AdditionalStaff.Where(ks => ks.Role.Abbreviation.StartsWith(value))
                    .Select(t => new AutocompleteResult(t.Role.Abbreviation, t.Role.Abbreviation))
            );
            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }

    /// <summary>
    /// Autocompletion for Conga participants
    /// </summary>
    public class CongaNodesAutocompleteHandler(DataContext db) : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var focusedOption = interaction.Data.Current;
            var userId = interaction.User.Id;

            List<AutocompleteResult> choices = [];
            var alias = (
                (string?)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")?.Value
            )?.Trim();
            if (alias is null)
                return AutocompletionResult.FromSuccess([]);

            var cachedProject = db.ResolveAlias(alias, interaction);
            if (cachedProject is null)
                return AutocompletionResult.FromSuccess([]);
            if (!Utils.VerifyUser(userId, cachedProject))
                return AutocompletionResult.FromSuccess([]);

            var value = ((string)focusedOption.Value).ToUpperInvariant();

            // Return list of conga participants
            choices.AddRange(
                cachedProject
                    .CongaParticipants.GetEdges()
                    .Where(cn =>
                        cn.Current.StartsWith(value, StringComparison.InvariantCultureIgnoreCase)
                        || cn.Next.StartsWith(value, StringComparison.InvariantCultureIgnoreCase)
                    )
                    .Select(cn => new AutocompleteResult(cn.ToString(), cn.ToString()))
            );
            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }

    /// <summary>
    /// Autocompletion for Current Conga targets
    /// </summary>
    public class CongaCurrentAutocompleteHandler(DataContext db) : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var focusedOption = interaction.Data.Current;
            var userId = interaction.User.Id;

            List<AutocompleteResult> choices = [];
            var alias = (
                (string?)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")?.Value
            )?.Trim();
            if (alias is null)
                return AutocompletionResult.FromSuccess([]);

            var cachedProject = db.ResolveAlias(alias, interaction);
            if (cachedProject is null)
                return AutocompletionResult.FromSuccess([]);
            if (!Utils.VerifyUser(userId, cachedProject))
                return AutocompletionResult.FromSuccess([]);

            var value = ((string)focusedOption.Value).ToUpperInvariant();

            // Generate list of targets

            choices.AddRange(
                CongaGraph
                    .CurrentSpecials // Current specials
                    .Concat(cachedProject.KeyStaff.Select(ks => ks.Role.Abbreviation))
                    .Concat(
                        cachedProject
                            .Episodes.SelectMany(e => e.AdditionalStaff)
                            .Select(ks => ks.Role.Abbreviation)
                    )
                    .ToHashSet()
                    .Select(i => new AutocompleteResult(i, i))
            );

            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }

    /// <summary>
    /// Autocompletion for Next Conga targets
    /// </summary>
    public class CongaNextAutocompleteHandler(DataContext db) : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var focusedOption = interaction.Data.Current;
            var userId = interaction.User.Id;

            List<AutocompleteResult> choices = [];
            var alias = (
                (string?)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")?.Value
            )?.Trim();
            if (alias is null)
                return AutocompletionResult.FromSuccess([]);

            var cachedProject = db.ResolveAlias(alias, interaction);
            if (cachedProject is null)
                return AutocompletionResult.FromSuccess([]);
            if (!Utils.VerifyUser(userId, cachedProject))
                return AutocompletionResult.FromSuccess([]);

            var value = ((string)focusedOption.Value).ToUpperInvariant();

            // Generate list of targets

            choices.AddRange(
                CongaGraph
                    .NextSpecials // Next specials
                    .Concat(cachedProject.KeyStaff.Select(ks => ks.Role.Abbreviation))
                    .Concat(
                        cachedProject
                            .Episodes.SelectMany(e => e.AdditionalStaff)
                            .Select(ks => ks.Role.Abbreviation)
                    )
                    .ToHashSet()
                    .Select(i => new AutocompleteResult(i, i))
            );

            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }

    /// <summary>
    /// Autocompletion for project names/aliases
    /// </summary>
    public class LocaleAutocompleteHandler : AutocompleteHandler
    {
        private static readonly Dictionary<string, string> Options = Enum.GetValues<Locale>()
            .ToDictionary(locale => locale.ToFriendlyString(), locale => $"{(int)locale}");

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            var interaction = (SocketAutocompleteInteraction)context.Interaction;
            var focusedOption = interaction.Data.Current;

            List<AutocompleteResult> choices = [];
            var query = ((string)focusedOption.Value).Trim();
            choices.AddRange(
                Options
                    .Where(o =>
                        o.Key.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)
                    )
                    .Select(o => new AutocompleteResult(o.Key, o.Value))
            );
            return AutocompletionResult.FromSuccess(choices.Take(25));
        }
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
