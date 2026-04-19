// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Tasks.Edit;
using Nino.Core.Features.Queries.Episodes.Resolve;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Tasks;

public partial class TaskModule
{
    [SlashCommand("edit", "Edit an episode's task")]
    public async Task<RuntimeResult> EditAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Abbreviation), Autocomplete(typeof(ProjectTaskAutocompleteHandler))]
            Abbreviation abbreviation,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number firstEpisode,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number? lastEpisode = null,
        [MaxLength(Length.Abbreviation)] Abbreviation? newAbbreviation = null,
        [MaxLength(Length.RoleName)] string? name = null,
        SocketUser? assignee = null,
        decimal? weight = null,
        bool? isPseudo = null
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        name = name?.Trim();

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolve = await projectResolver
            .HandleAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
            .ThenAsync(prjId =>
                episodeResolver.HandleAsync(new ResolveEpisodeQuery(prjId, firstEpisode))
            );

        if (!resolve.IsSuccess)
        {
            return await interaction.FailAsync(
                resolve.Status,
                locale,
                new FailureContext { Alias = alias, Episode = firstEpisode }
            );
        }

        var (projectId, firstId) = resolve.Value;
        var lastId = firstId;

        // Resolve the last episode if needed
        if (lastEpisode is not null)
        {
            var lastResolve = await episodeResolver.HandleAsync(
                new ResolveEpisodeQuery(projectId, lastEpisode.Value)
            );
            if (!resolve.IsSuccess)
            {
                return await interaction.FailAsync(
                    resolve.Status,
                    locale,
                    new FailureContext { Alias = alias, Episode = lastEpisode.Value }
                );
            }
            lastId = lastResolve.Value;
        }

        var assigneeId = assignee is null
            ? null
            : (UserId?)
                await identityService.GetOrCreateUserByDiscordIdAsync(
                    assignee.Id,
                    assignee.Username
                );

        var command = new EditTaskCommand(
            ProjectId: projectId,
            FirstEpisodeId: firstId,
            LastEpisodeId: lastId,
            RequestedBy: requestedBy,
            Abbreviation: abbreviation,
            AssigneeId: assigneeId,
            NewAbbreviation: newAbbreviation,
            Name: name,
            Weight: weight,
            IsPseudo: isPseudo
        );

        var result = await editHandler
            .HandleAsync(command)
            .BindAsync(() =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
            );

        if (!result.IsSuccess)
        {
            return await interaction.FailAsync(
                result.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var pData = result.Value;
        var locKey = lastEpisode is null ? "task.edit.success" : "task.edit.success.range";

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(T(locKey, locale, abbreviation, firstEpisode, lastEpisode))
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
