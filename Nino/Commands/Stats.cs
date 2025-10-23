using System.Globalization;
using System.Text;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public class Stats(DataContext db) : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [SlashCommand("stats", "Nino Statistics")]
        public async Task<RuntimeResult> Handle()
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            Log.Trace(
                $"Displaying Stats for M[{interaction.User.Id} (@{interaction.User.Username})]"
            );

            var allowedOngoingProjects = await db
                .Projects.Where(p => !p.IsArchived)
                .Select(p => p.Id)
                .ToListAsync();
            var archivedCount = await db.Projects.CountAsync(p => p.IsArchived);

            var ongoingList = await (
                from e in db.Episodes
                join unfinished in db.Episodes on e.ProjectId equals unfinished.ProjectId
                where allowedOngoingProjects.Contains(e.ProjectId) && !unfinished.Done
                select e
            )
                .Distinct()
                .ToListAsync();

            var ongoing = ongoingList
                .GroupBy(e => e.ProjectId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var totalProjects = await db.Projects.CountAsync();

            if (totalProjects == 0)
            {
                await interaction.FollowupAsync(
                    embed: new EmbedBuilder()
                        .WithTitle(T("title.stats", lng))
                        .WithDescription(T("nino.stats.empty", lng))
                        .WithUrl("https://github.com/9vult/nino")
                        .Build()
                );
                return ExecutionResult.Success;
            }

            var totalGuilds = await db.Projects.GroupBy(p => p.GuildId).CountAsync();
            var ongoingProjects = ongoing.Count;
            var totalEpisodes = await db.Episodes.CountAsync();
            var totalDoneEpisodes = await db.Episodes.CountAsync(ep => ep.Done);
            var ongoingProjectEpisodes = ongoing.Sum(kv => kv.Value.Count);
            var ongoingProjectDoneEpisodes = ongoing.Sum(kv => kv.Value.Count(ep => ep.Done));
            var totalObservers = await db.Observers.CountAsync();
            var uniqueObservers = await db.Observers.GroupBy(o => o.ProjectId).CountAsync();
            var completedTasks = await db.Episodes.SelectMany(e => e.Tasks).CountAsync(e => e.Done);

            var totalDoneEpisodesPercent = Math.Round(
                totalDoneEpisodes / (decimal)totalEpisodes * 100.0m,
                2
            );
            var totalDoneOngoingProjectEpisodesPercent = Math.Round(
                ongoingProjectDoneEpisodes / (decimal)ongoingProjectEpisodes * 100.0m,
                2
            );

            var nfi = NumberFormatInfo.GetInstance(GetCultureInfo(lng));

            // String components
            var projectsTotalPart = T(
                "nino.stats.projects.total",
                lng,
                T(
                    "nino.stats.projects.total.projectCount",
                    lng,
                    PluralDict(totalProjects.ToString(nfi))
                ),
                T(
                    "nino.stats.projects.total.guildCount",
                    lng,
                    PluralDict(totalGuilds.ToString(nfi))
                )
            );
            var episodesTotalPart = T(
                "nino.stats.episodes.total",
                lng,
                T(
                    "nino.stats.episodes.total.episodeCount",
                    lng,
                    PluralDict(totalEpisodes.ToString(nfi))
                ),
                totalDoneEpisodesPercent.ToString(nfi)
            );
            var projectsDetailsPart = T(
                "nino.stats.projects.details",
                lng,
                T(
                    "nino.stats.projects.details.ongoingCount",
                    lng,
                    PluralDict(ongoingProjects.ToString(nfi))
                ),
                T(
                    "nino.stats.projects.details.archivedCount",
                    lng,
                    PluralDict(archivedCount.ToString(nfi))
                )
            );
            var episodesDetailsPart = T(
                "nino.stats.episodes.details",
                lng,
                T(
                    "nino.stats.episodes.details.ongoingCount",
                    lng,
                    PluralDict(ongoingProjectEpisodes.ToString(nfi))
                ),
                totalDoneOngoingProjectEpisodesPercent.ToString(nfi)
            );
            var observersPart = T(
                "nino.stats.observers.total",
                lng,
                T(
                    "nino.stats.observers.observerCount",
                    lng,
                    PluralDict(totalObservers.ToString(nfi))
                ),
                T(
                    "nino.stats.observers.projectCount",
                    lng,
                    PluralDict(uniqueObservers.ToString(nfi))
                )
            );
            var tasksPart = T("nino.stats.tasks.done", lng, completedTasks);

            var sb = new StringBuilder();
            sb.AppendLine(projectsTotalPart);
            sb.AppendLine(episodesTotalPart);
            sb.AppendLine(tasksPart);
            sb.AppendLine();
            sb.AppendLine(projectsDetailsPart);
            sb.AppendLine(episodesDetailsPart);
            sb.AppendLine();
            sb.AppendLine(observersPart);

            var embed = new EmbedBuilder()
                .WithTitle(T("title.stats", lng))
                .WithDescription(sb.ToString())
                .WithUrl("https://github.com/9vult/nino")
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }

        private static Dictionary<string, object> PluralDict(object value) =>
            new() { ["number"] = value };
    }
}
