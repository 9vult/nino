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
        private static readonly Logger log = LogManager.GetCurrentClassLogger();


        [SlashCommand("stats", "Nino Statistics")]
        public async Task<RuntimeResult> Handle()
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            
            var guildCount = Cache.GetProjectGuilds().Count;
            var projectCount = Cache.GetProjects().Count;
            var episodeCount = Cache.GetEpisodes().Count;
            var observerCount = Cache.GetObservers().Count;
            var uniqueObservers = Cache.GetObservers().GroupBy(o => o.ProjectId).Count();
            var completedPercentage = Math.Round(Cache.GetEpisodes().Count(e => !e.Done) / (decimal)episodeCount * 100.0m, 2);

            var embed = new EmbedBuilder()
                .WithTitle(T("title.stats", lng))
                .WithDescription(T("nino.stats", lng, 
                    projectCount, guildCount, episodeCount,
                    completedPercentage, observerCount, uniqueObservers))
                .WithUrl("https://github.com/9vult/nino")
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
