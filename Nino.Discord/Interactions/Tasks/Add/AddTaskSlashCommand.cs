// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Tasks.Add;
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
    [SlashCommand("add", "Add a task to an episode")]
    public async Task<RuntimeResult> AddAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Abbreviation)] Abbreviation abbreviation,
        [MaxLength(Length.RoleName)] string name,
        SocketUser assignee,
        bool isPseudo,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number firstEpisode,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number? lastEpisode = null
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        name = name.Trim();

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

        var assigneeId = await identityService.GetOrCreateUserByDiscordIdAsync(
            assignee.Id,
            assignee.Username
        );

        var command = new AddTaskCommand(
            ProjectId: projectId,
            FirstEpisodeId: firstId,
            LastEpisodeId: lastId,
            RequestedBy: requestedBy,
            AssigneeId: assigneeId,
            Abbreviation: abbreviation,
            Name: name,
            IsPseudo: isPseudo
        );

        var result = await addHandler
            .HandleAsync(command)
            .BindAsync(() =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
            );

        if (!result.IsSuccess)
        {
            return await interaction.FailAsync(
                result.Status,
                locale,
                new FailureContext { Alias = alias, Task = abbreviation }
            );
        }

        var pData = result.Value;
        var locKey = lastEpisode is null ? "task.creation.success" : "task.creation.success.range";

        // Success!
        var staffMention = $"<@{assignee.Id}>";
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(T(locKey, locale, staffMention, name, firstEpisode, lastEpisode))
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
