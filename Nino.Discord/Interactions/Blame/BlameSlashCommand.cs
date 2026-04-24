// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Queries.Episodes.Blame;
using Nino.Core.Features.Queries.Episodes.Resolve;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Discord.Services;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Blame;

public sealed class BlameSlashCommand(
    IInteractionIdentityService interactionIdService,
    GetGenericProjectDataHandler getProjectDataHandler,
    ResolveProjectHandler projectResolver,
    ResolveEpisodeHandler episodeResolver,
    BlameHandler blameHandler
) : InteractionModuleBase<IInteractionContext>
{
    [SlashCommand("blame", "Check the status of a project")]
    public async Task<RuntimeResult> BlameAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number? episodeNumber = null,
        bool explain = false,
        bool includePseudo = false
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var projectResolve = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy, IncludeObservers: true)
        );

        if (!projectResolve.IsSuccess)
        {
            return await interaction.FailAsync(
                projectResolve.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var projectId = projectResolve.Value;

        EpisodeId? episodeId = null;
        if (episodeNumber.HasValue)
        {
            var episodeResolve = await episodeResolver.HandleAsync(
                new ResolveEpisodeQuery(projectId, episodeNumber.Value)
            );
            if (!episodeResolve.IsSuccess)
            {
                return await interaction.FailAsync(
                    episodeResolve.Status,
                    locale,
                    new FailureContext { Alias = alias, Episode = episodeNumber }
                );
            }
            episodeId = episodeResolve.Value;
        }

        var command = new BlameQuery(
            ProjectId: projectId,
            EpisodeId: episodeId,
            IncludePseudo: includePseudo,
            RequestedBy: requestedBy
        );

        var result = await blameHandler
            .HandleAsync(command)
            .ThenAsync(_ =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
            );

        if (!result.IsSuccess)
        {
            return await interaction.FailAsync(
                result.Status,
                locale,
                new FailureContext
                {
                    Alias = alias,
                    Episode = episodeNumber,
                    Overrides = new Dictionary<ResultStatus, string>
                    {
                        [ResultStatus.BadRequest] = "blame.allComplete",
                    },
                }
            );
        }

        var bData = result.Value.Item1;
        var pData = result.Value.Item2;

        var b = new StringBuilder();
        if (!explain)
        {
            foreach (var task in bData.Statuses.OrderBy(t => t.Weight))
            {
                if (task.IsDone)
                    b.Append($"~~{task.Abbreviation}~~ ");
                else
                    b.Append($"**{task.Abbreviation}** ");
            }
            b.AppendLine(); // Adds newline to the end
        }
        else
        {
            foreach (var task in bData.Statuses.OrderBy(t => t.Weight))
            {
                if (task.IsDone)
                    b.AppendLine($"~~{task.Name}~~");
                else
                    b.AppendLine($"**{task.Name}**");
            }
        }

        if (bData.UpdatedAt is not null && bData.Statuses.Any(t => t.IsDone))
        {
            var relativeTime = $"<t:{bData.UpdatedAt.Value.ToUnixTimeSeconds()}:R>";
            b.AppendLine();
            b.AppendLine(T("blame.updated", locale, relativeTime));
        }
        else if (bData.AiredAt is not null)
        {
            var relativeTime = $"<t:{bData.AiredAt.Value.ToUnixTimeSeconds()}:R>";
            var key = bData.AiredAt > DateTimeOffset.UtcNow ? "blame.airs" : "blame.aired";
            b.AppendLine();
            b.AppendLine(T(key, locale, relativeTime));
        }

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(
                !bData.IsSingleEpisodeMovie
                    ? T("episode.title", locale, bData.EpisodeNumber)
                    : string.Empty
            )
            .WithDescription(b.ToString().TrimEnd())
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
