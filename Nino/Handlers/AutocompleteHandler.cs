using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Localizer;
using Microsoft.EntityFrameworkCore;
using NaturalSort.Extension;
using Nino.Records;
using Nino.Utilities.Extensions;

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

            var baseQuery = db
                .Projects.Include(p => p.Episodes)
                .Where(p =>
                    p.GuildId == guildId
                    || (includeObservers && p.Observers.Any(o => o.GuildId == guildId))
                );

            if (!includeArchived)
            {
                baseQuery = baseQuery.Where(p => !p.IsArchived);
            }

            if (guildAdmins.All(a => a.UserId != userId)) // Not a guild admin
            {
                baseQuery = baseQuery.Where(p =>
                    !p.IsPrivate
                    || p.OwnerId == userId
                    || p.Administrators.Any(a => a.UserId == userId)
                    || p.KeyStaff.Any(s => s.UserId == userId)
                );
            }

            baseQuery = baseQuery.Where(p =>
                p.Nickname.StartsWith(query) || p.Aliases.Any(a => a.Value.StartsWith(query))
            );

            var projects = await baseQuery.ToListAsync();

            var aliases = projects
                .SelectMany(p => new[] { p.Nickname }.Concat(p.Aliases.Select(a => a.Value)))
                .Where(a => a.StartsWith(query, StringComparison.InvariantCultureIgnoreCase))
                .Distinct()
                .Take(25)
                .ToList();

            choices.AddRange(aliases.Select(m => new AutocompleteResult(m, m)));
            return AutocompletionResult.FromSuccess(choices);
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

            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return AutocompletionResult.FromSuccess([]);

            choices.AddRange(
                project
                    .Episodes.Where(e =>
                        e.Number.ToString()
                            .StartsWith(
                                (string)focusedOption.Value,
                                StringComparison.InvariantCultureIgnoreCase
                            )
                    )
                    .OrderBy(e => e.Number, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
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
            if (alias is null)
                return AutocompletionResult.FromSuccess([]);

            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return AutocompletionResult.FromSuccess([]);

            var episodeNumber = episodeInput is null
                ? project.Episodes.FirstOrDefault(e => !e.Done)?.Number
                : Episode.CanonicalizeEpisodeNumber((string)episodeInput);

            if (episodeNumber is null)
                return AutocompletionResult.FromSuccess([]);

            var value = (string)focusedOption.Value;
            var episode = project.Episodes.FirstOrDefault(e => e.Number == episodeNumber);
            if (episode is null)
            {
                // Return list of key staff
                choices.AddRange(
                    project
                        .KeyStaff.Where(ks =>
                            ks.Role.Abbreviation.StartsWith(
                                value,
                                StringComparison.InvariantCultureIgnoreCase
                            )
                        )
                        .Select(t => new AutocompleteResult(
                            t.Role.Abbreviation,
                            t.Role.Abbreviation
                        ))
                );
            }
            else
            {
                // Return list of episode tasks
                choices.AddRange(
                    episode
                        .Tasks.Where(t =>
                            t.Abbreviation.StartsWith(
                                value,
                                StringComparison.InvariantCultureIgnoreCase
                            )
                        )
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

            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return AutocompletionResult.FromSuccess([]);

            // Return list of key staff
            var value = (string)focusedOption.Value;
            choices.AddRange(
                project
                    .KeyStaff.Where(ks =>
                        ks.Role.Abbreviation.StartsWith(
                            value,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    )
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

            var episodeNumber = Episode.CanonicalizeEpisodeNumber(episodeInput);
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return AutocompletionResult.FromSuccess([]);

            var episode = project.Episodes.FirstOrDefault(e => e.Number == episodeNumber);
            if (episode is null)
                return AutocompletionResult.FromSuccess([]);

            var value = ((string)focusedOption.Value);
            // Return list of additional staff
            choices.AddRange(
                episode
                    .AdditionalStaff.Where(ks =>
                        ks.Role.Abbreviation.StartsWith(
                            value,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    )
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

            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return AutocompletionResult.FromSuccess([]);
            if (!project.VerifyUser(db, userId))
                return AutocompletionResult.FromSuccess([]);

            // Return list of conga participants
            var value = (string)focusedOption.Value;
            choices.AddRange(
                project
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

            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return AutocompletionResult.FromSuccess([]);
            if (!project.VerifyUser(db, userId))
                return AutocompletionResult.FromSuccess([]);

            // Generate list of targets
            var value = (string)focusedOption.Value;
            choices.AddRange(
                CongaGraph
                    .CurrentSpecials // Current specials
                    .Concat(project.KeyStaff.Select(ks => ks.Role.Abbreviation))
                    .Concat(
                        project
                            .Episodes.SelectMany(e => e.AdditionalStaff)
                            .Select(ks => ks.Role.Abbreviation)
                    )
                    .Where(t => t.StartsWith(value, StringComparison.InvariantCultureIgnoreCase))
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

            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return AutocompletionResult.FromSuccess([]);
            if (!project.VerifyUser(db, userId))
                return AutocompletionResult.FromSuccess([]);

            // Generate list of targets
            var value = (string)focusedOption.Value;
            choices.AddRange(
                CongaGraph
                    .NextSpecials // Next specials
                    .Concat(project.KeyStaff.Select(ks => ks.Role.Abbreviation))
                    .Concat(
                        project
                            .Episodes.SelectMany(e => e.AdditionalStaff)
                            .Select(ks => ks.Role.Abbreviation)
                    )
                    .Where(t => t.StartsWith(value, StringComparison.InvariantCultureIgnoreCase))
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
