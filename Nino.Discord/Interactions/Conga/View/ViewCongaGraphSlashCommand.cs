// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.Json;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Queries.Episodes.Resolve;
using Nino.Core.Features.Queries.Projects.Conga.GetDot;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Conga;

public partial class CongaModule
{
    [SlashCommand("view", "View a project's Conga graph")]
    public async Task<RuntimeResult> ViewAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number? episode = null
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolveProject = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy)
        );

        if (!resolveProject.IsSuccess)
        {
            return await interaction.FailAsync(
                resolveProject.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var projectId = resolveProject.Value;

        EpisodeId? episodeId = null;
        if (episode is not null)
        {
            var resolveEpisode = await episodeResolver.HandleAsync(
                new ResolveEpisodeQuery(projectId, episode.Value)
            );
            if (!resolveEpisode.IsSuccess)
            {
                return await interaction.FailAsync(
                    resolveProject.Status,
                    locale,
                    new FailureContext { Alias = alias, Episode = episode }
                );
            }
            episodeId = resolveEpisode.Value;
        }

        var query = new GetCongaDotQuery(projectId, episodeId);

        var result = await getDotHandler
            .HandleAsync(query)
            .ThenAsync(_ =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
            );

        var dot = result.Value.Item1;
        var pData = result.Value.Item2;

        // Success!
        var successEmbed = new EmbedBuilder().WithProjectInfo(pData, locale, includePoster: false);

        if (episode is not null)
            successEmbed = successEmbed.WithTitle(T("episode.title", locale, episode));

        if (!string.IsNullOrEmpty(dot))
        {
            var response = await httpClient.PostAsync(
                "https://quickchart.io/graphviz",
                new StringContent(
                    JsonSerializer.Serialize(
                        new
                        {
                            graph = dot,
                            layout = "dot",
                            format = "png",
                        }
                    ),
                    Encoding.UTF8,
                    "application/json"
                )
            );

            if (response.IsSuccessStatusCode)
            {
                using var stream = new MemoryStream(await response.Content.ReadAsByteArrayAsync());
                successEmbed = successEmbed.WithImageUrl("attachment://congo.png");
                await interaction.FollowupWithFileAsync(
                    stream,
                    "congo.png",
                    embed: successEmbed.Build()
                );
                return ExecutionResult.Success;
            }

            logger.LogError(
                "Failed to generate Conga image: {StatusCode}: {ReasonPhrase}",
                response.StatusCode,
                response.ReasonPhrase
            );
            successEmbed = successEmbed.WithDescription(
                $"Failed to generate Conga image: {response.StatusCode}: {response.ReasonPhrase}"
            );
        }
        else
        {
            successEmbed = successEmbed.WithDescription(T("conga.empty", locale));
        }

        await interaction.FollowupAsync(embed: successEmbed.Build());
        return ExecutionResult.Success;
    }
}
