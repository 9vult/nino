// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Features.Queries.Tasks.AtMe;
using Nino.Core.Services;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Discord.Interactions.AtMe;

public sealed class AtMeNextButton(
    IStateService stateService,
    IIdentityService identityService,
    GetTasksAtMeHandler handler,
    ILogger<AtMeSlashCommand> logger
) : InteractionModuleBase<IInteractionContext>
{
    [ComponentInteraction("nino.atMe.next:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> GoToNextPageAsync(string rawId)
    {
        if (!StateId.TryParse(rawId, out var stateId))
        {
            logger.LogError("Could not parse state id: {StateId}", rawId);
            return ExecutionResult.Failure;
        }

        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var query = await stateService.LoadStateAsync<GetTasksAtMeQuery>(stateId);
        if (query is null)
            return await interaction.FailAsync(T("error.state", locale));

        // Verify button is not being hijacked
        if (
            await identityService.GetOrCreateUserByDiscordIdAsync(
                interaction.User.Id,
                interaction.User.Username
            ) != query.RequestedBy
        )
            return await interaction.FailAsync(T("error.hijack", locale), ephemeral: true);

        // Delete old state
        await stateService.DeleteStateAsync(stateId);

        // Move forward a page
        query = query with
        {
            Page = query.Page + 1,
        };

        logger.LogInformation(
            "Generating At Me page {Page} for user {User}",
            query.Page,
            query.RequestedBy
        );

        var request = await handler.HandleAsync(query);
        if (!request.IsSuccess)
            return await interaction.FailAsync(request.Status, locale);
        var data = request.Value;

        var body = new StringBuilder();

        if (data.Results.Count == 0)
        {
            body.AppendLine(T("atMe.empty", locale));
        }
        else
        {
            foreach (var entry in data.Results)
            {
                body.Append($"{entry.Nickname} ({entry.EpisodeNumber}): ");
                body.AppendLine(
                    string.Join(
                        ", ",
                        entry
                            .Tasks.OrderBy(t => t.Weight)
                            .Select(t => t.IsPseudo ? $"{t.Abbreviation}*" : t.Abbreviation.Value)
                    )
                );
            }
        }

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithTitle(T("atMe.title", locale, data.Type.ToFriendlyString(locale)))
            .WithDescription(body.ToString())
            .WithFooter(T("blameAll.footer", locale, data.Page + 1, data.PageCount))
            .WithCurrentTimestamp()
            .Build();

        // Buttons?
        ComponentBuilder? component = null;
        if (data.PageCount != 1)
        {
            stateId = await stateService.SaveStateAsync(query with { Page = data.Page });

            var prevId = $"nino.atMe.prev:{stateId}";
            var nextId = $"nino.atMe.next:{stateId}";

            var hasPrev = data.Page != 0;
            var hasNext = data.Page < data.PageCount - 1;

            component = new ComponentBuilder()
                .WithButton("◀", prevId, disabled: !hasPrev)
                .WithButton("▶", nextId, disabled: !hasNext);
        }

        await interaction.FollowupAsync(embed: successEmbed, components: component?.Build());
        return ExecutionResult.Success;
    }
}
