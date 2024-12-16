using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public class Stats(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


        [SlashCommand("stats", "Nino Statistics")]
        public async Task<RuntimeResult> Handle()
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            
            Log.Trace($"Displaying Stats for M[{interaction.User.Id} (@{interaction.User.Username})]");
            
            var allowedOngoingProjects = Cache.GetProjects().Where(p => !p.IsArchived).Select(p => p.Id).ToList();
            var archivedCount = Cache.GetProjects().Count(p => p.IsArchived);
            
            var ongoing = Cache.GetEpisodes().GroupBy(e => e.ProjectId)
                .Where(g => g.Any(e => !e.Done) && allowedOngoingProjects.Contains(g.Key))
                .ToDictionary(g => g.Key, g => g.ToList());
            
            var guildCount = Cache.GetProjectGuilds().Count;
            var totalProjects = Cache.GetProjects().Count;
            var ongoingProjects = ongoing.Count;
            var totalEpisodes = Cache.GetEpisodes().Count;
            var totalDoneEpisodes = Cache.GetEpisodes().Count(ep => ep.Done);
            var ongoingProjectEpisodes = ongoing.Sum(kv => kv.Value.Count);
            var ongoingProjectDoneEpisodes = ongoing.Sum(kv => kv.Value.Count(ep => ep.Done));
            var observerCount = Cache.GetObservers().Count;
            var uniqueObservers = Cache.GetObservers().GroupBy(o => o.ProjectId).Count();
            
            var totalDoneEpisodesPercent = Math.Round(totalDoneEpisodes / (decimal)totalEpisodes * 100.0m, 2);
            var totalDoneOngoingProjectEpisodesPercent = Math.Round(ongoingProjectDoneEpisodes / (decimal)ongoingProjectEpisodes * 100.0m, 2);

            var embed = new EmbedBuilder()
                .WithTitle(T("title.stats", lng))
                .WithDescription(T("nino.stats", lng, 
                    totalProjects,
                    guildCount,
                    totalEpisodes,
                    totalDoneEpisodesPercent,
                    ongoingProjects,
                    archivedCount,
                    ongoingProjectEpisodes,
                    totalDoneOngoingProjectEpisodesPercent,
                    observerCount,
                    uniqueObservers
                ))
                .WithUrl("https://github.com/9vult/nino")
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
