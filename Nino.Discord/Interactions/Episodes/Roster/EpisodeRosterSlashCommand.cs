// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Enums;
using Nino.Core.Features.Episodes.Roster;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.Episodes;

public partial class EpisodesModule
{
    [SlashCommand("roster", "See who's working on an episode")]
    public async Task<RuntimeResult> GenerateRosterAsync(
        [MaxLength(32)] string alias,
        [MaxLength(32)] string episodeNumber,
        bool withWeights = false
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        alias = alias.Trim();
        episodeNumber = episodeNumber.Trim();

        // Resolve user, group, and project
        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var (resolveStatus, projectId) = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy)
        );

        if (resolveStatus is not ResultStatus.Success)
            return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

        var commandDto = new EpisodeRosterCommand(projectId, episodeNumber, requestedBy);
        var result = await rosterHandler.HandleAsync(commandDto);

        if (result.Status != ResultStatus.Success)
        {
            return await interaction.FailAsync(
                result.Status switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T("error.episodeNotFound", locale),
                    _ => T("error.generic", locale),
                }
            );
        }

        var data = await dataService.GetProjectBasicInfoAsync(projectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        List<string> statuses = [];
        foreach (var task in result.Value!.Tasks.OrderBy(t => t.Weight))
        {
            var weight = withWeights ? $" ({task.Weight})" : string.Empty;
            var mention = $"<@{task.User.DiscordId}>";
            var abbreviation = task.IsDone
                ? $"~~{task.Abbreviation}~~"
                : $"**{task.Abbreviation}**";
            var pseudo = task.IsPseudo ? "†" : string.Empty;

            statuses.Add($"{abbreviation}{pseudo}: {mention}{weight}");
        }

        var roster =
            statuses.Count > 0
                ? string.Join(Environment.NewLine, statuses)
                : T("episode.roster.empty", locale);

        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("episode.roster.title", locale))
            .WithDescription(roster)
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
