using Discord;
using Discord.WebSocket;
using Nino.Utilities;

namespace Nino.Listeners
{
    internal static partial class Listener
    {
        public static async Task AutocompleteExecuted(SocketAutocompleteInteraction interaction)
        {
            var commandName = interaction.Data.CommandName;
            var focusedOption = interaction.Data.Current;
            var guildId = interaction.GuildId ?? 0;
            var userId = interaction.User.Id;

            List<AutocompleteResult> choices = [];

            switch (focusedOption.Name)
            {
                case "project":
                    {
                        if (guildId == 0) break; // return []
                        var alias = ((string)focusedOption.Value).Trim();
                        if (alias == null) break; // return []

                        choices.AddRange(Getters.GetFilteredAliases(guildId, userId, (string)focusedOption.Value)
                            .Select(m => new AutocompleteResult(m, m))
                        );
                    }
                    break;

                case "episode":
                case "start_episode":
                case "end_episode":
                    {
                        var alias = ((string?)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")?.Value)?.Trim();
                        if (alias == null) break; // return []

                        var cachedProject = Utils.ResolveCachedAlias(alias, interaction);
                        if (cachedProject != null)
                        {
                            
                            choices.AddRange(Cache.GetEpisodes(cachedProject.Id)
                                .Where(e => e.Number.ToString().StartsWith((string)focusedOption.Value))
                                .Select(e => new AutocompleteResult(e.Number.ToString(), e.Number))
                            );
                        }

                        if (commandName == "blame")
                        {
                            // TODO: Add observing projects
                        }
                    }
                    break;

                case "abbreviation":
                case "next":
                    {
                        var alias = ((string?)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")?.Value)?.Trim();
                        var episodeInput = (decimal?)interaction.Data.Options.FirstOrDefault(o => o.Name == "episode")?.Value;
                        if (alias == null) break; // return []

                        var cachedProject = Utils.ResolveCachedAlias(alias, interaction);
                        if (cachedProject == null) break; // return []
                        var cachedEpisode = Cache.GetEpisodes(cachedProject.Id).FirstOrDefault(e => e.Number == episodeInput);
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
                    break;
            }
            await interaction.RespondAsync(choices.Take(25));
        }
    }
}
