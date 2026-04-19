// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Episodes.Add;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;
using RuntimeResult = Discord.Interactions.RuntimeResult;

namespace Nino.Discord.Interactions.Episodes;

public partial class EpisodesModule
{
    [SlashCommand("add", "Add episodes to a project")]
    public async Task<RuntimeResult> AddAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Number)] Number first,
        int count = 1,
        [MaxLength(Length.Number)] string format = "$"
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Setup
        format = format.Trim();

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolve = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy)
        );

        if (!resolve.IsSuccess)
        {
            return await interaction.FailAsync(
                resolve.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var projectId = resolve.Value;

        var command = new AddEpisodeCommand(
            ProjectId: projectId,
            RequestedBy: requestedBy,
            First: first,
            Count: count,
            Format: format
        );

        var result = await addHandler
            .HandleAsync(command)
            .ThenAsync(_ =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
            );

        // Not using the helper because of the high complexity of the responses
        if (!result.IsSuccess)
        {
            var key = result.Status switch
            {
                ResultStatus.Unauthorized => "error.permissions",
                ResultStatus.ProjectNotFound => "project.notFound",
                ResultStatus.EpisodeConflict when count == 1 => "episode.creation.conflict",
                ResultStatus.EpisodeConflict when count > 1 => "episode.creation.conflict.many",
                ResultStatus.BadRequest when result.Message == "not-number" =>
                    "episode.creation.error.notNumber",
                ResultStatus.BadRequest when result.Message == "bad-format" =>
                    "episode.creation.error.badFormat",
                ResultStatus.BadRequest when result.Message == "too-long" =>
                    "episode.creation.error.tooLong",
                _ => "error.generic",
            };
            var args = new Dictionary<string, object> { ["alias"] = alias, ["episode"] = first };
            var embed = new EmbedBuilder()
                .WithTitle("Baka.")
                .WithDescription(T(key, locale, args))
                .WithColor(0xd797ff)
                .Build();
            await interaction.FollowupAsync(embed: embed, ephemeral: false);
            return ExecutionResult.Failure;
        }

        var newEpisodeCount = result.Value.Item1.AddedEpisodeCount;
        var pData = result.Value.Item2;

        var responseArgs = new Dictionary<string, object> { ["number"] = newEpisodeCount };

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(T("episode.creation.success", locale, responseArgs))
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
