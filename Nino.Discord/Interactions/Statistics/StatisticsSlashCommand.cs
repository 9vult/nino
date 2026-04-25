// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Features.Queries.Nino.Statistics;

namespace Nino.Discord.Interactions.Statistics;

public class StatisticsSlashCommand(NinoStatisticsHandler statsHandler)
    : InteractionModuleBase<IInteractionContext>
{
    [SlashCommand("statistics", "Nino Statistics")]
    public async Task<RuntimeResult> ShowStatsAsync()
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var statsRequest = await statsHandler.HandleAsync(new NinoStatisticsQuery());
        if (!statsRequest.IsSuccess)
            return ExecutionResult.Failure; // Should be impossible, the handler only returns success
        var stats = statsRequest.Value;

        var body = new StringBuilder();

        if (stats.TotalEpisodes == 0)
        {
            body.AppendLine(T("nino.stats.empty", locale));
        }
        else
        {
            var episodePercent = Math.Round(
                stats.CompletedEpisodes / (decimal)stats.TotalEpisodes * 100.0m,
                2
            );

            body.AppendLine(
                T("nino.stats.projects", locale, stats.TotalProjects, stats.TotalGroups)
            );
            body.AppendLine(T("nino.stats.episodes", locale, stats.TotalEpisodes, episodePercent));
            body.AppendLine(T("nino.stats.tasks", locale, stats.CompletedTasks));
            body.AppendLine(
                T("nino.stats.observers", locale, stats.TotalObservers, stats.ObserverProjectCount)
            );
        }

        await interaction.FollowupAsync(
            embed: new EmbedBuilder()
                .WithAuthor(
                    name: T("nino.stats.title", locale),
                    url: "https://github.com/9vult/nino"
                )
                .WithThumbnailUrl("https://files.catbox.moe/j3qizm.png")
                .WithDescription(body.ToString())
                .WithCurrentTimestamp()
                .Build()
        );
        return ExecutionResult.Success;
    }
}
