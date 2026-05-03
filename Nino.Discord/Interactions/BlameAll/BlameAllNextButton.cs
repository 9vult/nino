// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using NaturalSort.Extension;
using Nino.Core.Features;
using Nino.Core.Features.Queries.Episodes.BlameAll;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Services;
using Nino.Discord.Entities;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.BlameAll;

public class BlameAllNextButton(
    IIdentityService identityService,
    IStateService stateService,
    GetGenericProjectDataHandler getProjectDataHandler,
    BlameAllHandler blameHandler,
    ILogger<BlameAllNextButton> logger
) : InteractionModuleBase<IInteractionContext>
{
    [ComponentInteraction("nino.blameAll.next:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> GoToNextPageAsync(string rawId)
    {
        if (!StateId.TryParse(rawId, out var stateId))
        {
            logger.LogError("Could not parse state id: {StateId}", rawId);
            return ExecutionResult.Failure;
        }

        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var command = await stateService.LoadStateAsync<BlameAllQuery>(stateId);
        if (command is null)
            return await interaction.FailAsync(T("error.state", locale));

        // Verify button is not being hijacked
        if (
            await identityService.GetOrCreateUserByDiscordIdAsync(
                interaction.User.Id,
                interaction.User.Username
            ) != command.RequestedBy
        )
            return await interaction.FailAsync(T("error.hijack", locale), ephemeral: true);

        // Delete old state
        await stateService.DeleteStateAsync(stateId);

        // Move forward a page
        command = command with
        {
            Page = command.Page + 1,
        };

        logger.LogInformation(
            "Generating Blame All page {Page} for project {ProjectId} for user {User}",
            command.Page,
            command.ProjectId,
            command.RequestedBy
        );

        var result = await blameHandler
            .HandleAsync(command)
            .ThenAsync(_ =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(command.ProjectId))
            );

        if (!result.IsSuccess)
        {
            return await interaction.FailAsync(result.Status, locale, new FailureContext());
        }

        var bData = result.Value.Item1;
        var pData = result.Value.Item2;

        var b = new StringBuilder();

        foreach (
            var episode in bData.Episodes.OrderBy(
                e => e.EpisodeNumber.Value,
                StringComparer.OrdinalIgnoreCase.WithNaturalSort()
            )
        )
        {
            b.Append($"{episode.EpisodeNumber}: ");
            if (episode.Statuses.All(t => !t.IsDone))
            {
                if (episode.AiredAt is not null && episode.AiredAt.Value > DateTimeOffset.UtcNow)
                    b.AppendLine('*' + T("blameAll.notAired", locale) + '*');
                else
                    b.AppendLine('*' + T("blameAll.notStarted", locale) + '*');
                continue;
            }

            foreach (var task in episode.Statuses.OrderBy(t => t.Weight))
            {
                if (task.IsDone)
                    b.Append($"~~{task.Abbreviation}~~ ");
                else
                    b.Append($"**{task.Abbreviation}** ");
            }
            b.AppendLine(); // Adds newline to the end
        }

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("blameAll.title", locale))
            .WithDescription(b.ToString().TrimEnd())
            .WithFooter(T("blameAll.footer", locale, bData.Page + 1, bData.PageCount))
            .Build();

        // New state
        stateId = await stateService.SaveStateAsync(command with { Page = bData.Page });

        var prevId = $"nino.blameAll.prev:{stateId}";
        var nextId = $"nino.blameAll.next:{stateId}";

        var hasPrev = bData.Page != 0;
        var hasNext = bData.Page < bData.PageCount - 1;

        var component = new ComponentBuilder()
            .WithButton("◀", prevId, disabled: !hasPrev)
            .WithButton("▶", nextId, disabled: !hasNext)
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = successEmbed;
            m.Components = component;
        });
        return ExecutionResult.Success;
    }
}
