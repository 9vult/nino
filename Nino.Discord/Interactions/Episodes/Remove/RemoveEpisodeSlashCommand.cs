// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Enums;
using Nino.Core.Features.Episodes.Remove;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.Episodes;

public partial class EpisodesModule
{
    [SlashCommand("remove", "Remove an episode")]
    public async Task<RuntimeResult> RemoveAsync(
        [MaxLength(32)] string alias,
        [MaxLength(32)] string episodeNumber,
        [MaxLength(32)] string? lastEpisodeNumber = null
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        alias = alias.Trim();
        episodeNumber = episodeNumber.Trim();
        lastEpisodeNumber = lastEpisodeNumber?.Trim();

        // Resolve user, group, and project
        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var (resolveStatus, projectId) = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy)
        );

        if (resolveStatus is not ResultStatus.Success)
            return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

        var commandDto = new RemoveEpisodeCommand(
            projectId,
            episodeNumber,
            lastEpisodeNumber ?? episodeNumber,
            requestedBy
        );
        var result = await removeHandler.HandleAsync(commandDto);

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
        var dict = new Dictionary<string, object>
        {
            ["number"] = result.Value,
            ["nickname"] = data.Nickname,
        };

        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(T("episode.delete.success", locale, dict))
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
