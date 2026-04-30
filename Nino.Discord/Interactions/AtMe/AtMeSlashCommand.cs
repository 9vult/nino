// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Features.Queries.Tasks.AtMe;
using Nino.Core.Services;
using Nino.Discord.Services;
using Nino.Domain.Enums;
using Nino.Localization;

namespace Nino.Discord.Interactions.AtMe;

public class AtMeSlashCommand(
    IInteractionIdentityService interactionIdService,
    IStateService stateService,
    GetTasksAtMeHandler handler,
    ILogger<AtMeSlashCommand> logger
) : InteractionModuleBase<IInteractionContext>
{
    [SlashCommand("atme", "View tasks At Me")]
    public async Task<RuntimeResult> GetTasksAtMeAsync(
        AtMeType type = AtMeType.Auto,
        bool global = false,
        bool excludePseudo = false
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        logger.LogInformation("Generating At Me for user {UserId}", requestedBy);

        var query = new GetTasksAtMeQuery(requestedBy, groupId, type, global, !excludePseudo, null);
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
            var stateId = await stateService.SaveStateAsync(query with { Page = data.Page });

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
